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
using CITCodeFasterNET.CodeRefactoring.ConvertStaticToExtension;

namespace CITCodeFasterNET.CodeRefactoring.MakeExplicit
{
    [ExportCodeRefactoringProvider(ConvertStaticToExtensionProvider.RefactoringId, LanguageNames.CSharp)]
    public class ConvertStaticToExtensionProvider : ICodeRefactoringProvider
    {
        public const string RefactoringId = "ConvertStaticToExtensionProvider";

        public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken)
        {
            var service = new ConvertStaticToExtensionService(document, textSpan);

            if(!service.IsAppliable())
                return null;

            // For any type declaration node, create a code action to reverse the identifier text.
            var action = CodeAction.Create(service.Description, c => service.ExecuteAsync(c));

            // Return this code action.
            return new[] { action };
        }
    }
}