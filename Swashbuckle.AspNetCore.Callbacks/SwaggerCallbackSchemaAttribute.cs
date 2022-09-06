using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;

namespace Swashbuckle.AspNetCore.Callbacks;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class SwaggerCallbackSchemaAttribute : Attribute
{
    public Type SchemaType { get; }
    public string Name { get; }

    public SwaggerCallbackSchemaAttribute(object name, Type schemaType)
    {
        Name = GetName(name);
        SchemaType = schemaType;
    }

    private static string GetName(object name) =>
        name switch
        {
            string stringName => stringName,
            Enum enumName => FromEnum(enumName),
            _ => throw new ArgumentException($"{name.GetType().FullName} is not supported")
        };

    private static string FromEnum(Enum value)
    {
        var member = value.GetType().GetMember(value.ToString(),
            MemberTypes.Field, BindingFlags.Public | BindingFlags.Static).FirstOrDefault();
        if (member == null)
        {
            return value.ToString();
        }

        var attribute = member.GetCustomAttributes(false).OfType<EnumMemberAttribute>().FirstOrDefault();

        return attribute == null ? value.ToString() : attribute.Value;
    }
}
