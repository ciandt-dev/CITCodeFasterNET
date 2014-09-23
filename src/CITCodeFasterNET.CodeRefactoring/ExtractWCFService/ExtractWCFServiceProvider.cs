using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CITCodeFasterNET.CodeRefactoring.ExtractWCFService
{
    [ExportCodeRefactoringProvider(ExtractWCFServiceProvider.RefactoringId, LanguageNames.CSharp)]
    public class ExtractWCFServiceProvider : ICodeRefactoringProvider
    {
        public const string RefactoringId = "ExtractWCFServiceProvider";

        public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken)
        {
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
            var action = CodeAction.Create("Extract WCF Service", c => ExtractWCFServiceAsync(document, classDecl, c));

            // Return this code action.
            return new[] { action };
        }

        private async Task<Solution> ExtractWCFServiceAsync(Document document, ClassDeclarationSyntax classDecl, CancellationToken cancellationToken)
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