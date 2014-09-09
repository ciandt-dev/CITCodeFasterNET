using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Results;

namespace WebCSharpEditor.WebApp.Controllers.Api
{
    public class ScriptingContext
    {
        public ScriptingContext()
        {
        }
    }

    public class CSharpCompilerController : ApiController
    {
        private string workspaceFolder = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Workspace");

        [HttpPost]
        public JsonResult<object> CreateConsoleProject()
        {
            checkCreateProjectFolderFolder("ConsoleProject");

            return Json<object>(new {
                Status = "Project Created."
            });
        }

        [HttpPost]
        public JsonResult<object> BuildProject([FromBody]string sourceCode)
        {
            StringBuilder srtBuilder = new StringBuilder();

            srtBuilder.AppendLine("========== Build started ==========");
            srtBuilder.AppendLine();

            IEnumerable<Diagnostic> errorsAndWarnings = buildProject(sourceCode);

            errorsAndWarnings.Select(a => string.Format("[{0}]: {1}", a.Severity, a.GetMessage())).ToList().ForEach((item =>
            {
                srtBuilder.AppendLine(item);
            }));

            srtBuilder.AppendLine();

            if (!errorsAndWarnings.Any(a => a.Severity == DiagnosticSeverity.Error))
            {
                srtBuilder.AppendLine(string.Format("========== Build succeeded =========="));
            }
            else
            {
                srtBuilder.AppendLine(string.Format("========== Build finished with errors =========="));
            }

            return Json<object>(new
            {
                Status = srtBuilder.ToString()
            });
        }

        [HttpPost]
        public JsonResult<object> RunProject()
        {
            if (!File.Exists(getOuputAssembly()))
            {
                return Json<object>(new
                {
                    Status = "You need to Build the project before!!!"
                });
            }

            StringBuilder srtBuilder = new StringBuilder();

            using (var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = getOuputAssembly(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            })
            {
                proc.Start();
                while (!proc.StandardOutput.EndOfStream)
                {
                    srtBuilder.AppendLine(proc.StandardOutput.ReadLine());
                }

                proc.Close();
            }

            return Json<object>(new
            {
                Output = srtBuilder.ToString()
            });
        }

        private IEnumerable<Diagnostic> buildProject(string sourceCode)
        {
            var tree = SyntaxFactory.ParseSyntaxTree(sourceCode);

            var compilation = CSharpCompilation
                .Create("ConsoleProject.exe")
                .AddSyntaxTrees(tree)
                .AddReferences(new MetadataFileReference(typeof(object).Assembly.Location));

            IEnumerable<Diagnostic> errorsAndWarnings = compilation.GetDiagnostics();

            if (!errorsAndWarnings.Any(a => a.Severity == DiagnosticSeverity.Error))
            {
                using (StreamWriter binaryWriter = new StreamWriter(getOuputAssembly()))
                {
                    compilation.Emit(binaryWriter.BaseStream);
                }
            }
            else
            {
                if (File.Exists(getOuputAssembly()))
                {
                    File.Delete(getOuputAssembly());
                }
            }

            return errorsAndWarnings;
        }

        private string getOuputAssembly()
        {
            return Path.Combine(getProjectFolder("ConsoleProject"), "ConsoleProject.exe");
        }

        private void checkCreateWorkspaceFolder()
        {
            if (!Directory.Exists(workspaceFolder))
            {
                Directory.CreateDirectory(workspaceFolder);
            }
        }

        private string getProjectFolder(string projectName)
        {
            return Path.Combine(workspaceFolder, projectName);
        }

        private void checkCreateProjectFolderFolder(string projectName)
        {
            checkCreateWorkspaceFolder();

            if (!Directory.Exists(getProjectFolder(projectName)))
            {
                Directory.CreateDirectory(getProjectFolder(projectName));
            }
        }
    }
}
