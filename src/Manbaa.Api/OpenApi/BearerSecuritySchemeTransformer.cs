using Microsoft.AspNetCore.Authentication;

using Microsoft.AspNetCore.OpenApi;

using Microsoft.OpenApi;

namespace Manbaa.Api.OpenApi;

internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var schemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        if (schemes.All(scheme => scheme.Name != "Bearer"))
        {
            return;
        }

        document.Components ??= new OpenApiComponents();

        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,

                Scheme = "bearer",

                In = ParameterLocation.Header,

                BearerFormat = "Json Web Token"
            }
        };

        foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations!))
        {
            operation.Value.Security ??= [];

            operation.Value.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        }
    }
}
