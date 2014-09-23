using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;
using System.Threading;
using System;

namespace CITCodeFasterNET.InfraStructure
{
    public static class Extensions
    {
        public static CompilationUnitSyntax WithUsing(this CompilationUnitSyntax root, string name)
        {
            if (!root.Usings.Any(u => u.Name.ToString() == name))
            {
                root = root.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(name)));
            }

            return root;
        }

        public static IList<string> GetUsings(this CompilationUnitSyntax root)
        {
            return root.Usings.Select(u => u.Name.ToString()).ToList();
        }

        public static TypeInfo GetTypeInfoFromIdentifier(this IdentifierNameSyntax identifier, Document document)
        {
            var semanticModel = document.GetSemanticModelAsync().Result;
            return semanticModel.GetTypeInfo(identifier);
        }

        public static INamespaceSymbol GetNamespaceSymbol(this TypeInfo typeInfo)
        {
            if (typeInfo.Type is IArrayTypeSymbol)
                return (typeInfo.Type as IArrayTypeSymbol).ElementType.ContainingNamespace;

            return typeInfo.Type.ContainingNamespace;
        }

        public static Document SimplifyDocument(this Document document)
        {
            return Simplifier.ReduceAsync(document, Simplifier.Annotation).Result;
        }

        public static Document FormatDocument(this Document document)
        {
            return Formatter.FormatAsync(document).Result;
        }

        public static CodeAction GetCodeActionByDescription(this ICodeRefactoringProvider provider, Document document, TextSpan textSpan, string description)
        {
            return provider.GetRefactoringsAsync(document, textSpan, CancellationToken.None).Result.Where(a => a.Description == description).Single();
        }

        public static ApplyChangesOperation GetApplyChangesOperation(this CodeAction codeAction)
        {
            return codeAction.GetOperationsAsync(CancellationToken.None).Result.OfType<ApplyChangesOperation>().Single();
        }

        public static IEnumerable<int> IndexesOf(this string str, char c)
        {
            for (int position = 0; position < str.Length; position++)
            {
                char x = str[position];
                if (x == c)
                {
                    yield return position;
                }
            }
        }

        public static Project ForEachDocument(this Project project, Func<Document, Document> func)
        {
            Solution projectSolution = project.Solution;

            foreach (var projectDoc in project.Documents)
            {
                var newDocument = func(projectDoc);

                projectSolution = projectSolution.WithDocumentSyntaxRoot(projectDoc.Id, newDocument.GetSyntaxRootAsync().Result);
            }

            return projectSolution.GetProject(project.Id);
        }

        public static Solution ForEachDocument(this Solution solution, Func<Document, Document> func)
        {
            Solution newSolution = solution;

            foreach (var project in solution.Projects)
            {
                var newProj = newSolution.GetProject(project.Id);

                newProj = newProj.ForEachDocument(func);

                newSolution = newProj.Solution;
            }

            return newSolution;
        }
    }
}