using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CITCodeFasterNET.Test.Infrastructure.SourceCode
{
    public class UsingCodeBuilder : BaseSourceCodeBuilder
    {
        private string usingName;
        private string usingAlias;

        public UsingCodeBuilder(string usingName, string alias = null)
        {
            WithName(usingName);
            WithAlias(alias);
        }

        public UsingCodeBuilder WithName(string usingName)
        {
            this.usingName = usingName;

            return this;
        }

        public UsingCodeBuilder WithAlias(string alias)
        {
            this.usingAlias = alias;

            return this;
        }

        public override string Build(int identTabs = 0)
        {
            buildCode(identTabs);

            return base.Build(identTabs);
        }

        private void buildCode(int currentIdentTabs = 0)
        {
            SourceBuilder.AppendFormat("using {0}{1};", 
                (!string.IsNullOrWhiteSpace(usingAlias)) ? string.Format("{0} = ", usingAlias) : string.Empty,
                usingName
            );
        }

        public static UsingCodeBuilder New(string usingName, string alias = null)
        {
            return new UsingCodeBuilder(usingName, alias);
        }
    }
}
