using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis;
using CITCodeFasterNET.CodeRefactoring.MakeExplicit;
using System.Threading;
using System.Linq;
using Microsoft.CodeAnalysis.CodeActions;
using CITCodeFasterNET.Test.Infrastructure;
using CITCodeFasterNET.InfraStructure;
using System.Collections.Generic;
using CITCodeFasterNET.Test.Infrastructure.SourceCode;

namespace CITCodeFasterNET.CodeRefactoring.Test
{
    [TestClass]
    public class ConvertStaticToExtensionFixture
    {
        [TestMethod]
        public void should_000()
        {
            var sourceBuilder = SourceCodeBuilder.New()
                .WithUsing(
                    UsingCodeBuilder.New("System"),
                    UsingCodeBuilder.New("System.Collections.Generic"),
                    UsingCodeBuilder.New("System.Linq", "linq")
                )
                .WithNamespace(
                    NamespaceCodeBuilder.New("MyNameSpace")
                        .WithTypes(
                            ClassCodeBuilder.New("MyClass", CodeModifier.Public, CodeModifier.Abstract)
                                .WithFields(FieldDeclarationCodeBuilder.New("int", "testIntField"))
                                .WithTypes(
                                    ClassCodeBuilder.New("MyIntClass", CodeModifier.Internal)
                                        .WithTypes(
                                            ClassCodeBuilder.New("MyIntClass02", CodeModifier.Internal)
                                        )
                                )
                        ),
                    NamespaceCodeBuilder.New("MyNameSpace.SubSpace")
                );

            var sourceCode = sourceBuilder.Build();

            Console.WriteLine();
        }

        [TestMethod]
        public void should_refactory_when_static_call_in_same_class()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithClassAditionalContent(
StaticMethod() +
@"
public static void Test()
{
    var strTest = " + "\"My queued string.\"" + @";
 
    var queuedString = ConvertToQueue(strTest);

}
");

            // Act
            var finalText = TestHelper.ApplyRefactory<ConvertStaticToExtensionProvider>("Convert to extension method", testCase);

            // Assert
            Assert.IsTrue(finalText.Contains("var queuedString = strTest.ConvertToQueue();"));
        }


        [TestMethod]
        public void should_refactory_when_static_call_is_made_using_static_class()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithClassAditionalContent(
StaticMethod() +
@"
public static void Test()
{
    var strTest = " + "\"My queued string.\"" + @";
 
    var test = TestClass.ConvertToQueue(strTest, 10);
}
");

            // Act
            var finalText = TestHelper.ApplyRefactory<ConvertStaticToExtensionProvider>("Convert to extension method", testCase);

            // Assert
            Assert.IsTrue(finalText.Contains("var test = strTest.ConvertToQueue(10);"));
        }



        [TestMethod]
        public void should_refactory_when_static_call_is_made_as_a_parameter_using_static_class()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithClassAditionalContent(
StaticMethod() + @"
public static void Test()
{
    var strTest = " + "\"My queued string.\"" + @";
 
    Method(TestClass.ConvertToQueue(strTest));
}

public static void Method(Queue<char> queueChar)
{
    return;
}
");

            // Act
            var finalText = TestHelper.ApplyRefactory<ConvertStaticToExtensionProvider>("Convert to extension method", testCase);

            // Assert
            Assert.IsTrue(finalText.Contains("Method(strTest.ConvertToQueue());"));
        }


        [TestMethod]
        public void should_refactory_when_static_call_is_made_as_a_parameter_using_same_class_call()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithClassAditionalContent(
StaticMethod() + 
@"
public static void Test()
{
    var strTest = " + "\"My queued string.\"" + @";
 
    Method(ConvertToQueue(strTest));
}

public static void Method(Queue<char> queueChar)
{
    return;
}
");

            // Act
            var finalText = TestHelper.ApplyRefactory<ConvertStaticToExtensionProvider>("Convert to extension method", testCase);

            // Assert
            Assert.IsTrue(finalText.Contains("Method(strTest.ConvertToQueue());"));
        }




        [TestMethod]
        public void should_refactory_when_static_call_is_made_as_a_parameter_using_linq()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithClassAditionalContent(
StaticMethod() + 
@"
public static void Test()
{
    var strTest = " + "\"My queued string.\"" + @";
 
    MethodClone((x) => ConvertToQueue(x));
}
public static Queue<char> MethodClone(Func<string, Queue<char>> funcQueueChar)
{
    return funcQueueChar(" + "\"bla\"" + @");
}
");
            // Act
            var finalText = TestHelper.ApplyRefactory<ConvertStaticToExtensionProvider>("Convert to extension method", testCase);

            // Assert
            Assert.IsTrue(finalText.Contains("MethodClone((x) => x.ConvertToQueue());"));
        }

        [TestMethod]
        public void should_refactory_when_static_call_is_made_as_a_parameter_using_linq_2()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithClassAditionalContent(
StaticMethod() +
@"
public static void Test()
{
    var strTest = " + "\"My queued string.\"" + @";
 
    MethodClone((x) => { return ConvertToQueue(x); });

}

public static Queue<char> MethodClone(Func<string, Queue<char>> funcQueueChar)
{
    return funcQueueChar(" + "\"bla\"" + @");
}
");

            // Act
            var finalText = TestHelper.ApplyRefactory<ConvertStaticToExtensionProvider>("Convert to extension method", testCase);

            // Assert
            Assert.IsTrue(finalText.Contains("MethodClone((x) => { return x.ConvertToQueue(); });"));
        }


        [TestMethod]
        public void should_refactor_when_code_contains_several_refactory_occurrences()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithClassAditionalContent(
@"public static Queue<char> Con$vertToQueue(string str, int count = 0)
{
    var result = new Queue<char>();

    foreach (var strChar in str)
    {
        result.Enqueue(strChar);
    }

    return result;
}

public static void Test()
{
    var strTest = " + "\"My queued string.\"" + @";
 
    var queuedString = ConvertToQueue(strTest);
    var test = TestClass.ConvertToQueue(strTest, 10);
    Method(TestClass.ConvertToQueue(strTest));
    Method(ConvertToQueue(strTest));
    MethodClone((x) => ConvertToQueue(x));
    MethodClone((x) => { return ConvertToQueue(x); });

}

public static void Method(Queue<char> queueChar)
{
    return;
}

public static Queue<char> MethodClone(Func<string, Queue<char>> funcQueueChar)
{
    return funcQueueChar(" + "\"bla\"" + @");
}
");

            // Act
            var finalText = TestHelper.ApplyRefactory<ConvertStaticToExtensionProvider>("Convert to extension method", testCase);

            // Assert
            Assert.IsTrue(finalText.Contains(
    @"var queuedString = strTest.ConvertToQueue();
    var test = strTest.ConvertToQueue(10);
    Method(strTest.ConvertToQueue());
    Method(strTest.ConvertToQueue());
    MethodClone((x) => x.ConvertToQueue());
    MethodClone((x) => { return x.ConvertToQueue(); });"));
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void should_not_be_accessible_for_non_static_methods()
        {
            var testCase = TestHelper.CreateTestDocumentWithClassAditionalContent(
@"public Queue<char> Con$vertToQueue(string str)
{
    var result = new Queue<char>();

    foreach (var strChar in str)
    {
        result.Enqueue(strChar);
    }

    return result;
}");

            // Act
            var finalText = TestHelper.ApplyRefactory<ConvertStaticToExtensionProvider>("Convert to extension method", testCase);
        }



        private string StaticMethod()
        {
            return @"public static Queue<char> Con$vertToQueue(string str, int count = 0)
{
    var result = new Queue<char>();

    foreach (var strChar in str)
    {
        result.Enqueue(strChar);
    }

    return result;
}";
        }

    }


}
