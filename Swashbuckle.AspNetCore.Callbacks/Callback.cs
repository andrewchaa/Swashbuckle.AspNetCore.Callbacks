using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.Callbacks;

[ExcludeFromCodeCoverage]
public class Callback
{
    public string Name { get; init; }

    public OpenApiCallback CallbackObj { get; init; }
}
