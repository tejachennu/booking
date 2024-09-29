using BusBooking.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;

namespace BusBooking.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly RazorpayService _razorpayService;
        private readonly BusBookingContext _context;

        public PaymentsController(RazorpayService razorpayService, BusBookingContext context)
        {
            _razorpayService = razorpayService;
            _context = context;
        }

        // POST api/payments/create-order
        [HttpPost("create-order")]
        public IActionResult CreateOrder([FromBody] PaymentRequest paymentRequest)
        {
            try
            {
                var order = _razorpayService.CreateOrder(paymentRequest.Amount, paymentRequest.Currency);
                return Ok(new { orderId = order["id"].ToString(), amount = order["amount"], currency = order["currency"] });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

      

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyPayment([FromBody] PaymentVerificationRequest verificationRequest)
        {
            // Check if the verification request is null
            if (verificationRequest == null)
            {
                return BadRequest("Invalid verification request.");
            }

            try
            {
                // Verify the payment using the Razorpay service
                var isPaymentValid = _razorpayService.VerifyPayment(
                    verificationRequest.RazorpayOrderId,
                    verificationRequest.RazorpayPaymentId,
                    verificationRequest.RazorpaySignature);

                if (isPaymentValid)
                {
                    // Payment is valid, update the booking and tickets status to "success"
                    var booking = await _context.BookSeats
                        .Include(b => b.Tickets) // Ensure tickets are included in the query
                        .FirstOrDefaultAsync(b => b.RazorpayOrderId == verificationRequest.RazorpayOrderId);

                    if (booking != null)
                    {
                        // Update booking status
                        booking.Status = "booked";

                        // Update ticket statuses
                        foreach (var ticket in booking.Tickets)
                        {
                            ticket.Status = "booked"; // Change each ticket's status to "success"
                        }

                        // Save changes to the database
                        await _context.SaveChangesAsync();

                        return Ok("Payment verified");
                    }
                    else
                    {
                        return NotFound("Booking not found.");
                    }
                }
                else
                {
                    // Invalid payment signature
                    return BadRequest("Invalid payment signature.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }


        // POST api/payments/refund
        [HttpPost("refund")]
        public IActionResult RefundPayment([FromBody] RefundRequest refundRequest)
        {
            try
            {
                var refund = _razorpayService.RefundPayment(refundRequest.PaymentId, refundRequest.Amount);
                return Ok(new { refundId = refund["id"].ToString(), amount = refund["amount"], status = refund["status"] });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }

    public class PaymentVerificationRequest
    {
        public string RazorpayOrderId { get; set; }
        public string RazorpayPaymentId { get; set; }
        public string RazorpaySignature { get; set; }
    }

    public class RefundRequest
    {
        public string PaymentId { get; set; }
        public decimal Amount { get; set; }
    }
}
