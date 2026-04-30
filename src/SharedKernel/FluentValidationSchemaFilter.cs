using System.Globalization;
using System.Reflection;
using FluentValidation;
using FluentValidation.Validators;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SharedKernel;

internal sealed class FluentValidationSchemaFilter : ISchemaFilter
{
    private static readonly Lazy<IReadOnlyCollection<IValidator>> Validators = new(FindValidators);

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema openApiSchema || openApiSchema.Properties is null or { Count: 0 })
            return;

        var type = context.Type;
        foreach (var validator in Validators.Value.Where(x => x.CanValidateInstancesOfType(type)))
        {
            foreach (var rule in validator.CreateDescriptor().Rules)
            {
                var propertyName = GetSchemaPropertyName(openApiSchema, rule.PropertyName);
                if (propertyName is null || !openApiSchema.Properties.TryGetValue(propertyName, out var propertySchema))
                    continue;

                if (propertySchema is not OpenApiSchema openApiPropertySchema)
                    continue;

                foreach (var component in rule.Components)
                    ApplyRule(openApiSchema, propertyName, openApiPropertySchema, component.Validator);
            }
        }
    }

    private static IReadOnlyCollection<IValidator> FindValidators()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .SelectMany(assembly =>
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    return ex.Types.OfType<Type>();
                }
            })
            .Where(type => type is { IsAbstract: false, IsInterface: false, ContainsGenericParameters: false })
            .Where(type => typeof(IValidator).IsAssignableFrom(type))
            .Select(CreateValidator)
            .Where(validator => validator is not null)
            .Cast<IValidator>()
            .ToArray();
    }

    private static IValidator? CreateValidator(Type type)
    {
        try
        {
            return Activator.CreateInstance(type) as IValidator;
        }
        catch
        {
            return null;
        }
    }

    private static string? GetSchemaPropertyName(OpenApiSchema schema, string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName) || schema.Properties is null)
            return null;

        if (schema.Properties.ContainsKey(propertyName))
            return propertyName;

        var camelCaseName = char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
        return schema.Properties.ContainsKey(camelCaseName) ? camelCaseName : null;
    }

    private static void ApplyRule(
        OpenApiSchema parentSchema,
        string propertyName,
        OpenApiSchema propertySchema,
        IPropertyValidator validator
    )
    {
        switch (validator)
        {
            case INotNullValidator or INotEmptyValidator:
                parentSchema.Required ??= new HashSet<string>();
                parentSchema.Required.Add(propertyName);
                break;

            case ILengthValidator lengthValidator:
                ApplyLength(propertySchema, lengthValidator);
                break;

            case IBetweenValidator betweenValidator:
                propertySchema.Minimum = ToOpenApiNumber(betweenValidator.From);
                propertySchema.Maximum = ToOpenApiNumber(betweenValidator.To);
                break;

            case IRegularExpressionValidator regularExpressionValidator:
                propertySchema.Pattern = regularExpressionValidator.Expression;
                break;
        }
    }

    private static void ApplyLength(OpenApiSchema schema, ILengthValidator validator)
    {
        if (validator.Min > 0)
            schema.MinLength = validator.Min;

        if (validator.Max < int.MaxValue)
            schema.MaxLength = validator.Max;
    }

    private static string ToOpenApiNumber(object value)
    {
        return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
    }
}
