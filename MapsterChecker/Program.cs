using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

using var workspace = MSBuildWorkspace.Create();
var solution = await workspace.OpenSolutionAsync(@"C:\Path\To\Your.sln");

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
            var symbol = semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
            if (symbol != null)
            {
                var objectType = symbol?.Type; // Get the type of the object
                Console.WriteLine($"Type of Object Invoked On: {objectType}");
            }

            // Get the generic type parameters (e.g., UserDto from .Adapt<UserDto>())
            var typeArgs = string.Join(", ", genericName.TypeArgumentList.Arguments
                .Select(arg => arg.ToString()));
            Console.WriteLine($"Generic Type Arguments: {typeArgs}");
        }
}