using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace CIandTRefactoringTools
{
    [ExportCodeRefactoringProvider(MakeExplicitProvider.RefactoringId, LanguageNames.CSharp)]
    internal class MakeExplicitProvider : ICodeRefactoringProvider
    {
        public const string RefactoringId = "MakeExplicitProvider";

        public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            // Find the node at the selection.
            var node = root.FindNode(textSpan);

            // Only offer a refactoring if the selected node is a type declaration node.
            var varDecl = node as IdentifierNameSyntax;
            if (varDecl == null || !(varDecl.IsVar && varDecl.Parent is VariableDeclarationSyntax))
            {
                return null;
            }

            // For any type declaration node, create a code action to reverse the identifier text.
            var action = CodeAction.Create("Make explicit", c => MakeExplicitAsync(document, varDecl, c));

            // Return this code action.
            return new[] { action };
        }

        private async Task<Solution> MakeExplicitAsync(Document document, IdentifierNameSyntax varDecl, CancellationToken cancellationToken)
        {
            var originalSolution = document.Project.Solution;


            var newSolution = MakeExplicit(document, varDecl);

            // Return the new solution with the now-uppercase type name.
            return newSolution;
        }

        private Solution MakeExplicit(Document document, IdentifierNameSyntax varDecl)
        {
            return document.Project.Solution;
        }
    }
}