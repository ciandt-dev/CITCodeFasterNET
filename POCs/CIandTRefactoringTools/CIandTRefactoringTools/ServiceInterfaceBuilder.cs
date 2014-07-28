using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeGeneration;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace CIandTRefactoringTools
{
    public class ServiceInterfaceBuilder
    {
        public static Solution CreateInterface(Solution solution, Project targetProject, ClassDeclarationSyntax classDecl)
        {
            var interfaceName = "I" + classDecl.Identifier + "Service";
            var interfaceFileName = string.Concat(interfaceName, ".cs");
            var targetNamespace = classDecl.Parent.AncestorsAndSelf().OfType<NamespaceDeclarationSyntax>().First();

            IList<IdentifierNameSyntax> complexTypes = new List<IdentifierNameSyntax>();

            foreach (var method in GetPublicMethods(classDecl))
            {
                ExtractComplexTypes(complexTypes, method);
            }

            List<DocumentId> changedDocs = new List<DocumentId>();
            Solution newSolution = AddDataContractAttributes(solution, complexTypes, changedDocs);

            var source = GetInterfaceSource(interfaceName, targetNamespace, classDecl);

            DocumentId documentId = DocumentId.CreateNewId(targetProject.Id);

            newSolution = newSolution.AddDocument(documentId, interfaceFileName, source);
            changedDocs.Add(documentId);

            return FormatDocs(changedDocs, newSolution);
        }

        private static Solution FormatDocs(List<DocumentId> changedDocs, Solution solution)
        {
            var formattedSolution = solution;
            foreach (DocumentId docId in changedDocs)
            {
                var document = formattedSolution.GetDocument(docId);
                document = Simplifier.ReduceAsync(document, Simplifier.Annotation).Result;
                document = Formatter.FormatAsync(document).Result;
                formattedSolution = document.Project.Solution;
            }
            return formattedSolution;
        }

        private static Solution AddDataContractAttributes(Solution solution, IList<IdentifierNameSyntax> complexTypes, List<DocumentId> changedDocs)
        {
            Solution newSolution = solution;
            var visitorDataContractRewriter = new DataContractElegibleComplexTypeRewriter(complexTypes, newSolution.Workspace);

            foreach (var project in solution.Projects)
            {
                var newProj = newSolution.GetProject(project.Id);
                foreach (var projectDoc in newProj.Documents)
                {
                    var originalNode = projectDoc.GetSyntaxRootAsync().Result;
                    var newNode = visitorDataContractRewriter.Visit(originalNode);
                    if (newNode != originalNode)
                    {
                        newNode = (newNode as CompilationUnitSyntax).WithUsing("System.Runtime.Serialization");
                        newSolution = newSolution.WithDocumentSyntaxRoot(projectDoc.Id, newNode);
                        changedDocs.Add(projectDoc.Id);
                    }
                }
            }

            return newSolution;
        }

        public static Solution CreateServiceClass(Solution solution, Project targetProject, ClassDeclarationSyntax classDecl)
        {
            var className = classDecl.Identifier + "Service";
            var interfaceName = "I" + classDecl.Identifier + "Service";
            var classFileName = string.Concat(className, ".cs");
            var targetNamespace = classDecl.Parent.AncestorsAndSelf().OfType<NamespaceDeclarationSyntax>().First();

            var source = GetServiceClassSource(className, interfaceName, targetNamespace, classDecl);

            DocumentId documentId = DocumentId.CreateNewId(targetProject.Id);

            var newSolution = solution.AddDocument(documentId, className, source);

            var document = newSolution.GetDocument(documentId);
            document = Simplifier.ReduceAsync(document, Simplifier.Annotation).Result;
            document = Formatter.FormatAsync(document).Result;

            return document.Project.Solution;
        }

        public static Solution AddReferencesToProject(Solution solution, Project project)
        {
            var newProject = solution.GetProject(project.Id);

            var svcModelLoc = Assembly.LoadWithPartialName("System.ServiceModel").Location;

            if (!newProject.MetadataReferences.Any(a => Path.GetFileName(a.Display).Equals(Path.GetFileName(svcModelLoc), StringComparison.InvariantCultureIgnoreCase)))
            {
                var metaRef = new MetadataFileReference(svcModelLoc);

                newProject = newProject.AddMetadataReference(metaRef);
            }

            return newProject.Solution;
        }

        private static string GetInterfaceSource(string interfaceName, NamespaceDeclarationSyntax targetNamespace, ClassDeclarationSyntax classDecl)
        {
            StringBuilder source = new StringBuilder();

            source.Append(GetUsingDirectives(classDecl));
            source.AppendLine("using System.ServiceModel;");
            source.AppendLine();
            source.AppendFormat("{0} {1}", targetNamespace.NamespaceKeyword, targetNamespace.Name);
            source.AppendLine("{");
            source.AppendLine("[ServiceContract]");
            source.AppendLine(string.Format("public interface {0}", interfaceName));
            source.AppendLine("{");

            foreach (MethodDeclarationSyntax method in GetPublicMethods(classDecl))
            {
                string parameters = ExtractParameters(method.ParameterList);
                string typeParameters = ExtractTypeParameters(method.TypeParameterList);
                source.AppendLine("[OperationContract]");
                source.AppendFormat("{0} {1}{2}({3}){4};", method.ReturnType, method.Identifier, typeParameters, parameters, method.ConstraintClauses.ToString());
                source.AppendLine();
                source.AppendLine();
            }

            source.AppendLine("}");
            source.AppendLine("}");

            return source.ToString();
        }

        private static void ExtractComplexTypes(IList<IdentifierNameSyntax> complexTypes, MethodDeclarationSyntax methodDeclSyntax)
        {
            if ((methodDeclSyntax.ReturnType is IdentifierNameSyntax) && (!complexTypes.Any(t => t.Identifier.ValueText.Equals((methodDeclSyntax.ReturnType as IdentifierNameSyntax).Identifier.ValueText))))
            {
                complexTypes.Add((methodDeclSyntax.ReturnType as IdentifierNameSyntax));
            }

            foreach (var param in methodDeclSyntax.ParameterList.Parameters.Where(p => p.Type is IdentifierNameSyntax).Select(p => p.Type as IdentifierNameSyntax))
            {
                if (!complexTypes.Any(t => t.Identifier.ValueText.Equals(param.Identifier.ValueText)))
                {
                    complexTypes.Add(param);
                }
            }
        }

        private static string GetServiceClassSource(string className, string interfaceName, NamespaceDeclarationSyntax targetNamespace, ClassDeclarationSyntax classDecl)
        {
            StringBuilder source = new StringBuilder();

            source.Append(GetUsingDirectives(classDecl));
            source.AppendLine("using System.ServiceModel;");
            source.AppendLine();
            source.AppendFormat("{0} {1}", targetNamespace.NamespaceKeyword, targetNamespace.Name);
            source.AppendLine("{");
            source.AppendLine(string.Format("public class {0}: {1}", className, interfaceName));
            source.AppendLine("{");

            source.Append(string.Format("public {0} {1}", classDecl.Identifier, classDecl.Identifier));
            source.AppendLine("{ get; set; }");
            source.AppendLine();
            foreach (MethodDeclarationSyntax method in GetPublicMethods(classDecl))
            {
                string parameters = ExtractParameters(method.ParameterList);
                string typeParameters = ExtractTypeParameters(method.TypeParameterList);
                source.AppendFormat("public {0} {1}{2}({3}){4}", method.ReturnType, method.Identifier, typeParameters, parameters, method.ConstraintClauses.ToString());
                source.AppendLine("{");
                if (method.ReturnType.ToString() != "void")
                    source.Append("return ");
                source.Append("this.");
                source.Append(AddBusinessMethodCall(method, classDecl));
                source.AppendLine();
                source.AppendLine("}");
                source.AppendLine();
                source.AppendLine();
            }

            source.AppendLine("}");
            source.AppendLine("}");

            return source.ToString();
        }

        private static string AddBusinessMethodCall(MethodDeclarationSyntax method, ClassDeclarationSyntax classDecl)
        {
            StringBuilder methodSource = new StringBuilder();
            methodSource.AppendFormat("{0}.{1}{2}(", classDecl.Identifier, method.Identifier, ExtractTypeParameters(method.TypeParameterList));

            List<string> listParameters = new List<string>();
            foreach (var parameter in method.ParameterList.Parameters)
            {
                StringBuilder parameterString = new StringBuilder();
                foreach (var modifier in parameter.Modifiers)
                    parameterString.AppendFormat("{0} ", modifier.Text);
                parameterString.Append(parameter.Identifier);
                listParameters.Add(parameterString.ToString());
            }

            methodSource.Append(string.Join(",", listParameters.ToArray()));
            methodSource.Append(");");

            return methodSource.ToString();
        }

        private static string GetUsingDirectives(ClassDeclarationSyntax classDecl)
        {
            StringBuilder usingDirectives = new StringBuilder();
            foreach (var usingDirective in classDecl.Parent.AncestorsAndSelf().OfType<CompilationUnitSyntax>().FirstOrDefault().Usings)
                usingDirectives.AppendLine(usingDirective.ToString());
            return usingDirectives.ToString();
        }

        private static IEnumerable<MethodDeclarationSyntax> GetPublicMethods(ClassDeclarationSyntax classDecl)
        {
            return classDecl.Members.OfType<MethodDeclarationSyntax>().Where(m => m.Modifiers.Any(p => p.ValueText.Equals("public")));
        }

        private static string ExtractParameters(ParameterListSyntax parameterList)
        {
            List<string> parameterListAsString = new List<string>();
            foreach (var parameter in parameterList.Parameters)
                parameterListAsString.Add(string.Format("{0} {1}{2}", parameter.Type, parameter.Identifier, parameter.Default));
            return string.Join(",", parameterListAsString.ToArray());
        }

        private static string ExtractTypeParameters(TypeParameterListSyntax typeParameterList)
        {
            if (typeParameterList == null)
                return string.Empty;

            List<string> typeParameterListAsString = new List<string>();
            foreach (var parameter in typeParameterList.Parameters)
                typeParameterListAsString.Add(parameter.Identifier.ToString());
            return string.Format("<{0}>", string.Join(",", typeParameterListAsString.ToArray()));
        }

        private static bool IsComplexType()
        {
            return false;
        }
    }

    public class DataContractElegibleComplexTypeRewriter(private IList<IdentifierNameSyntax> elegibleComplexTypes, private Workspace workspace) : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (elegibleComplexTypes.Any(cp => cp.Identifier.ValueText.Equals(node.Identifier.ValueText)))
            {
                //TODO Add DataMember in all public methods
                //TODO Add DataMember and DataContract in subclasses (Think about circular dependency)

                List<AttributeData> attributeList = new List<AttributeData>();
                attributeList.Add(
                CodeGenerationSymbolFactory.CreateAttributeData(CodeGenerationSymbolFactory.CreateNamedTypeSymbol(null, Accessibility.Public, new SymbolModifiers(), TypeKind.Unknown, "DataContract")));
                var newNode = CodeGenerator.AddAttributes(node, workspace, attributeList);
                return newNode;
            }

            return base.VisitClassDeclaration(node);
        }
    }

    
}
