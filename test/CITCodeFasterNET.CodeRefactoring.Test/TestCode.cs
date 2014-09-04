using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CITCodeFasterNET.CodeRefactoring.Test
{
    /// <summary>
    /// Helper class to bundle together information about a piece of analyzed test code.
    /// </summary>
    public class TestCode
    {
        public int CursorMarkPosition { get; private set; }
        public SyntaxNode CursorMarkNode { get; private set; }
        public string Text { get; private set; }
        public SyntaxTree SyntaxTree { get; private set; }

        public TestCode(string textWithMarker)
        {
            // $ marks the position in source code. It's better than passing a manually calculated
            // int, and is just for test convenience. $ is a char that is used nowhere in the C#
            // language.
            CursorMarkPosition = textWithMarker.IndexOf('$');
            if (CursorMarkPosition != -1)
            {
                textWithMarker = textWithMarker.Remove(CursorMarkPosition, 1);
            }

            Text = textWithMarker;
            SyntaxTree = SyntaxFactory.ParseSyntaxTree(Text);

            if (CursorMarkPosition != -1)
            {
                CursorMarkNode = GetNode(GetTokenOnPosition(CursorMarkPosition).Span);
            }
        }

        public SyntaxNode GetNode(TextSpan span)
        {
            return SyntaxTree.GetRoot().FindNode(span);
        }

        public SyntaxToken GetTokenOnPosition(int position)
        {
            return SyntaxTree.GetRoot().FindToken(position);
        }

        public TextSpan GetCursorMarkSpan()
        {
            var token = SyntaxTree.GetRoot().FindToken(CursorMarkPosition);

            return token.Span;
        }
    }
}
