using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace POCEditorClassifier
{
    internal static class POCEditorClassifierClassificationDefinition
    {
        /// <summary>
        /// Defines the "POCEditorClassifier" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("POCEditorClassifier")]
        internal static ClassificationTypeDefinition POCEditorClassifierType = null;
    }
}
