using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.OpenApi.Expressions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Callbacks;

/// <summary>
/// Adds support for callbacks to the swagger doc
/// </summary>
[ExcludeFromCodeCoverage]
public class CallbacksOperationFilter : IOperationFilter
{
	private static readonly OpenApiResponses DefaultCallbackResponses = new()
	{
		{ "200", new OpenApiResponse { Description = "Your server implementation should return this HTTP status code if the data was received successfully" } },
		{ "4xx", new OpenApiResponse { Description = "If your server returns an HTTP status code indicating it does not understand the format of the payload the delivery will be treated as a failure. No retries are attempted." } },
		{ "5xx", new OpenApiResponse { Description = "If your server returns an HTTP status code indicating a server-side error the delivery will be treated as a failure. retries are attempted." } },
	};

	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		foreach (var attribute in context
					 .MethodInfo
					 .GetCustomAttributes<SwaggerCallbackSchemaAttribute>())
		{
			context.SchemaGenerator.GenerateSchema(attribute.SchemaType, context.SchemaRepository);

			var callback = CreateCallback(attribute);
			operation.Callbacks.Add(callback.Name, callback.CallbackObj);
		}
	}

	private static Callback CreateCallback(SwaggerCallbackSchemaAttribute attribute)
	{
		var obsoleteAttrib = attribute.SchemaType.GetCustomAttribute<ObsoleteAttribute>();
		var callbackObj = new OpenApiPathItem
		{
			Operations = new Dictionary<OperationType, OpenApiOperation>
			{
				{
					OperationType.Post,
					new OpenApiOperation
					{
						RequestBody = new OpenApiRequestBody
						{
							Content = new Dictionary<string, OpenApiMediaType>
							{
								{
									"application/json",
									new OpenApiMediaType { Schema = GetSchema(attribute) }
								}
							}
						},
						Deprecated = obsoleteAttrib != null,
						Responses = DefaultCallbackResponses
					}
				}
			}
		};

		return new Callback
		{
			Name = attribute.Name,
			CallbackObj = new OpenApiCallback
			{
				PathItems = new Dictionary<RuntimeExpression, OpenApiPathItem>
				{
					{ RuntimeExpression.Build("https://www.partner.bank/webhook_endpoint"), callbackObj }
				}
			}
		};
	}

	private static OpenApiSchema GetSchema(SwaggerCallbackSchemaAttribute attribute) =>
		attribute.SchemaType.IsArray switch
		{
			true => new OpenApiSchema
			{
				Type = "array",
				Items = new OpenApiSchema
				{
					Reference = new OpenApiReference
					{
						Id = attribute.SchemaType.GetElementType()!.ToString(),
						Type = ReferenceType.Schema
					}
				}
			},
			_ => new OpenApiSchema
			{
				Reference = new OpenApiReference { Id = attribute.SchemaType.Name, Type = ReferenceType.Schema }
			}
		};
}
