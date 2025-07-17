using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Mapster;
using Todos.Api.Models.Requests;
using Todos.Application.Commands.Todos;

namespace Todos;

public class MappingRegister : ICodeGenerationRegister
{
    public void Register(CodeGenerationConfig config)
    {
        config.AdaptFrom("[name]Add", MapType.Map)
            .ApplyDefaultRule();
        config.AdaptFrom("[name]Update", MapType.MapToTarget)
            .ApplyDefaultRule()
            .IgnoreAttributes(typeof(KeyAttribute));

        config.AdaptFrom("[name]Merge", MapType.MapToTarget)
            .ApplyDefaultRule()
            .IgnoreAttributes(typeof(KeyAttribute))
            .IgnoreNullValues(true);

        config.GenerateMapper("[name]Mapper")
            .ForType<CreateTodoRequest>()
            .ForType<CreateNewTodoCommand>();
    }
}

internal static class RegisterExtensions
{
    public static AdaptAttributeBuilder ApplyDefaultRule(this AdaptAttributeBuilder builder)
    {
        return builder
            .ForAllTypesInNamespace(Assembly.GetExecutingAssembly(), "Sample.CodeGen.Domains")
            .ExcludeTypes(type => type.IsEnum)
            .AlterType(type => type.IsEnum || Nullable.GetUnderlyingType(type)?.IsEnum == true, typeof(string));
    }
}