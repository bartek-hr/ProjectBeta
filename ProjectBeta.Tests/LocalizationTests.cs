namespace ProjectBeta.Tests;

[TestClass]
public class LocalizationTests
{
    [TestInitialize]
    public void Setup()
    {
        SetLocale("en-GB");
    }

    [TestCleanup]
    public void Cleanup()
    {
        SetLocale("en-GB");
    }

    [TestMethod]
    public void L10n_ExistingEnglishKey_ReturnsEnglishValue()
    {
        Assert.AreEqual("Project Beta", l10n("app.title"));
    }

    [TestMethod]
    public void L10n_ExplicitLocaleOverride_ReturnsLocaleSpecificValue()
    {
        Assert.AreEqual("Gebruikersnaam", l10n("login.username", "nl-NL"));
    }

    [TestMethod]
    public void L10n_WithReplacements_InterpolatesPlaceholders()
    {
        var replacements = new Dictionary<string, string>
        {
            ["name"] = "dayle"
        };

        Assert.AreEqual("Welcome, dayle!", l10n("messages.welcome", replacements));
    }

    [TestMethod]
    public void L10n_WithCapitalizedPlaceholder_CapitalizesReplacement()
    {
        var replacements = new Dictionary<string, string>
        {
            ["name"] = "dayle"
        };

        Assert.AreEqual("Welcome, Dayle!", l10n("messages.title_welcome", replacements));
    }

    [TestMethod]
    public void L10n_WithUppercasePlaceholder_UppercasesReplacement()
    {
        var replacements = new Dictionary<string, string>
        {
            ["name"] = "dayle"
        };

        Assert.AreEqual("Welcome, DAYLE!", l10n("messages.loud_welcome", replacements));
    }

    [TestMethod]
    public void L10n_WithReplacementsAndLocale_UsesLocaleSpecificString()
    {
        var replacements = new Dictionary<string, string>
        {
            ["name"] = "dayle"
        };

        Assert.AreEqual("Welkom, dayle!", l10n("messages.welcome", replacements, "nl-NL"));
    }

    [TestMethod]
    public void L10n_MissingKeyInRequestedLocale_FallsBackToEnglish()
    {
        Assert.AreEqual("Welcome", l10n("home.welcome", "nl-NL"));
    }

    [TestMethod]
    public void SetLocale_ChangesActiveLocaleUsedByLookups()
    {
        SetLocale("nl-NL");

        Assert.AreEqual("Gebruikersnaam", l10n("login.username"));
    }

    [TestMethod]
    public void L10n_MissingLocaleFile_FallsBackToEnglish()
    {
        Assert.AreEqual("Project Beta", l10n("app.title", "fr-FR"));
    }

    [TestMethod]
    public void L10n_SimplePluralization_UsesSingularForCountOne()
    {
        var replacements = new Dictionary<string, string>
        {
            ["count"] = "1"
        };

        Assert.AreEqual("car", l10n("inventory.car_simple", replacements));
    }

    [TestMethod]
    public void L10n_SimplePluralization_UsesPluralForCountGreaterThanOne()
    {
        var replacements = new Dictionary<string, string>
        {
            ["count"] = "3"
        };

        Assert.AreEqual("cars", l10n("inventory.car_simple", replacements));
    }

    [TestMethod]
    public void L10n_ExplicitPluralRules_SelectMatchingExactRule()
    {
        var replacements = new Dictionary<string, string>
        {
            ["count"] = "0"
        };

        Assert.AreEqual("no cars", l10n("inventory.car_detailed", replacements));
    }

    [TestMethod]
    public void L10n_ExplicitPluralRules_SelectMatchingRangeRuleAndReplaceCount()
    {
        var replacements = new Dictionary<string, string>
        {
            ["count"] = "5"
        };

        Assert.AreEqual("5 cars", l10n("inventory.car_detailed", replacements));
    }

    [TestMethod]
    public void L10n_Pluralization_FallsBackToEnglishWhenLocaleKeyMissing()
    {
        var replacements = new Dictionary<string, string>
        {
            ["count"] = "2"
        };

        Assert.AreEqual("2 minutes ago", l10n("inventory.minutes_ago", replacements, "nl-NL"));
    }

    [TestMethod]
    public void L10n_MissingKeyInAllLocales_ReturnsKey()
    {
        const string key = "missing.key";

        Assert.AreEqual(key, l10n(key, "nl-NL"));
    }

    [TestMethod]
    public void L10n_ExplicitLocaleOverride_DoesNotChangeActiveLocale()
    {
        SetLocale("en-GB");

        _ = l10n("login.username", "nl-NL");

        Assert.AreEqual("Username", l10n("login.username"));
    }
}
