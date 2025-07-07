using CleanResult;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;
using Wolverine;
using Wolverine.Configuration;
using Wolverine.Middleware;

namespace SharedKernel.Infrastructure;

/// <summary>
/// Custom continuation strategy for handling CleanResult types.
/// <br /><br />
/// Strategy is applied on handlers where <c>Load/LoadAsync</c> methods return a <see cref="Result" /> or
/// <see cref="Result{T}" /> type.
/// <br /><br />
/// This continuation strategy checks if the <c> Load/LoadAsync </c> methods return value represents an error.
/// If yes the continuation is stopped and the <see cref="Result" /> type representing the error values is returned.
/// If the return value represents success the handler execution continues. and if the result contains the success
/// value, it is extracted and stored in a new variable for further use in the handler execution.
/// </summary>
/// <remarks>
/// This strategy is supposed to be used primary with <see cref="IMessageBus.InvokeAsync{T}" /> methods (other
/// invocation like Publish or Send types were not tested yet)
/// </remarks>
public class CleanResultContinuationStrategy : IContinuationStrategy
{
    /// <summary>
    /// </summary>
    public bool TryFindContinuationHandler(IChain chain, MethodCall call, out Frame? frame)
    {
        var result = call.Creates.FirstOrDefault(x => x.VariableType == typeof(Result));
        if (result != null)
        {
            frame = new MaybeEndHandlerWithCleanResultFrame(result, call.ReturnType);
            return true;
        }

        result = call.Creates.FirstOrDefault(x =>
            x.VariableType.IsGenericType && x.VariableType.GetGenericTypeDefinition() == typeof(Result<>));
        if (result != null)
        {
            frame = new MaybeEndHandlerWithGenericCleanResultFrame(result, call.ReturnType);
            return true;
        }

        frame = null;
        return false;
    }

    /// <summary>
    /// Helper function to get a friendly type name for the generics Result type for code generation,
    /// since the Type.FullName property returns name in the format "CleanResult.Result`1" or "CleanResult.Result",
    /// which is not usable directly in code
    /// </summary>
    /// <param name="type">Type to be converted to the string</param>
    /// <returns>Codegen friendly type name</returns>
    private static string GetFriendlyTypeName(Type type)
    {
        if (!type.IsGenericType)
            return type.FullName ?? type.Name;

        var namePrefix =
            (type.GetGenericTypeDefinition().FullName ?? type.Name).Split('`',
                StringSplitOptions.RemoveEmptyEntries)[0];
        var genericParameters = string.Join(",", type.GetGenericArguments().Select(GetFriendlyTypeName));
        return namePrefix + "<" + genericParameters + ">";
    }

    /// <summary>
    /// Generate code for the continuation frame that checks the non-generics cleanResult of the handler execution.
    /// </summary>
    private class MaybeEndHandlerWithCleanResultFrame : AsyncFrame
    {
        private readonly Type? _handlerReturnType;
        private readonly Variable _result;

        public MaybeEndHandlerWithCleanResultFrame(Variable result, Type? handlerReturnType)
        {
            uses.Add(result);
            _result = result;
            _handlerReturnType = handlerReturnType;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.BlankLine();
            writer.WriteComment("CleanResult continuation check for Result types");

            writer.Write($"BLOCK:if ({_result.Usage}.IsError())");
            if (_handlerReturnType != null)
                writer.Write(
                    $"await context.EnqueueCascadingAsync(_handlerReturnType {GetFriendlyTypeName(_handlerReturnType)}.Error({_result.Usage}.ErrorValue)).ConfigureAwait(false);");
            writer.Write("return;");
            writer.FinishBlock();
            writer.BlankLine();

            Next?.GenerateCode(method, writer);
        }
    }

    /// <summary>
    /// Generate code for the continuation frame that checks the generics cleanResult of the handler execution.
    /// And extracts the success value to a new variable.
    /// </summary>
    private class MaybeEndHandlerWithGenericCleanResultFrame : AsyncFrame
    {
        private readonly Type? _handlerReturnType;
        private readonly Variable _result;

        public MaybeEndHandlerWithGenericCleanResultFrame(Variable result, Type? handlerReturnType)
        {
            uses.Add(result);
            // Register a new variable for the success value of the Result<T>
            creates.Add(new Variable(result.VariableType.GetGenericArguments()[0], result.Usage + "SuccessValue"));
            _result = result;
            _handlerReturnType = handlerReturnType;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.BlankLine();
            writer.WriteComment("CleanResult continuation check for Result<T> types");

            writer.Write($"BLOCK:if ({_result.Usage}.IsError())");
            if (_handlerReturnType != null)
                writer.Write(
                    $"await context.EnqueueCascadingAsync({GetFriendlyTypeName(_handlerReturnType)}.Error({_result.Usage}.ErrorValue)).ConfigureAwait(false);");
            writer.Write("return;");
            writer.FinishBlock();

            writer.WriteComment("Extracting the success value from Result<T>");
            writer.WriteLine(
                $"{_result.VariableType.GetGenericArguments()[0].FullName} {_result.Usage}SuccessValue = {_result.Usage}.Value;");

            Next?.GenerateCode(method, writer);
        }
    }
}