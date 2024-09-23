using System.Net;
using System.Text.Json;

namespace BusBooking.Server
{
    // Define a middleware class to handle Method Not Allowed (405) status codes
    public class MethodNotAllowedMiddleware
    {
        // Field to store the next middleware in the pipeline
        private readonly RequestDelegate _next;

        // Constructor to initialize the next middleware
        public MethodNotAllowedMiddleware(RequestDelegate next)
        {
            _next = next; // Assign the next middleware
        }

        // Method to handle the HTTP context
        public async Task Invoke(HttpContext context)
        {
            // Invoke the next middleware in the pipeline
            await _next(context);

            // Check if the response status code is 405 Method Not Allowed
            if (context.Response.StatusCode == (int)HttpStatusCode.MethodNotAllowed)
            {
                // Set the response content type to application/json
                context.Response.ContentType = "application/json";

                // Create a custom response object with code and message
                var customResponse = new
                {
                    // Custom code field indicating the status code
                    Code = 405,
                    // Custom message field
                    Message = "HTTP Method not allowed"
                };

                // Serialize the custom response object to JSON
                var responseJson = JsonSerializer.Serialize(customResponse);

                // Write the JSON response to the HTTP response body
                await context.Response.WriteAsync(responseJson);
            }
        }
    }
}