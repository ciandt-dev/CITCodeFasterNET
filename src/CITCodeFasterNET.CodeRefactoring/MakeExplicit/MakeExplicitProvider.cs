using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;
using CITCodeFasterNET.InfraStructure;

namespace CITCodeFasterNET.CodeRefactoring.MakeExplicit
{
    [ExportCodeRefactoringProvider(MakeExplicitProvider.RefactoringId, LanguageNames.CSharp)]
    public class MakeExplicitProvider : ICodeRefactoringProvider
    {
        public const string RefactoringId = "MakeExplicitProvider";

        public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            // Find the node at the selection.
            var node = root.FindNode(textSpan);

            var semanticModel = document.GetSemanticModelAsync().Result;

            bool hasCompileError = semanticModel.GetDiagnostics(node.Parent.Span).Any(p => p.Severity == DiagnosticSeverity.Error);

            if (hasCompileError) return null;

            // Only offer a refactoring if the selected node is a type declaration node.
            var varDecl = node as IdentifierNameSyntax;

            if ((varDecl == null) 
                || (varDecl.GetTypeInfoFromIdentifier(document).Type.IsAnonymousType)
                || (!varDecl.IsVar && varDecl.Parent is VariableDeclarationSyntax))
            {
                return null;
            }

            // For any type declaration node, create a code action to reverse the identifier text.
            var action = CodeAction.Create("Make explicit", c => MakeExplicitAsync(document, varDecl, c));

            // Return this code action.
            return new[] { action };
        }

        private async Task<Document> MakeExplicitAsync(Document document, IdentifierNameSyntax varDecl, CancellationToken cancellationToken)
        {
            var newDocument = MakeExplicit(document, varDecl);

            // Return the new solution with the now-uppercase type name.
            return newDocument;
        }

        private Document MakeExplicit(Document document, IdentifierNameSyntax varDecl)
        {
            var syntaxRoot = document.GetSyntaxRootAsync().Result;

            var varTargetType = varDecl.GetTypeInfoFromIdentifier(document);

            var varTargetTypeNamespace = varTargetType.GetNamespaceSymbol();

            var fullNamespaceString = varTargetTypeNamespace.ToDisplayString();

            //Try to add the using before
            var varTargetTypeIdentifierName = SyntaxFactory.IdentifierName(varTargetType.Type.ToMinimalDisplayString(document.GetSemanticModelAsync().Result, varDecl.Span.Start))
                .WithLeadingTrivia(varDecl.GetLeadingTrivia())
                .WithTrailingTrivia(varDecl.GetTrailingTrivia());

            var newDeclaration = varDecl.ReplaceNode(varDecl, varTargetTypeIdentifierName).WithAdditionalAnnotations(Simplifier.Annotation);

            var parentVarDeclNamespace = varDecl.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();

            var parentVarDeclNamespaceString = (parentVarDeclNamespace != null) ? parentVarDeclNamespace.Name.ToString() : null;

            var newRoot = syntaxRoot.ReplaceNode(varDecl, newDeclaration);

            if ((!string.IsNullOrWhiteSpace(parentVarDeclNamespaceString)) && (parentVarDeclNamespaceString != fullNamespaceString))
            {
                newRoot = (newRoot as CompilationUnitSyntax).WithUsing(fullNamespaceString);
            }

            return document.WithSyntaxRoot(newRoot).SimplifyDocument().FormatDocument();
        }
    }
}