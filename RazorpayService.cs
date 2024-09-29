using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using System;
using System.Collections.Generic;

public class RazorpayService
{
    private readonly string _keyId;
    private readonly string _keySecret;

    public RazorpayService(IConfiguration configuration)
    {
        _keyId = configuration["Razorpay:KeyId"];
        _keySecret = configuration["Razorpay:KeySecret"];
    }

    // Create a new payment order
    public Order CreateOrder(decimal amount, string currency)
    {
        // Convert amount to paise as Razorpay uses the smallest currency unit
        var amountInPaise = amount * 100;
        Dictionary<string, object> options = new Dictionary<string, object>
        {
            { "amount", amountInPaise },
            { "currency", currency },
            { "receipt", Guid.NewGuid().ToString() },
            { "payment_capture", 1 } 
        };

        // Create a Razorpay client instance
        RazorpayClient client = new RazorpayClient(_keyId, _keySecret);
        Order order = client.Order.Create(options); // Create order
        return order;
    }

    // Verify the payment using Razorpay signature
    public bool VerifyPayment(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature)
    {
        var attributes = new Dictionary<string, string>
        {
            { "razorpay_order_id", razorpayOrderId },
            { "razorpay_payment_id", razorpayPaymentId },
            { "razorpay_signature", razorpaySignature }
        };

        try
        {
            // Razorpay SDK verifies payment signature
            Utils.verifyPaymentSignature(attributes);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    // Capture the payment (if payment_capture is not enabled during order creation)
    public Payment CapturePayment(string paymentId, decimal amount)
    {
        var amountInPaise = amount * 100; // Convert amount to paise
        RazorpayClient client = new RazorpayClient(_keyId, _keySecret);
        Payment payment = client.Payment.Fetch(paymentId);
        Payment capturedPayment = payment.Capture(new Dictionary<string, object> { { "amount", amountInPaise } });
        return capturedPayment;
    }


    // Split the payment: 5% to commission account, 95% to owner account
    //public void SplitPayment(string paymentId, decimal amount)
    //{
    //    RazorpayClient client = new RazorpayClient(_keyId, _keySecret);

    //    // Define amounts in paise
    //    var amountInPaise = amount * 100;
    //    var commissionAmount = amountInPaise * 0.05m; // 5% commission
    //    var ownerAmount = amountInPaise * 0.95m;      // 95% to owner

    //    // Create transfer to commission account
    //    Dictionary<string, object> commissionTransfer = new Dictionary<string, object>
    //    {
    //        { "amount", commissionAmount },
    //        { "currency", "INR" },
    //        { "account", _commissionAccountId },
    //        { "payment_id", paymentId }
    //    };

    //    // Create transfer to owner account
    //    Dictionary<string, object> ownerTransfer = new Dictionary<string, object>
    //    {
    //        { "amount", ownerAmount },
    //        { "currency", "INR" },
    //        { "account", _ownerAccountId },
    //        { "payment_id", paymentId }
    //    };

    //    // Execute transfers
    //    Transfer transferCommission = client.Transfer.Create(commissionTransfer);
    //    Transfer transferOwner = client.Transfer.Create(ownerTransfer);
    //}

    // Refund a payment
    public Refund RefundPayment(string paymentId, decimal amount)
    {
        RazorpayClient client = new RazorpayClient(_keyId, _keySecret);
        Payment payment = client.Payment.Fetch(paymentId);
        Refund refund = payment.Refund(new Dictionary<string, object> { { "amount", amount * 100 } });
        return refund;
    }
}
