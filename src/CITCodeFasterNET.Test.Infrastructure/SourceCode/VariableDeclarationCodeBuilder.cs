using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CITCodeFasterNET.Test.Infrastructure.SourceCode
{
    public class VariableDeclarationCodeBuilder : BaseSourceCodeBuilder
    {
        private string identifierName;
        private string type;

        public VariableDeclarationCodeBuilder(string identifierName, string type = "var")
        {
            WithIdentifierName(identifierName);
            WithType(type);
        }

        public VariableDeclarationCodeBuilder WithIdentifierName(string identifierName)
        {
            this.identifierName = identifierName;

            return this;
        }

        public VariableDeclarationCodeBuilder WithType(string type)
        {
            this.type = type;

            return this;
        }

        public override string Build(int identTabs = 0)
        {
            buildCode(identTabs);

            return base.Build(identTabs);
        }

        private void buildCode(int identTabs)
        {
            SourceBuilder.AppendFormat("{0}{1} {2}{3};", string.Empty, type, identifierName, string.Empty);
        }
    }
}
