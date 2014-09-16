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
        #region Source creation methods 

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


        private string getOriginalSourceFor_002()
        {
            return @"
using System;

namespace TestNamespace
{
    public class TestClass
    {
        public void Method()
        {
            // faz algum processamento 
            const string stringTest = " + "\"\"" + @";
            
            $var variavel = StringMethod();
            
            $var inteiro = 12;
            
            $var variavelString = StringMethod();

            $var aa = true;

            va$r tArrayFixed = new int[10];

            $var tArrayFixed01 = new int[0];

            v$ar tArray = new int[] { 1 };
        }

        private string StringMethod()
        {
            return " + "\"String\"" + @";
        }
    }
}";
        }

        private string getExpectedSourceFor_002()
        {
            return @"
using System;

namespace TestNamespace
{
    public class TestClass
    {
        public void Method()
        {
            // faz algum processamento 
            const string stringTest = " + "\"\"" + @";

            string variavel = StringMethod();

            int inteiro = 12;

            string variavelString = StringMethod();

            bool aa = true;

            int[] tArrayFixed = new int[10];

            int[] tArrayFixed01 = new int[0];

            int[] tArray = new int[] { 1 };
        }

        private string StringMethod()
        {
            return " + "\"String\"" + @";
        }
    }
}";
        }

        #endregion Source creation methods 


        [TestMethod]
        public void should_be_able_to_make_explicit_primitive_implicit_types_001()
        {
            var originalSource = getOriginalSourceFor_001();
            var expectedSource = getExpectedSourceFor_001();

            TestProvider(originalSource, expectedSource);
        }

        [TestMethod]
        public void should_be_able_to_make_explicit_dynamic_array_generics_implicit_types_002()
        {
            var originalSource = getOriginalSourceFor_002();
            var expectedSource = getExpectedSourceFor_002();
            TestProvider(originalSource, expectedSource);
        }

        private void TestProvider(string originalSource, string expectedSource)
        {

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
