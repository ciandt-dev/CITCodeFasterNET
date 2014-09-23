using CITCodeFasterNET.CodeRefactoring.MakeExplicit;
using CITCodeFasterNET.InfraStructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CITCodeFasterNET.Test.Infrastructure
{
    public static class TestHelper
    {
        public static TestCase CreateTestDocumentWithVoidMethodBodyContent(string bodyContent)
        {
            var source = @"
using System;
using System.Collections.Generic;
using TestDifferentNamespace;

namespace TestNamespace
{
    public class TestClass
    {
        public static void Evaluate()
        {
            $bodyContent
        } 

        private static string StringMethod()
        {
            return " + "\"String\"" + @";
        }
    }

    public class TestObject
    {
        
    }
}

namespace TestDifferentNamespace {
    public class TestObjectDifferentNamespace
    {
        
    }
}
";
            var testCode = new TestCode(source.Replace("$bodyContent", bodyContent));
            ProjectId projectId = ProjectId.CreateNewId();
            DocumentId documentId = DocumentId.CreateNewId(projectId);

            var solution = new CustomWorkspace().CurrentSolution
                .AddProject(projectId, "MyProject", "MyProject", LanguageNames.CSharp)
                .AddMetadataReference(projectId, CommonMetadataReferences.System)
                .AddDocument(documentId, "MyFile.cs", testCode.Text);

            return new TestCase() { Document = solution.GetDocument(documentId), Span = testCode.NodesAtCursorMarkers.First().Span };
        }

        public static TestCase CreateTestDocumentWithClassAditionalContent(string aditionalContent)
        {
            var source = @"
using System;
using System.Collections.Generic;
using TestDifferentNamespace;

namespace TestNamespace
{
    public static class TestClass
    {
        $aditionalContent
    }
}
";
            var testCode = new TestCode(source.Replace("$aditionalContent", aditionalContent));
            ProjectId projectId = ProjectId.CreateNewId();
            DocumentId documentId = DocumentId.CreateNewId(projectId);

            var solution = new CustomWorkspace().CurrentSolution
                .AddProject(projectId, "MyProject", "MyProject", LanguageNames.CSharp)
                .AddMetadataReference(projectId, CommonMetadataReferences.System)
                .AddDocument(documentId, "MyFile.cs", testCode.Text);

            return new TestCase() { Document = solution.GetDocument(documentId), Span = testCode.NodesAtCursorMarkers.First().Span };
        }

        public static string ApplyRefactory<TCodeRefectoryProvider>(string codeActionDescription, TestCase TestCase)
            where TCodeRefectoryProvider : ICodeRefactoringProvider
        {
            var codeRefactProvider = Activator.CreateInstance<TCodeRefectoryProvider>();

            var codeAction = codeRefactProvider.GetCodeActionByDescription(TestCase.Document, TestCase.Span, codeActionDescription);

            var codeActionOperation = codeAction.GetApplyChangesOperation();

            var changedSolution = codeActionOperation.ChangedSolution;

            var changedDocument = changedSolution.GetDocument(TestCase.Document.Id);

            return changedDocument.GetTextAsync().Result.ToString();
        }
    }
}
