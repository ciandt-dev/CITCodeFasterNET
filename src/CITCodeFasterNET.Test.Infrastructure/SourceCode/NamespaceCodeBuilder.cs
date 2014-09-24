using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CITCodeFasterNET.Test.Infrastructure.SourceCode
{
    public class NamespaceCodeBuilder : BaseSourceCodeBuilder, ITypeCodeBuilderContainer<NamespaceCodeBuilder>
    {
        private string name;

        public IList<TypeCodeBuilder> TypeCodes { get; private set; }

        public NamespaceCodeBuilder(string name)
        {
            TypeCodes = new List<TypeCodeBuilder>();

            WithName(name);
        }

        public NamespaceCodeBuilder WithName(string name)
        {
            this.name = name;

            return this;
        }

        public NamespaceCodeBuilder WithTypes(params TypeCodeBuilder[] typeCodeBuilders)
        {
            if (typeCodeBuilders != null)
            {
                foreach (var item in typeCodeBuilders)
                {
                    TypeCodes.Add(item);
                }
            }

            return this;
        }

        public override string Build(int identTabs = 0)
        {
            buildCode(identTabs);

            return base.Build(identTabs);
        }

        private void buildCode(int currentTabs = 0)
        {
            SourceBuilder.AppendLine(string.Format("namespace {0}", this.name));
            SourceBuilder.AppendLine("{");

            buildTypeCodes(currentTabs + 1);

            SourceBuilder.Append("}");
        }

        private void buildTypeCodes(int currentTabs = 0)
        {
            foreach (var item in TypeCodes)
            {
                SourceBuilder.AppendLine(item.Build(currentTabs));
            }
        }

        public static NamespaceCodeBuilder New(string name)
        {
            return new NamespaceCodeBuilder(name);
        }
    }
}
