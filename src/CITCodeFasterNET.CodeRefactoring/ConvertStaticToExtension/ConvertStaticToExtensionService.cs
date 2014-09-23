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
    }
}
