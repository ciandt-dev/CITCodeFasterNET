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
    [ExportCodeRefactoringProvider(ExtractInterfaceProvider.RefactoringId, LanguageNames.CSharp)]
    internal class ExtractInterfaceProvider : ICodeRefactoringProvider
    {
        public const string RefactoringId = "ExtractInterfaceProvider";

        public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken)
        {
            // TODO: Replace the following code with your own analysis, generating a CodeAction for each refactoring to offer

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            // Find the node at the selection.
            var node = root.FindNode(textSpan);

            // Only offer a refactoring if the selected node is a type declaration node.
            var classDecl = node as ClassDeclarationSyntax;
            if (classDecl == null)
            {
                return null;
            }
            
            // For any type declaration node, create a code action to reverse the identifier text.
            var action = CodeAction.Create("Export WCF Service", c => ExportWCFServiceAsync(document, classDecl, c));

            // Return this code action.
            return new[] { action };
        }

        private async Task<Solution> ExportWCFServiceAsync(Document document, ClassDeclarationSyntax classDecl, CancellationToken cancellationToken)
        {
            var originalSolution = document.Project.Solution;

            var newSolution = ServiceInterfaceBuilder.CreateInterface(originalSolution, document.Project, classDecl);

            newSolution = ServiceInterfaceBuilder.CreateServiceClass(newSolution, document.Project, classDecl);

            newSolution = ServiceInterfaceBuilder.AddReferencesToProject(newSolution, document.Project);

            // Return the new solution with the now-uppercase type name.
            return newSolution;
        }
    }
}