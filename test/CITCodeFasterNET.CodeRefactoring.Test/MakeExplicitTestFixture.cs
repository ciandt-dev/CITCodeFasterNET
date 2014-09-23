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
        const string CodeActionDescription = "Make explicit";

        #region Source creation methods 


        #endregion Source creation methods 


        [TestMethod]
        public void should_refactory_when_value_is_primitive_int_constant()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithVoidMethodBodyContent("va$r i = 0;");

            // Act
            var finalText = TestHelper.ApplyRefactory<MakeExplicitProvider>(CodeActionDescription, testCase);

            // Assert
            Assert.IsTrue(finalText.Contains("int i = 0;"));
        }

        [TestMethod]
        public void should_refactory_when_value_is_primitive_string_constant()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithVoidMethodBodyContent("va$r i = \"0\";");

            // Act
            var finalText = TestHelper.ApplyRefactory<MakeExplicitProvider>(CodeActionDescription, testCase);

            // Assert
            Assert.IsTrue(finalText.Contains("string i = \"0\";"));
        }

        [TestMethod]
        public void should_refactory_when_value_is_primitive_bool_constant()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithVoidMethodBodyContent("v$ar tBoolean = true;");

            // Act
            var finalText = TestHelper.ApplyRefactory<MakeExplicitProvider>(CodeActionDescription, testCase);

            // Assert
            Assert.IsTrue(finalText.Contains("bool tBoolean = true;"));
        }

        [TestMethod]
        public void should_refactory_when_value_is_primitive_char_constant()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithVoidMethodBodyContent("$var tChar = 'C';");

            // Act
            var finalText = TestHelper.ApplyRefactory<MakeExplicitProvider>(CodeActionDescription, testCase);

            // Assert
            Assert.IsTrue(finalText.Contains("char tChar = 'C';"));
        }

        [TestMethod]
        public void should_refactory_when_value_is_a_system_complex_type()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithVoidMethodBodyContent("v$ar date = DateTime.Now;");

            // Act
            var finalText = TestHelper.ApplyRefactory<MakeExplicitProvider>(CodeActionDescription, testCase);

            // Assert
            Assert.IsTrue(finalText.Contains("DateTime date = DateTime.Now;"));
        }

        [TestMethod]
        public void should_refactory_when_value_is_a_user_defined_complex_type()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithVoidMethodBodyContent("v$ar testObj = new TestObject();");

            // Act
            var finalText = TestHelper.ApplyRefactory<MakeExplicitProvider>(CodeActionDescription, testCase);

            // Assert
            Assert.IsTrue(finalText.Contains("TestObject testObj = new TestObject();"));
        }

        [TestMethod]
        public void should_refactory_when_value_is_a_user_defined_complex_type_in_different_namespace()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithVoidMethodBodyContent("v$ar testObj = new TestObjectDifferentNamespace();");

            // Act
            var finalText = TestHelper.ApplyRefactory<MakeExplicitProvider>(CodeActionDescription, testCase);

            // Assert
            Assert.IsTrue(finalText.Contains("TestObjectDifferentNamespace testObj = new TestObjectDifferentNamespace();"));
        }

        [TestMethod]
        public void should_refactory_when_value_is_a_primitive_fixed_array()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithVoidMethodBodyContent("va$r tArrayFixed = new int[10];");

            // Act
            var finalText = TestHelper.ApplyRefactory<MakeExplicitProvider>(CodeActionDescription, testCase);

            // Assert
            Assert.IsTrue(finalText.Contains("int[] tArrayFixed = new int[10];"));
        }

        [TestMethod]
        public void should_refactory_when_value_is_a_primitive_fixed_single_position_array()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithVoidMethodBodyContent("va$r tArrayFixed = new int[0];");

            // Act
            var finalText = TestHelper.ApplyRefactory<MakeExplicitProvider>(CodeActionDescription, testCase);

            // Assert
            Assert.IsTrue(finalText.Contains("int[] tArrayFixed = new int[0];"));
        }

        [TestMethod]
        public void should_refactory_when_value_is_a_primitive_variable_array()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithVoidMethodBodyContent("v$ar tArray = new int[] { 1 };");

            // Act
            var finalText = TestHelper.ApplyRefactory<MakeExplicitProvider>(CodeActionDescription, testCase);

            // Assert
            Assert.IsTrue(finalText.Contains("int[] tArray = new int[] { 1 };"));
        }

        [TestMethod]
        public void should_refactory_when_value_is_the_return_of_a_function()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithVoidMethodBodyContent("$var stringVar = StringMethod();");

            // Act
            var finalText = TestHelper.ApplyRefactory<MakeExplicitProvider>(CodeActionDescription, testCase);

            // Assert
            Assert.IsTrue(finalText.Contains("string stringVar = StringMethod();"));
        }
       
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void should_NOT_refactory_when_there_are_compilation_errors()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithVoidMethodBodyContent("$var stringVar = StringMethod()");

            // Act
            TestHelper.ApplyRefactory<MakeExplicitProvider>(CodeActionDescription, testCase);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void should_NOT_refactory_when_value_type_is_anonymous()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithVoidMethodBodyContent("$var dynVar = new { s = 1; };");

            // Act
            TestHelper.ApplyRefactory<MakeExplicitProvider>(CodeActionDescription, testCase);
        }

        [TestMethod]
        public void should_refactory_when_value_is_type_dynamic()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithVoidMethodBodyContent("$var dynVar = 1 as dynamic;");

            // Act
            var finalText = TestHelper.ApplyRefactory<MakeExplicitProvider>(CodeActionDescription, testCase);

            // Assert
            Assert.IsTrue(finalText.Contains("dynamic dynVar = 1 as dynamic;"));
        }

        [TestMethod]
        public void should_refactory_when_value_type_is_generic()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithVoidMethodBodyContent("$var genericVar = new Dictionary<int, List<Tuple<int, int, string>>>();");

            // Act
            var finalText = TestHelper.ApplyRefactory<MakeExplicitProvider>(CodeActionDescription, testCase);

            // Assert                       
            Assert.IsTrue(finalText.Contains("Dictionary<int, List<Tuple<int, int, string>>> genericVar = new Dictionary<int, List<Tuple<int, int, string>>>();"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void should_NOT_refactory_when_value_is_not_set()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithVoidMethodBodyContent("$var emptyVar;");

            // Act
            TestHelper.ApplyRefactory<MakeExplicitProvider>(CodeActionDescription, testCase);
        }
    }
}
