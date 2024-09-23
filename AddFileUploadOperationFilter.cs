using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

public class AddFileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody?.Content != null)
        {
            var requestBodyContent = operation.RequestBody.Content;

            // Add a file upload form-data parameter
            if (requestBodyContent.ContainsKey("multipart/form-data"))
            {
                var formData = requestBodyContent["multipart/form-data"];
                formData.Schema.Properties.Add("file", new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                });
            }
        }
    }
}
