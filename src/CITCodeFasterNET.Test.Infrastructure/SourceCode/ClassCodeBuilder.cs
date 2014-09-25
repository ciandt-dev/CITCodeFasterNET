using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CITCodeFasterNET.Test.Infrastructure.SourceCode
{
    public class ClassCodeBuilder : TypeCodeBuilder, ITypeCodeBuilderContainer<ClassCodeBuilder>
    {
        private CodeModifier[] modifiers;
        private string name;

        public IList<FieldDeclarationCodeBuilder> Fields { get; private set; }
        public IList<TypeCodeBuilder> TypeCodes { get; private set; }

        public ClassCodeBuilder(string name, CodeModifier[] modifiers)
        {
            Fields = new List<FieldDeclarationCodeBuilder>();
            TypeCodes = new List<TypeCodeBuilder>();

            WithName(name);
            WithModifiers(modifiers);
        }

        public void WithModifiers(params CodeModifier[] modifiers)
        {
            this.modifiers = modifiers;
        }

        public void WithName(string name)
        {
            this.name = name;
        }

        public ClassCodeBuilder WithTypes(params TypeCodeBuilder[] typeCodeBuilders)
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

        public ClassCodeBuilder WithFields(params FieldDeclarationCodeBuilder[] fields)
        {
            if (fields != null)
            {
                foreach (var item in fields)
                {
                    Fields.Add(item);
                }
            }

            return this;
        }

        public override string Build(int identTabs = 0)
        {
            buildCode(identTabs);

            return base.Build(identTabs);
        }

        private void buildCode(int identTabs = 0)
        {
            var codeModifiers = modifiers.DistinctByGroups().OrderModifiers();

            if (codeModifiers != null)
            {
                foreach (var mod in codeModifiers)
                {
                    SourceBuilder.AppendFormat("{0} ", mod.GetCodeValue());
                }
            }

            SourceBuilder.AppendLine(string.Format("class {0}", this.name));
            SourceBuilder.AppendLine("{");

            buildFields(identTabs);

            buildTypeCodes(identTabs);

            SourceBuilder.Append("}");
        }

        private void buildFields(int currentTabs = 0)
        {
            foreach (var item in Fields)
            {
                SourceBuilder.AppendLine(item.Build(currentTabs));
            }
        }

        private void buildTypeCodes(int currentTabs = 0)
        {
            foreach (var item in TypeCodes)
            {
                SourceBuilder.AppendLine(item.Build(currentTabs));
            }
        }

        public static ClassCodeBuilder New(string name, params CodeModifier[] modifiers)
        {
            return new ClassCodeBuilder(name, modifiers);
        }
    }
}
