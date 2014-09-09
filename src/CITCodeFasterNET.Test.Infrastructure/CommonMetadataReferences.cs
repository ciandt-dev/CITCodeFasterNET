using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CITCodeFasterNET.Test.Infrastructure
{
    public static class CommonMetadataReferences
    {
        public static MetadataReference System
        {
            get
            {
                return new MetadataFileReference(typeof(object).Assembly.Location); ;
            }
        }
    }
}
