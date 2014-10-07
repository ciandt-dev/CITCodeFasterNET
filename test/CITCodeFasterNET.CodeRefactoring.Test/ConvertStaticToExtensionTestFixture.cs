﻿using System;
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
        public void should_001()
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

public static bool TestExtension(this string str)
{
    return true;
}

public static void Test()
{
    var strTest = " + "\"My queued string.\"" + @";

    //var t01 = strTest.TestExtension();

    MethodClone(ConvertToQueue);
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
    }
}
