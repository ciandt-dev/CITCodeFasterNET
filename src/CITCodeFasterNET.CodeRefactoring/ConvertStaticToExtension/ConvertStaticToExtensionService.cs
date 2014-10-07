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

            var originalSolution = newDocument.Project.Solution;

            originalSolution.ForEachDocument((document) => {

                var newRWDocument = document;

                var rewriter = new StaticToExtensionReferencesRewriter();
                var syntaxRoot = document.GetSyntaxRootAsync().Result;
                var newSyntaxRoot = rewriter.Visit(syntaxRoot);
                newRWDocument = document.WithSyntaxRoot(newSyntaxRoot);

                return newRWDocument;
            });

            return newDocument;
        }

        public override bool IsAppliable()
        {
            // Only offer a refactoring if the selected node is a type declaration node.
            var varDecl = Node as MethodDeclarationSyntax;

            if (varDecl == null) return false;

            bool isStaticMethod = (varDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)));
            bool isDeclaredTypeStatic = (varDecl.FirstAncestorOrSelf<ClassDeclarationSyntax>().Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)));

            return (
                (isStaticMethod)
                && (isDeclaredTypeStatic)
            );
        }
    }

    public class StaticToExtensionReferencesRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            MemberAccessExpressionSyntax m = null;
            IdentifierNameSyntax i = null;
            var syntaxNode = node.Expression as SyntaxNode;
            var expression = node.Expression as ExpressionSyntax;
            var tokens = node.DescendantNodesAndTokens().Where(p => p.IsToken && p.AsToken().ValueText.Equals("ConvertToQueue"));

            var token = tokens.OfType<SyntaxNodeOrToken>().FirstOrDefault().AsToken();

            var parentToken = token.Parent as IdentifierNameSyntax;

            if (parentToken != null)
            {
                // Replace the identifier token containing the name of the class.
                SyntaxToken updatedIdentifierToken =
                    SyntaxFactory.Identifier(
                        token.LeadingTrivia,
                        "newName",
                        token.TrailingTrivia);

                parentToken = parentToken.WithIdentifier(updatedIdentifierToken);
            }

            return base.VisitInvocationExpression(node);
        }
    }
}
