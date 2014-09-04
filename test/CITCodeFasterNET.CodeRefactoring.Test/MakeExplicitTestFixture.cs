using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis;
using CITCodeFasterNET.CodeRefactoring.MakeExplicit;
using System.Threading;
using System.Linq;
using Microsoft.CodeAnalysis.CodeActions;

namespace CITCodeFasterNET.CodeRefactoring.Test
{
    [TestClass]
    public class MakeExplicitTestFixture
    {
        private MetadataReference mscorlib;
        private MetadataReference Mscorlib
        {
            get
            {
                if (mscorlib == null)
                {
                    mscorlib = new MetadataFileReference(typeof(object).Assembly.Location);
                }

                return mscorlib;
            }
        }

        [TestMethod]
        public void TestMethod1()
        {
            var source = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestClass
    {
        public static void Evaluate()
        {
            v$ar tBoolean = true;

            var tDateTime = DateTime.MinValue;

            var tChar = 'C';

            var tInt = 0;

            var tString = " + "\"Test\"" + @";

            //TODO: Handle this scenario.
            var tDynamic = new { tInt = 0, tString = " + "\"Test\"" + @" };

            //TODO: Handle this scenario.
            var tDictionary = new Dictionary<int, string>();

            //TODO: Handle this scenario.
            var tWTF = new List<Tuple<String, DateTime, int, Contact>>();
        } 
    }
}";
            var testCode = new TestCode(source);

            ProjectId projectId = ProjectId.CreateNewId();
            DocumentId documentId = DocumentId.CreateNewId(projectId);

            var solution = new CustomWorkspace().CurrentSolution
                .AddProject(projectId, "MyProject", "MyProject", LanguageNames.CSharp)
                .AddMetadataReference(projectId, Mscorlib)
                .AddDocument(documentId, "MyFile.cs", testCode.Text);

            var document = solution.GetDocument(documentId);

            var makeExplicit = new MakeExplicitProvider();

            var codeAction = makeExplicit.GetRefactoringsAsync(document, testCode.GetCursorMarkSpan(), CancellationToken.None).Result;

            var codeActionOperation = codeAction.FirstOrDefault().GetOperationsAsync(CancellationToken.None).Result.OfType<ApplyChangesOperation>().FirstOrDefault();

            var changedSolution = codeActionOperation.ChangedSolution;

            var changedDocument = changedSolution.GetDocument(documentId);

            var changedDocumentText = changedDocument.GetTextAsync().Result;

            var testChangedCode = new TestCode(changedDocumentText.ToString());

            var newTokenOnPos = testChangedCode.GetTokenOnPosition(testCode.CursorMarkNode.Span.Start);

            Assert.AreEqual(newTokenOnPos.Text, "bool");
        }
    }
}
