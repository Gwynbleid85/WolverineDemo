using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace MapsterChecker;

public class AdaptInvocationFinder
{
    public record AdaptInvocation(
        string FilePath,
        int LineNumber,
        string SourceExpression,
        string? SourceType,
        string? DestinationType,
        string FullInvocation);

    public static async Task<List<AdaptInvocation>> FindAdaptInvocationsAsync(string solutionPath)
    {
        var invocations = new List<AdaptInvocation>();

        using var workspace = MSBuildWorkspace.Create();
        var solution = await workspace.OpenSolutionAsync(solutionPath);

        foreach (var project in solution.Projects)
        {
            var compilation = await project.GetCompilationAsync();
            if (compilation == null) continue;

            foreach (var document in project.Documents)
            {
                var syntaxTree = await document.GetSyntaxTreeAsync();
                if (syntaxTree == null) continue;

                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var root = await syntaxTree.GetRootAsync();

                var walker = new AdaptInvocationWalker(semanticModel, document.FilePath ?? "Unknown");
                walker.Visit(root);
                invocations.AddRange(walker.Invocations);
            }
        }

        return invocations;
    }

    private class AdaptInvocationWalker : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        private readonly string _filePath;
        private readonly List<AdaptInvocation> _invocations = new();

        public IReadOnlyList<AdaptInvocation> Invocations => _invocations;

        public AdaptInvocationWalker(SemanticModel semanticModel, string filePath)
        {
            _semanticModel = semanticModel;
            _filePath = filePath;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var memberAccess = node.Expression as MemberAccessExpressionSyntax;
            if (memberAccess?.Name.Identifier.ValueText == "Adapt")
            {
                var sourceExpression = memberAccess.Expression.ToString();
                var lineNumber = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                var fullInvocation = node.ToString();

                var sourceType = GetTypeFromExpression(memberAccess.Expression);
                var destinationType = GetDestinationType(node);

                _invocations.Add(new AdaptInvocation(
                    _filePath,
                    lineNumber,
                    sourceExpression,
                    sourceType,
                    destinationType,
                    fullInvocation));
            }

            base.VisitInvocationExpression(node);
        }

        private string? GetTypeFromExpression(ExpressionSyntax expression)
        {
            var typeInfo = _semanticModel.GetTypeInfo(expression);
            return typeInfo.Type?.ToDisplayString();
        }

        private string? GetDestinationType(InvocationExpressionSyntax invocation)
        {
            var argumentList = invocation.ArgumentList;
            if (argumentList.Arguments.Count > 0)
            {
                var firstArg = argumentList.Arguments[0];
                if (firstArg.Expression is TypeOfExpressionSyntax typeOfExpr)
                {
                    var typeInfo = _semanticModel.GetTypeInfo(typeOfExpr.Type);
                    return typeInfo.Type?.ToDisplayString();
                }
            }

            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess?.Name is GenericNameSyntax genericName)
            {
                if (genericName.TypeArgumentList.Arguments.Count > 0)
                {
                    var typeArg = genericName.TypeArgumentList.Arguments[0];
                    var typeInfo = _semanticModel.GetTypeInfo(typeArg);
                    return typeInfo.Type?.ToDisplayString();
                }
            }

            return null;
        }
    }
}