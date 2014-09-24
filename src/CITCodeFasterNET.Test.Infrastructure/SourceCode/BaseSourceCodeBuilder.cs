using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CITCodeFasterNET.Test.Infrastructure.SourceCode
{
    public abstract class BaseSourceCodeBuilder
    {
        protected StringBuilder SourceBuilder { get; private set; }

        public BaseSourceCodeBuilder()
        {
            SourceBuilder = new StringBuilder();
        }

        protected virtual string ApplyLineTabIndentations(string sourceString, int tabNumbers = 0)
        {
            string strResult = String.Empty;

            var listTabs = new List<string>();
            for (int i = 0; i < tabNumbers; i++)
            {
                listTabs.Add("\t");
            }

            strResult = Regex.Replace(sourceString, @"(^([ ])*)", string.Join(string.Empty, listTabs.ToArray()), RegexOptions.Multiline);

            return strResult;
        }

        public virtual string Build(int identTabs = 0)
        {
            return ApplyLineTabIndentations(SourceBuilder.ToString(), identTabs);
        }
    }
}
