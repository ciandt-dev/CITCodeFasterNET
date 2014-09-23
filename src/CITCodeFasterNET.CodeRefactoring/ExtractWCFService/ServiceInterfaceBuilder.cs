using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeGeneration;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CITCodeFasterNET.InfraStructure;

namespace CITCodeFasterNET.CodeRefactoring.ExtractWCFService
{
    public class ServiceInterfaceBuilder
    {
        public static Solution CreateInterface(Solution solution, Project targetProject, ClassDeclarationSyntax classDecl)
        {
            var interfaceName = "I" + classDecl.Identifier + "Service";
            var interfaceFileName = string.Concat(interfaceName, ".cs");
            var targetNamespace = classDecl.Parent.AncestorsAndSelf().OfType<NamespaceDeclarationSyntax>().First();

            IList<SimpleNameSyntax> complexTypes = new List<SimpleNameSyntax>();

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

                document = document.SimplifyDocument().FormatDocument();

                formattedSolution = document.Project.Solution;
            }
            return formattedSolution;
        }

        private static Solution AddDataContractAttributes(Solution solution, IList<SimpleNameSyntax> complexTypes, List<DocumentId> changedDocs)
        {
            Solution newSolution = solution;

            var visitorDataContract = new DataContractElegibleComplexTypeWalker(complexTypes, solution.Workspace, null);

            foreach (var project in solution.Projects)
            {
                var newProj = newSolution.GetProject(project.Id);
                foreach (var projectDoc in newProj.Documents)
                {
                    var originalNode = projectDoc.GetSyntaxRootAsync().Result;
                    visitorDataContract.Visit(originalNode);
                }
            }

            var visitorDataContractRewriter = new DataContractElegibleComplexTypeRewriter(visitorDataContract.ElegibleClasses, newSolution.Workspace);

            foreach (var project in solution.Projects)
            {
                var newProj = newSolution.GetProject(project.Id);
                foreach (var projectDoc in newProj.Documents)
                {
                    var originalNode = projectDoc.GetSyntaxRootAsync().Result;
                    var newNode = visitorDataContractRewriter.Visit(originalNode);
                    projectDoc.WithSyntaxRoot(newNode);

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

            document = document.SimplifyDocument().FormatDocument();

            return document.Project.Solution;
        }

        public static Solution AddReferencesToProject(Solution solution, Project project)
        {
            var newProject = solution.GetProject(project.Id);

            newProject = AddReferenceToProject(newProject, "System.ServiceModel");
            newProject = AddReferenceToProject(newProject, "System.Runtime.Serialization");

            return newProject.Solution;
        }

        private static Project AddReferenceToProject(Project newProject, string projectName)
        {
            var svcModelLoc = Assembly.LoadWithPartialName(projectName).Location;

            if (!newProject.MetadataReferences.Any(a => Path.GetFileName(a.Display).Equals(Path.GetFileName(svcModelLoc), StringComparison.InvariantCultureIgnoreCase)))
            {
                var metaRef = new MetadataFileReference(svcModelLoc);

                newProject = newProject.AddMetadataReference(metaRef);
            }

            return newProject;
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

        private static void ExtractComplexTypes(IList<SimpleNameSyntax> complexTypes, MethodDeclarationSyntax methodDeclSyntax)
        {
            if ((methodDeclSyntax.ReturnType is IdentifierNameSyntax) && (!complexTypes.Any(t => 
                t.Identifier.ValueText.Equals((methodDeclSyntax.ReturnType as IdentifierNameSyntax).Identifier.ValueText))))
            {
                complexTypes.Add((methodDeclSyntax.ReturnType as IdentifierNameSyntax));
            }

            foreach (var param in methodDeclSyntax.ParameterList.Parameters.Where(p => 
                p.Type is IdentifierNameSyntax).Select(p => p.Type as IdentifierNameSyntax))
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

    public class DataContractElegibleComplexTypeRewriter : CSharpSyntaxRewriter
    {
        private readonly Dictionary<string, ClassDeclarationSyntax> elegibleComplexTypes;
        private readonly Workspace workspace;

        public DataContractElegibleComplexTypeRewriter(Dictionary<string, ClassDeclarationSyntax> elegibleComplexTypes, Workspace workspace)
        {
            this.elegibleComplexTypes = elegibleComplexTypes;
            this.workspace = workspace;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            NamespaceDeclarationSyntax namespaceDSyntax = node.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();

            string nodeFullName = string.Format("{0}.{1}", namespaceDSyntax.Name, node.Identifier.ValueText);

            if (elegibleComplexTypes.ContainsKey(nodeFullName))
            {
                List<AttributeData> attributeList = new List<AttributeData>();
                attributeList.Add(
                CodeGenerationSymbolFactory.CreateAttributeData(CodeGenerationSymbolFactory.CreateNamedTypeSymbol(null, Accessibility.Public, new SymbolModifiers(), TypeKind.Unknown, "DataContract")));
                var newNode = CodeGenerator.AddAttributes(node, workspace, attributeList);

                List<PropertyDeclarationSyntax> propertiesList = new List<PropertyDeclarationSyntax>();
                propertiesList = newNode.Members.OfType<PropertyDeclarationSyntax>().ToList();

                newNode = newNode.ReplaceNodes(propertiesList.Select(p => p as SyntaxNode), (originalNode, updatedNode) =>
                {
                    List<AttributeData> memberAttributeList = new List<AttributeData>();
                    memberAttributeList.Add(
                        CodeGenerationSymbolFactory.CreateAttributeData(CodeGenerationSymbolFactory.CreateNamedTypeSymbol(
                            null, Accessibility.Public, new SymbolModifiers(), TypeKind.Unknown, "DataMember")));

                    return CodeGenerator.AddAttributes(originalNode, workspace, memberAttributeList);
                });

                return newNode;
            }

            return base.VisitClassDeclaration(node);
        }
    }


    public class DataContractElegibleComplexTypeWalker : CSharpSyntaxWalker
    {
        Dictionary<string, ClassDeclarationSyntax> elegibleClasses = new Dictionary<string, ClassDeclarationSyntax>();

        IList<SimpleNameSyntax> elegibleComplexTypes;

        Workspace workspace;

        public Dictionary<string, ClassDeclarationSyntax> ElegibleClasses
        {
            get
            {
                return elegibleClasses;
            }

            set
            {
                elegibleClasses = value;
            }
        }

        public DataContractElegibleComplexTypeWalker(IList<SimpleNameSyntax> elegibleComplexTypes, Workspace workspace, Dictionary<string, ClassDeclarationSyntax> elegibleClasses)
            : base()
        {
            this.elegibleComplexTypes = elegibleComplexTypes;
            this.workspace = workspace;
            if (elegibleClasses != null)
            {
                this.elegibleClasses = elegibleClasses;
            }
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (elegibleComplexTypes.Any(cp => cp.Identifier.ValueText.Equals(node.Identifier.ValueText)))
            {
                NamespaceDeclarationSyntax namespaceDSyntax = node.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();

                string nodeFullName = string.Format("{0}.{1}", namespaceDSyntax.Name, node.Identifier.ValueText);

                if (ElegibleClasses.ContainsKey(nodeFullName))
                {
                    return;
                }

                elegibleClasses[nodeFullName] = node;

                List<PropertyDeclarationSyntax> propertiesList = new List<PropertyDeclarationSyntax>();
                propertiesList = node.Members.OfType<PropertyDeclarationSyntax>().ToList();

                var complexSubTypes = new List<SimpleNameSyntax>();

                foreach (var property in propertiesList)
                {
                    List<AttributeData> memberAttributeList = new List<AttributeData>();

                    if ((property.Type is SimpleNameSyntax))
                    {
                        complexSubTypes.AddRange(GetIdentifiersGenericList(property.Type));
                    }
                }

                if (complexSubTypes.Any())
                {
                    Solution newSolution = workspace.CurrentSolution;
                    var visitorDataContractVisitor = new DataContractElegibleComplexTypeWalker(complexSubTypes, workspace, this.elegibleClasses);

                    foreach (var project in newSolution.Projects)
                    {
                        var newProj = newSolution.GetProject(project.Id);
                        foreach (var projectDoc in newProj.Documents)
                        {
                            var originalNode = projectDoc.GetSyntaxRootAsync().Result;

                            visitorDataContractVisitor.Visit(originalNode);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a list of Identifiers, checking recursively the generic description
        /// </summary>
        /// <param name="simpleNameSyntax">A generic syntax property</param> 
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<SimpleNameSyntax> GetIdentifiersGenericList(TypeSyntax simpleNameSyntax)
        {
            var complexSubTypes = new List<SimpleNameSyntax>();

            if ((simpleNameSyntax is IdentifierNameSyntax))
            {
                complexSubTypes.Add((simpleNameSyntax as SimpleNameSyntax));
                return complexSubTypes;
            }

            if (simpleNameSyntax is GenericNameSyntax)
            {
                var genericNameSyntax = simpleNameSyntax as GenericNameSyntax;

                var argumentList = genericNameSyntax.TypeArgumentList;

                complexSubTypes.Add(genericNameSyntax);

                foreach (var item in argumentList.Arguments)
                {
                    complexSubTypes.AddRange(GetIdentifiersGenericList(item));
                }
            }

            return complexSubTypes;
        } 
    }
}
