using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CITCodeFasterNET.Test.Infrastructure.SourceCode
{
    public class FieldDeclarationCodeBuilder : VariableDeclarationCodeBuilder
    {
        public FieldDeclarationCodeBuilder(string identifierName, string type = "object")
            : base(identifierName, type)
        {
        }

        public static FieldDeclarationCodeBuilder New(string identifierName, string type)
        {
            return new FieldDeclarationCodeBuilder(identifierName, type);
        }
    }
}
