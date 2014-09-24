using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
Implementaion Template:

public IList<TypeCodeBuilder> TypeCodes { get; private set; }

public TCodeBuilder WithTypes(params TypeCodeBuilder[] typeCodeBuilders)
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

private void buildTypeCodes(int currentTabs = 0)
{
    foreach (var item in TypeCodes)
    {
        SourceBuilder.AppendLine(item.Build(currentTabs));
    }
}
*/

namespace CITCodeFasterNET.Test.Infrastructure.SourceCode
{
    public interface ITypeCodeBuilderContainer<TCodeBuilder>
        where TCodeBuilder : BaseSourceCodeBuilder
    {
        IList<TypeCodeBuilder> TypeCodes { get; }

        TCodeBuilder WithTypes(params TypeCodeBuilder[] typeCodeBuilders);
    }
}
