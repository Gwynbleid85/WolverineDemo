using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

using var workspace = MSBuildWorkspace.Create();
var solution =
    await workspace.OpenSolutionAsync(@"/Users/miloshegr/Documents/Projects/Dotnet/WolverineDemo/WolverineDemo.sln");

foreach (var project in solution.Projects)
foreach (var document in project.Documents)
{
    var root = await document.GetSyntaxRootAsync();
    if (root == null)
        continue;

    var semanticModel = await document.GetSemanticModelAsync(); // Get the SemanticModel

    var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

    foreach (var invocation in invocations)
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Name is GenericNameSyntax genericName &&
            genericName.Identifier.Text == "Adapt") // Check if it's Adapt method
        {
            // Get the object that .Adapt was invoked on (e.g., user in user.Adapt<UserDto>())
            var objectExpression = memberAccess.Expression.ToString();
            Console.WriteLine($"Object Invoked On: {objectExpression}");

            // Get the type of the object using SemanticModel
            var invokerSymbol = semanticModel?.GetTypeInfo(memberAccess.Expression);

            var typeArguments = genericName.TypeArgumentList.Arguments
                .Select(arg => semanticModel?.GetTypeInfo(arg).Type) // Extract the type argument as string
                .ToList();

            Console.WriteLine($"Type of Object Invoked On: {invokerSymbol}");
            Console.WriteLine($"Generic Type Arguments: {genericName.Identifier.Text}");
            Console.WriteLine($"Type Arguments: {string.Join(", ", typeArguments)}");
            Console.WriteLine("--------------------------------------------------");
        }

    // Get the generic type parameters (e.g., UserDto from .Adapt<UserDto>())
}