using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CITCodeFasterNET.Test.Infrastructure.SourceCode
{
    public class SourceCodeBuilder : BaseSourceCodeBuilder
    {
        protected IList<UsingCodeBuilder> Usings { get; private set; }
        protected IList<NamespaceCodeBuilder> Namespaces { get; private set; }

        public SourceCodeBuilder()
        {
            Usings = new List<UsingCodeBuilder>();
            Namespaces = new List<NamespaceCodeBuilder>();
        }

        public SourceCodeBuilder WithUsing(params UsingCodeBuilder[] codeBuilder)
        {
            if (codeBuilder != null)
            {
                foreach (var item in codeBuilder)
                {
                    Usings.Add(item);
                }
            }

            return this;
        }

        public SourceCodeBuilder WithNamespace(params NamespaceCodeBuilder[] codeBuilder)
        {
            if (codeBuilder != null)
            {
                foreach (var item in codeBuilder)
                {
                    Namespaces.Add(item);
                }
            }

            return this;
        }

        public override string Build(int identTabs = 0)
        {
            buildCode(identTabs);

            return base.Build(identTabs);
        }

        private void buildCode(int currentIdentTabs = 0)
        {
            buildUsings(currentIdentTabs);

            SourceBuilder.AppendLine();

            buildNamespaces(currentIdentTabs);
        }

        private void buildUsings(int currentIdentTabs = 0)
        {
            foreach (var item in Usings)
            {
                SourceBuilder.AppendLine(item.Build(currentIdentTabs));
            }
        }

        private void buildNamespaces(int currentIdentTabs = 0)
        {
            foreach (var item in Namespaces)
            {
                SourceBuilder.AppendLine(item.Build(currentIdentTabs));
                SourceBuilder.AppendLine();
            }
        }

        public static SourceCodeBuilder New()
        {
            return new SourceCodeBuilder();
        }
    }
}
