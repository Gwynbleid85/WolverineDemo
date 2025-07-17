using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapsterChecker;

public class AdaptChecker
{
    public static bool CheckAdapt(InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Name is GenericNameSyntax genericName &&
            genericName.Identifier.Text == "Adapt") // Check if it's Adapt method
        {
            // Get the object that .Adapt was invoked on (e.g., user in user.Adapt<UserDto>())
            var objectExpression = memberAccess.Expression.ToString();

            // Get the generic type parameters (e.g., UserDto from .Adapt<UserDto>())
            var typeArgs = string.Join(", ", genericName.TypeArgumentList.Arguments
                .Select(arg => arg.ToString()));
            Console.WriteLine($"{objectExpression}.Adapt<{typeArgs}>()");
        }

        return true;
    }
}