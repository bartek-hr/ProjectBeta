using System;
using System.Collections.Generic;
using ProjectBeta.Model;
using ProjectBeta.Access;

namespace ProjectBeta.Logic;

public class ReceiptLogic
{
    private readonly ReceiptAccess _receiptAccess;

    public ReceiptLogic(ReceiptAccess receiptAccess)
    {
        _receiptAccess = receiptAccess;
    }

    public List<Receipt> GetReceipts()
    {
        return _receiptAccess.GetAll();
    }

    public Receipt GetReceipt(int id)
    {
        var receipt = _receiptAccess.GetById(id);

        if (receipt == null)
            throw new Exception("receipt not found");

        return receipt;
    }

    public void CreateReceipt(Receipt receipt)
    {
        if (receipt.Total <= 0)
            throw new Exception("Total price must be greater than 0");

        if (receipt.Booking_ID <= 0)
            throw new Exception("Invalid Booking");


        receipt.CreatedAt = DateTime.Now;

           

        _receiptAccess.Add(receipt);
    }

    public void MarkAsPaid(int receiptId)
    {
        var receipt = _receiptAccess.GetById(receiptId);

        if (receipt == null)
            throw new Exception("Receipt not found");


        _receiptAccess.Update(receipt);
    }

    public void DeleteReceipt(int id)
    {
        _receiptAccess.Delete(id);
    }
}