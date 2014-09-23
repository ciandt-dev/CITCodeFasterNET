using CITCodeFasterNET.CodeRefactoring.MakeExplicit;
using CITCodeFasterNET.InfraStructure;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CITCodeFasterNET.Test.Infrastructure
{
    public static class TestHelper
    {
        public static TestCase CreateTestDocumentWithDeclaration(string declaration)
        {
            var source = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    public class TestClass
    {
        public static void Evaluate()
        {
            $replaceString
        } 

        private static string StringMethod()
        {
            return " + "\"String\"" + @";
        }
    }

    public class TestObject
    {
        
    }
}";
            var testCode = new TestCode(source.Replace("$replaceString", declaration));
            ProjectId projectId = ProjectId.CreateNewId();
            DocumentId documentId = DocumentId.CreateNewId(projectId);

            var solution = new CustomWorkspace().CurrentSolution
                .AddProject(projectId, "MyProject", "MyProject", LanguageNames.CSharp)
                .AddMetadataReference(projectId, CommonMetadataReferences.System)
                .AddDocument(documentId, "MyFile.cs", testCode.Text);

            return new TestCase() { Document = solution.GetDocument(documentId), Span = testCode.NodesAtCursorMarkers.First().Span };
        }

        public static string ApplyRefactory(TestCase TestCase)
        {
            var codeAction = new MakeExplicitProvider().GetCodeActionByDescription(TestCase.Document, TestCase.Span, "Make explicit");

            var codeActionOperation = codeAction.GetApplyChangesOperation();

            var changedSolution = codeActionOperation.ChangedSolution;

            var changedDocument = changedSolution.GetDocument(TestCase.Document.Id);

            return changedDocument.GetTextAsync().Result.ToString();
        }
    }
}
