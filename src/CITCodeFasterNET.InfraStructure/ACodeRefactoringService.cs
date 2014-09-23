using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CITCodeFasterNET.InfraStructure
{
    public abstract class ACodeRefactoringService
    {
        protected Document Document { get; private set; }
        protected TextSpan TextSpan { get; private set; }
        protected SyntaxNode Node { get; private set; }
        public abstract string Description { get; }

        public ACodeRefactoringService(Document document, TextSpan textSpan)
        {
            Document = document;
            TextSpan = textSpan;

            var root = document.GetSyntaxRootAsync().Result;

            // Find the node at the selection.
            Node = root.FindNode(textSpan);
        }


        public abstract Document Execute();
        public virtual async Task<Document> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Execute();
        }
        public abstract bool IsAppliable();
    }
}