using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CITCodeFasterNET.Test.Infrastructure
{
    public class TestCase
    {
        public Document Document { get; set; }
        public TextSpan Span { get; set; }
    }
}