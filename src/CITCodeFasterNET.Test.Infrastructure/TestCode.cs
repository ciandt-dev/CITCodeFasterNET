using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CITCodeFasterNET.InfraStructure;

namespace CITCodeFasterNET.Test.Infrastructure
{
    /// <summary>
    /// Helper class to bundle together information about a piece of analyzed test code.
    /// </summary>
    public class TestCode
    {
        private IEnumerable<int> cursorMarkerPositions = new List<int>();
        private IEnumerable<SyntaxNode> nodesAtCursorMarkers = new List<SyntaxNode>();

        public IEnumerable<SyntaxNode> NodesAtCursorMarkers
        {
            get
            {
                return nodesAtCursorMarkers;
            }
            set
            {
                nodesAtCursorMarkers = value;
            }
        }

        public SyntaxNode CursorMarkNode { get; private set; }
        public string Text { get; private set; }
        public SyntaxTree SyntaxTree { get; private set; }


        public TestCode(string sourceCode)
        {
            Text = extractCursorMarkersPositions(sourceCode);

            SyntaxTree = SyntaxFactory.ParseSyntaxTree(Text);

            extractNodesAtCursorMarkers();
        }

        // $ marks the position in source code represents the keyboard cursor in the text.
        private string extractCursorMarkersPositions(string sourceCode)
        {
            string outText = sourceCode;

            var tmpCursorMarkerPositions = outText.IndexesOf('$');

            var listMarkerPositions = new List<int>();

            for (int i = 0; i < tmpCursorMarkerPositions.Count(); i++)
            {
                var markerIndex = outText.IndexOf('$');

                if (markerIndex >= 0)
                {
                    listMarkerPositions.Add(markerIndex);

                    outText = outText.Remove(markerIndex, 1);
                }
            }

            this.cursorMarkerPositions = listMarkerPositions.ToList();

            return outText;
        }

        private void extractNodesAtCursorMarkers()
        {
            var listNodes = new List<SyntaxNode>();

            if (cursorMarkerPositions.Any())
            {
                foreach (var markerPos in cursorMarkerPositions)
                {
                    listNodes.Add(GetNode(GetTokenOnPosition(markerPos).Span));
                }
            }

            this.nodesAtCursorMarkers = listNodes.ToList();
        }

        public SyntaxNode GetNode(TextSpan span)
        {
            return SyntaxTree.GetRoot().FindNode(span);
        }

        public SyntaxToken GetTokenOnPosition(int textPosition)
        {
            return SyntaxTree.GetRoot().FindToken(textPosition);
        }

        public TextSpan GetSpanOnPosition(int textPosition)
        {
            var token = SyntaxTree.GetRoot().FindToken(textPosition);

            return token.Span;
        }
    }
}
