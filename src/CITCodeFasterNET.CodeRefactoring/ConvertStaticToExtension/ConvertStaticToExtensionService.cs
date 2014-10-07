using CITCodeFasterNET.InfraStructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;

namespace CITCodeFasterNET.CodeRefactoring.ConvertStaticToExtension
{
    public class ConvertStaticToExtensionService : ACodeRefactoringService
    {
        public ConvertStaticToExtensionService(Document document, TextSpan textSpan) : base(document, textSpan)
        {

        }

        public override string Description
        {
            get
            {
                return "Convert to extension method";
            }
        }

        public override Document Execute()
        {
            var newDocument = Document;

            var root = Document.GetSyntaxRootAsync().Result;
            var semanticModel = Document.GetSemanticModelAsync().Result;

            // Find the node at the selection.
            var methodDeclSyntax = root.FindNode(this.TextSpan) as MethodDeclarationSyntax;

            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclSyntax);

            var originalSolution = newDocument.Project.Solution;

            var newSolution = originalSolution.ForEachDocument((document) =>
            {

                var newRWDocument = document;

                var semanticModelDocument = newRWDocument.GetSemanticModelAsync().Result;
                var rewriter = new StaticToExtensionReferencesRewriter(methodSymbol, newRWDocument, semanticModelDocument);
                var syntaxRoot = document.GetSyntaxRootAsync().Result;
                var newSyntaxRoot = rewriter.Visit(syntaxRoot);
                newRWDocument = document.WithSyntaxRoot(newSyntaxRoot);

                return newRWDocument;
            });

            newDocument = newSolution.GetDocument(newDocument.Id);

            return newDocument;
        }

        public override bool IsAppliable()
        {
            // Only offer a refactoring if the selected node is a type declaration node.
            var varDecl = Node as MethodDeclarationSyntax;

            if (varDecl == null) return false;

            bool isStaticMethod = (varDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)));
            bool isDeclaredTypeStatic = (varDecl.FirstAncestorOrSelf<ClassDeclarationSyntax>().Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)));
            bool hasParameters = varDecl.ParameterList.Parameters.Any();

            return (
                (isStaticMethod)
                && (isDeclaredTypeStatic)
                && (hasParameters)
            );
        }
    }

    public class StaticToExtensionReferencesRewriter : CSharpSyntaxRewriter
    {
        private IMethodSymbol searchSymbol;
        private Document document;
        private SemanticModel semanticModel
        {
            get
            {
                return document.GetSemanticModelAsync().Result;
            }
        }

        public StaticToExtensionReferencesRewriter(IMethodSymbol searchSymbol, Document document, SemanticModel semanticModel)
        {
            this.searchSymbol = searchSymbol;
            //this.semanticModel = semanticModel;
            this.document = document;
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var updatedDeclarationSyntax = base.VisitMethodDeclaration(node) as MethodDeclarationSyntax;

            var methodSymbol = semanticModel.GetDeclaredSymbol(node);
            if (methodSymbol.Equals(searchSymbol))
            {
                var firstMethodParameter = updatedDeclarationSyntax.ParameterList.Parameters.FirstOrDefault();

                if (firstMethodParameter != null)
                {
                    var thisParamModifier = SyntaxFactory.ParseToken("this")
                        .WithTrailingTrivia(SyntaxFactory.ParseTrailingTrivia(" "));

                    firstMethodParameter = firstMethodParameter.AddModifiers(thisParamModifier);

                    var sepSyntaxList = new SeparatedSyntaxList<ParameterSyntax>();

                    sepSyntaxList = sepSyntaxList
                        .Add(firstMethodParameter)
                        .AddRange(updatedDeclarationSyntax.ParameterList.Parameters.Skip(1).ToArray());

                    var parameterList = updatedDeclarationSyntax.ParameterList.WithParameters(sepSyntaxList);

                    updatedDeclarationSyntax = updatedDeclarationSyntax.WithParameterList(parameterList);
                }
            }

            return updatedDeclarationSyntax;
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var updatedInvocationSyntax = node;

            var expression = updatedInvocationSyntax.Expression;

            var expressionSymbol = semanticModel.GetSymbolInfo(expression as SyntaxNode).Symbol;

            if (expressionSymbol != null)
            {
                var isMatchingTypeName = expressionSymbol.Equals(searchSymbol);

                if (isMatchingTypeName)
                {
                    var firstArgument = updatedInvocationSyntax.ArgumentList.Arguments.First();

                    var firstArgumentIdentifier = firstArgument.Expression as IdentifierNameSyntax;

                    string finalExpression = string.Format("{0}.{1}", firstArgumentIdentifier.Identifier.Text, searchSymbol.Name);

                    var sepSyntaxList = new SeparatedSyntaxList<ArgumentSyntax>();

                    sepSyntaxList = sepSyntaxList.AddRange(updatedInvocationSyntax.ArgumentList.Arguments.Skip(1).ToArray());

                    var argumentList = updatedInvocationSyntax.ArgumentList.WithArguments(sepSyntaxList);

                    var newExpression = updatedInvocationSyntax.Expression
                        .ReplaceNode(updatedInvocationSyntax.Expression, 
                        SyntaxFactory.ParseExpression(finalExpression));

                    var newUpdatedInvocationSyntax = updatedInvocationSyntax
                        .WithExpression(newExpression)
                        .WithArgumentList(argumentList);

                    updatedInvocationSyntax = updatedInvocationSyntax.ReplaceNode(updatedInvocationSyntax, newUpdatedInvocationSyntax);

                    Console.WriteLine();
                }
            }

            return base.VisitInvocationExpression(updatedInvocationSyntax);
        }

        //public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        //{
        //    MemberAccessExpressionSyntax m = null;
        //    IdentifierNameSyntax i = null;
        //    var syntaxNode = node.Expression as SyntaxNode;
        //    var expression = node.Expression as ExpressionSyntax;
        //    var tokens = node.DescendantNodesAndTokens().Where(p => p.IsToken && p.AsToken().ValueText.Equals("ConvertToQueue"));

        //    var token = tokens.OfType<SyntaxNodeOrToken>().FirstOrDefault().AsToken();

        //    var parentToken = token.Parent as IdentifierNameSyntax;

        //    if (parentToken != null)
        //    {
        //        // Replace the identifier token containing the name of the class.
        //        SyntaxToken updatedIdentifierToken =
        //            SyntaxFactory.Identifier(
        //                token.LeadingTrivia,
        //                "newName",
        //                token.TrailingTrivia);

        //        parentToken = parentToken.WithIdentifier(updatedIdentifierToken);
        //    }

        //    return base.VisitInvocationExpression(node);
        //}
    }
}
