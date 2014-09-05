// Guids.cs
// MUST match guids.h
using System;

namespace Company.POCVSPackage
{
    static class GuidList
    {
        public const string guidPOCVSPackagePkgString = "5ac1d09d-a823-4f05-aa27-fac0d81c8d68";
        public const string guidPOCVSPackageCmdSetString = "4b095bba-f40b-49eb-850e-74d0ce086a11";
        public const string guidToolWindowPersistanceString = "19f52e78-9656-4224-a303-bdee68cb7804";
        public const string guidPOCVSPackageEditorFactoryString = "59bc0300-c920-48b9-b518-caad9b0ca502";

        public static readonly Guid guidPOCVSPackageCmdSet = new Guid(guidPOCVSPackageCmdSetString);
        public static readonly Guid guidPOCVSPackageEditorFactory = new Guid(guidPOCVSPackageEditorFactoryString);
    };
}