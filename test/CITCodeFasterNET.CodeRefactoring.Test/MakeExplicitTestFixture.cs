using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis;
using CITCodeFasterNET.CodeRefactoring.MakeExplicit;
using System.Threading;
using System.Linq;
using Microsoft.CodeAnalysis.CodeActions;
using CITCodeFasterNET.Test.Infrastructure;
using CITCodeFasterNET.InfraStructure;

namespace CITCodeFasterNET.CodeRefactoring.Test
{
    [TestClass]
    public class MakeExplicitTestFixture
    {
        private string getOriginalSourceFor_001()
        {
            return @"
using System;

namespace TestNamespace
{
    public class TestClass
    {
        public static void Evaluate()
        {
            v$ar tBoolean = true;

            va$r tDateTime = DateTime.MinValue;

            $var tChar = 'C';

            var$ tInt = 0;

            v$ar tString = " + "\"Test\"" + @";
        } 
    }
}";
        }

        private string getExpectedSourceFor_001()
        {
            return @"
using System;

namespace TestNamespace
{
    public class TestClass
    {
        public static void Evaluate()
        {
            bool tBoolean = true;

            DateTime tDateTime = DateTime.MinValue;

            char tChar = 'C';

            int tInt = 0;

            string tString = " + "\"Test\"" + @";
        }
    }
}";
        }

        [TestMethod]
        public void should_be_able_to_make_explicit_primitive_implicit_types_001()
        {
            var originalSource = getOriginalSourceFor_001();
            var expectedSource = getExpectedSourceFor_001();

            var testCode = new TestCode(originalSource);

            ProjectId projectId = ProjectId.CreateNewId();
            DocumentId documentId = DocumentId.CreateNewId(projectId);

            var solution = new CustomWorkspace().CurrentSolution
                .AddProject(projectId, "MyProject", "MyProject", LanguageNames.CSharp)
                .AddMetadataReference(projectId, CommonMetadataReferences.System)
                .AddDocument(documentId, "MyFile.cs", testCode.Text);

            var document = solution.GetDocument(documentId);

            var makeExplicit = new MakeExplicitProvider();

            Solution changedSolution = solution;
            Document changedDocument = document;
            string changedDocumentText = null;

            foreach (var itemNode in testCode.NodesAtCursorMarkers)
            {
                var codeAction = makeExplicit.GetCodeActionByDescription(changedDocument, itemNode.Span, "Make explicit");

                var codeActionOperation = codeAction.GetApplyChangesOperation();

                changedSolution = codeActionOperation.ChangedSolution;

                changedDocument = changedSolution.GetDocument(documentId);

                changedDocumentText = changedDocument.GetTextAsync().Result.ToString();
            }

            Assert.AreEqual(changedDocumentText, expectedSource, true);
        }
    }
}
