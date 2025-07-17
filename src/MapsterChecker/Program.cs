using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

using var workspace = MSBuildWorkspace.Create();
var solution =
    await workspace.OpenSolutionAsync(
        @"/Users/miloshegr/Documents/Projects/Dotnet/WolverineDemo/WolverineDemo.sln");
foreach (var project in solution.Projects)
{
    Console.WriteLine($"Project: {project.Name}");
    foreach (var document in project.Documents)
    {
        Console.WriteLine($"  Document: {document.Name}");
        var root = await document.GetSyntaxRootAsync();
        if (root is null)
            continue;

        var invocations = root.DescendantNodes()
            .OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name is GenericNameSyntax genericName &&
                genericName.Identifier.Text == "Adapt")
            {
                // Get the type argument (e.g., UserDto)
                var typeArgs = string.Join(", ", genericName.TypeArgumentList.Arguments);
                Console.WriteLine(
                    $"{document.FilePath}:{invocation.GetLocation().GetLineSpan().StartLinePosition.Line} -> Adapt<{typeArgs}>");
            }
    }
}