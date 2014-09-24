using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CITCodeFasterNET.Test.Infrastructure.SourceCode
{
    public enum CodeModifier
    {
        [GroupPriority(1)]
        [ModifierCodeValue("private")]
        Private,
        [GroupPriority(1)]
        [ModifierCodeValue("internal")]
        Internal,
        [GroupPriority(1)]
        [ModifierCodeValue("protected")]
        Protected,
        [GroupPriority(1)]
        [ModifierCodeValue("public")]
        Public,
        [GroupPriority(2)]
        [ModifierCodeValue("abstract")]
        Abstract,
        [GroupPriority(2)]
        [ModifierCodeValue("sealed")]
        Sealed,
        [GroupPriority(2)]
        [ModifierCodeValue("virtual")]
        Virtual,
        [GroupPriority(2)]
        [ModifierCodeValue("override")]
        Override,
        [GroupPriority(2)]
        [ModifierCodeValue("static")]
        Static,
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    internal class ModifierCodeValueAttribute : Attribute
    {
        public string CodeValue { get; private set; }

        public ModifierCodeValueAttribute(string codeValue)
        {
            this.CodeValue = codeValue;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    internal class GroupPriorityAttribute : Attribute
    {
        public int Priority { get; private set; }

        public GroupPriorityAttribute(int priority)
        {
            Priority = priority;
        }
    }

    public static class CodeModifierExtensions
    {
        public static int GetGroupPriority(this CodeModifier enumValue)
        {
            int priority = 0;

            var groupPriorityAttributes = enumValue.GetCustomAttributes<GroupPriorityAttribute>();

            if (groupPriorityAttributes != null)
            {
                var groupPriorityAttribute = groupPriorityAttributes.FirstOrDefault();

                if (groupPriorityAttribute != null)
                {
                    priority = groupPriorityAttribute.Priority;
                }
            }

            return priority;
        }

        public static string GetCodeValue(this CodeModifier enumValue)
        {
            string codeValue = string.Empty;

            var modifierCodeValueAttributes = enumValue.GetCustomAttributes<ModifierCodeValueAttribute>();

            if (modifierCodeValueAttributes != null)
            {
                var modifierCodeValueAttribute = modifierCodeValueAttributes.FirstOrDefault();

                if (modifierCodeValueAttribute != null)
                {
                    codeValue = modifierCodeValueAttribute.CodeValue;
                }
            }

            return codeValue;
        }

        public static IEnumerable<CodeModifier> DistinctByGroups(this IEnumerable<CodeModifier> enumValues)
        {
            return enumValues.Distinct(new CodeModifierGroupPriorityEqualityComparer());
        }

        public static IEnumerable<CodeModifier> OrderModifiers(this IEnumerable<CodeModifier> enumValues)
        {
            return enumValues.OrderBy(m => m.GetGroupPriority());
        }
    }

    public static class EnumExtensions
    {
        public static IEnumerable<TAttributeType> GetCustomAttributes<TAttributeType>(this Enum enumValue)
            where TAttributeType : Attribute
        {
            IEnumerable<TAttributeType> attributes = new TAttributeType[0];

            var enumType = enumValue.GetType();
            var memInfo = enumType.GetMember(enumValue.ToString()).FirstOrDefault();

            if (memInfo != null)
            {
                attributes = memInfo.GetCustomAttributes(typeof(TAttributeType), false).OfType<TAttributeType>();
            }

            return attributes;
        }
    }

    internal class CodeModifierGroupPriorityEqualityComparer : IEqualityComparer<CodeModifier>
    {
        public bool Equals(CodeModifier x, CodeModifier y)
        {
            return (x.GetGroupPriority() == y.GetGroupPriority());
        }

        public int GetHashCode(CodeModifier obj)
        {
            return (int)obj;
        }
    }
}
