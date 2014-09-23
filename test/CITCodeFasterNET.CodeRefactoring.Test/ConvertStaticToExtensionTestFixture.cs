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

namespace CITCodeFasterNET.CodeRefactoring.Test
{
    [TestClass]
    public class ConvertStaticToExtensionFixture
    {
        [TestMethod]
        public void should_001()
        {
            // Arrange
            var testCase = TestHelper.CreateTestDocumentWithClassAditionalContent(
@"public static Queue<char> Con$vertToQueue(string str)
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

    var queuedString =  ConvertToQueue(strTest);
}");

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
