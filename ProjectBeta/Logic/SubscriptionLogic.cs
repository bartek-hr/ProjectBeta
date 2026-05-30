using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Access;
using ProjectBeta.Model;
using ProjectBeta.Data;
using ProjectBeta.Localization;
using static ProjectBeta.Localization.Localizer;

namespace ProjectBeta.Logic
{
    public class SubscriptionLogic
    {
        private readonly SubscriptionAccess _access;
        public void AddSubscription(Subscription subscription)
        {
            _access.AddSubscription(subscription);
        }

        public void RemoveSubscription(int id)
        {
            _access.RemoveSubscription(id);
        }

        public List<Subscription> GetAvailableSubscriptionsWithSeatPrice()
        {
            return _context.Subscriptions
                .Include(s => s.SeatPrice)
                .Include(s => s.UserSubscriptions)
                .Where(s => s.IsActive)
                .ToList();
        }
        private readonly AppDbContext _context;
        public SubscriptionLogic(AppDbContext context)
        {
            _context = context;
            _access = new SubscriptionAccess(context);
        }

        public List<Subscription> GetAvailableSubscriptions()
        {
            return _access.GetActiveSubscriptions();
        }

        public bool UserHasActiveSubscription(int userId)
        {
            return _context.UserSubscriptions.Any(us => us.UserId == userId && us.IsActive);
        }

        public void BuySubscription(int userId, int subscriptionId, bool isShared = false, int? sharedWithUserId = null)
        {
            if (UserHasActiveSubscription(userId))
                throw new InvalidOperationException(l10n("user.subscriptions.errors.already_has_subscription"));
            var sub = _access.GetSubscriptionById(subscriptionId);
            if (sub == null || !sub.IsActive)
                throw new InvalidOperationException(l10n("user.subscriptions.errors.subscription_not_found"));
            var userSub = new UserSubscription
            {
                UserId = userId,
                SubscriptionId = subscriptionId,
                IsActive = true,
                IsConnected = isShared,
                ConnectedWithUserId = isShared ? sharedWithUserId : null,
                StartDate = DateTime.Now
            };
            _context.UserSubscriptions.Add(userSub);
            _context.SaveChanges();
        }

        public void CancelSubscription(int userId)
        {
            var userSub = _context.UserSubscriptions.FirstOrDefault(us => us.UserId == userId && us.IsActive);
            if (userSub == null) return;

            if (userSub.IsConnected && userSub.ConnectedWithUserId.HasValue)
            {
                var otherUserSub = _context.UserSubscriptions.FirstOrDefault(us =>
                    us.UserId == userSub.ConnectedWithUserId.Value &&
                    us.IsActive &&
                    us.ConnectedWithUserId == userId);
                if (otherUserSub != null)
                {
                    otherUserSub.IsConnected = false;
                    otherUserSub.ConnectedWithUserId = null;
                }
            }

            userSub.IsActive = false;
            userSub.EndDate = DateTime.Now;
            _context.SaveChanges();
        }

        public enum FriendCheckResult { NotFound, NoSubscription, HasSubscription }

        public FriendCheckResult CheckFriendSubscription(int subscriptionId, string otherEmail)
        {
            var otherUser = _context.Users.FirstOrDefault(u => u.Email == otherEmail);
            if (otherUser == null) return FriendCheckResult.NotFound;
            return _context.UserSubscriptions.Any(us =>
                us.UserId == otherUser.Id &&
                us.SubscriptionId == subscriptionId &&
                us.IsActive)
                ? FriendCheckResult.HasSubscription
                : FriendCheckResult.NoSubscription;
        }

        public void ConnectSubscription(int userId, string otherEmail)
        {
            if (string.IsNullOrWhiteSpace(otherEmail))
                throw new InvalidOperationException(l10n("user.subscriptions.errors.email_required"));

            var otherUser = _context.Users.FirstOrDefault(u => u.Email == otherEmail);
            if (otherUser == null)
                throw new InvalidOperationException(l10n("user.subscriptions.errors.user_not_found"));
            if (otherUser.Id == userId)
                throw new InvalidOperationException(l10n("user.subscriptions.errors.cannot_connect_self"));

            var mySub = _context.UserSubscriptions
                .Include(us => us.Subscription)
                .FirstOrDefault(us => us.UserId == userId && us.IsActive);
            if (mySub == null)
                throw new InvalidOperationException(l10n("user.subscriptions.errors.no_active_subscription"));
            if (!mySub.Subscription.IsConnectAllowed)
                throw new InvalidOperationException(l10n("user.subscriptions.errors.connecting_not_allowed"));

            var theirSub = _context.UserSubscriptions
                .FirstOrDefault(us => us.UserId == otherUser.Id && us.IsActive && us.SubscriptionId == mySub.SubscriptionId);
            if (theirSub == null)
                throw new InvalidOperationException(l10n("user.subscriptions.errors.other_user_no_subscription"));

            mySub.IsConnected = true;
            mySub.ConnectedWithUserId = otherUser.Id;
            theirSub.IsConnected = true;
            theirSub.ConnectedWithUserId = userId;
            _context.SaveChanges();
        }

        public string? GetUserEmail(int userId)
        {
            return _context.Users.FirstOrDefault(u => u.Id == userId)?.Email;
        }

        public SubscriptionPricingContext? GetActiveSubscriptionPricingInfo(int userId)
        {
            var userSub = _context.UserSubscriptions
                .Include(us => us.Subscription)
                    .ThenInclude(s => s.SeatPrice)
                .FirstOrDefault(us => us.UserId == userId && us.IsActive);

            if (userSub == null) return null;

            return new SubscriptionPricingContext(
                userSub.Subscription.ApplicableDayOfWeek,
                userSub.Subscription.SeatPrice.Price
            );
        }
    }
}