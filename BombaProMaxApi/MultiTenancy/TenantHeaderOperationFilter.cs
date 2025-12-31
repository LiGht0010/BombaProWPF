using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BombaProMaxApi.MultiTenancy;

/// <summary>
/// Adds the X-Tenant-ID header parameter to all Swagger operations.
/// </summary>
public class TenantHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = TenantService.TenantHeaderName,
            In = ParameterLocation.Header,
            Required = false,
            Description = "Tenant identifier (e.g., 'client1', 'client2', 'client3', 'sidikassem'). Defaults to 'client1' if not provided.",
            Schema = new OpenApiSchema
            {
                Type = "string",
                Default = new Microsoft.OpenApi.Any.OpenApiString("client1")
            }
        });
    }
}
