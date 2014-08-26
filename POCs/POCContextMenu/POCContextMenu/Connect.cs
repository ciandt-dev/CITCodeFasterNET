using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;

namespace POCContextMenu
{
    /// <summary>The object for implementing an Add-in.</summary>
    /// <seealso class='IDTExtensibility2' />
    public class Connect : IDTExtensibility2, IDTCommandTarget
    {
        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
        public Connect()
        {
        }

        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            _applicationObject = (DTE2)application;
            _addInInstance = (AddIn)addInInst;
            if (connectMode == ext_ConnectMode.ext_cm_UISetup)
            {
                object[] contextGUIDS = new object[] { };
                Commands2 commands = (Commands2)_applicationObject.Commands;

                //Place the command on the tools menu.
                //Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
                Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar = ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars)["Code Window"];

                //This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
                //  just make sure you also update the QueryStatus/Exec method to include the new command names.
                try
                {
                    //Add a command to the Commands collection:
                    Command command = commands.AddNamedCommand2(_addInInstance, "POCContextMenu", "POCContextMenu", "Executes the command for POCContextMenu", true, 59, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

                    //Add a control for the command to the tools menu:
                    if ((command != null) )
                    {
                        command.AddControl(menuBarCommandBar, 1);
                    }
                }
                catch (System.ArgumentException)
                {
                    //If we are here, then the exception is probably because a command with that name
                    //  already exists. If so there is no need to recreate the command and we can 
                    //  safely ignore the exception.
                }
            }
        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
        }

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />		
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {
        }

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom)
        {
        }

        /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
        /// <param term='commandName'>The name of the command to determine state for.</param>
        /// <param term='neededText'>Text that is needed for the command.</param>
        /// <param term='status'>The state of the command in the user interface.</param>
        /// <param term='commandText'>Text requested by the neededText parameter.</param>
        /// <seealso class='Exec' />
        public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
        {
            if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                if (commandName == "POCContextMenu.Connect.POCContextMenu")
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                    return;
                }
            }
        }

        /// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
        /// <param term='commandName'>The name of the command to execute.</param>
        /// <param term='executeOption'>Describes how the command should be run.</param>
        /// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
        /// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
        /// <param term='handled'>Informs the caller if the command was handled or not.</param>
        /// <seealso class='Exec' />
        public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
        {
            handled = false;
            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                if (commandName == "POCContextMenu.Connect.POCContextMenu")
                {
                    var objTextDocument = _applicationObject.ActiveDocument.Object() as EnvDTE.TextDocument;
                    var objCursorTextPoint = objTextDocument.Selection.ActivePoint;

                    var objCodeElement = GetCodeElementAtTextPoint(vsCMElement.vsCMElementClass, _applicationObject.ActiveDocument.ProjectItem.FileCodeModel.CodeElements, objCursorTextPoint);

                    Console.WriteLine(_applicationObject.ActiveDocument.FullName);
                    handled = true;
                    return;
                }
            }
        }

        private EnvDTE.CodeElement GetCodeElementAtTextPoint(EnvDTE.vsCMElement eRequestedCodeElementKind, EnvDTE.CodeElements colCodeElements, EnvDTE.TextPoint objTextPoint)
        {
            EnvDTE.CodeElement objResultCodeElement = null;
            EnvDTE.CodeElements colCodeElementMembers;
            EnvDTE.CodeElement objMemberCodeElement;
            if (!(colCodeElements == null))
            {
                foreach (EnvDTE.CodeElement objCodeElement in colCodeElements)
                {
                    if (objCodeElement.StartPoint.GreaterThan(objTextPoint))
                    {
                        //  The code element starts beyond the point
                    }
                    else if (objCodeElement.EndPoint.LessThan(objTextPoint))
                    {
                        //  The code element ends before the point
                    }
                    else
                    {
                        //  The code element contains the point
                        if ((objCodeElement.Kind == eRequestedCodeElementKind))
                        {
                            //  Found
                            objResultCodeElement = objCodeElement;
                        }
                        //  We enter in recursion, just in case there is an inner code element that also 
                        //  satisfies the conditions, for example, if we are searching a namespace or a class
                        colCodeElementMembers = GetCodeElementMembers(objCodeElement);
                        objMemberCodeElement = GetCodeElementAtTextPoint(eRequestedCodeElementKind, colCodeElementMembers, objTextPoint);
                        if (!(objMemberCodeElement == null))
                        {
                            //  A nested code element also satisfies the conditions
                            objResultCodeElement = objMemberCodeElement;
                        }
                        break;
                    }
                }
            }
            return objResultCodeElement;
        }

        private EnvDTE.CodeElements GetCodeElementMembers(CodeElement objCodeElement)
        {
            EnvDTE.CodeElements colCodeElements = null;

            if ((objCodeElement.GetType() == typeof(EnvDTE.CodeNamespace)))
            {
                colCodeElements = ((EnvDTE.CodeNamespace)(objCodeElement)).Members;
            }
            else if ((objCodeElement.GetType() == typeof(EnvDTE.CodeType)))
            {
                colCodeElements = ((EnvDTE.CodeType)(objCodeElement)).Members;
            }
            else if ((objCodeElement.GetType() == typeof(EnvDTE.CodeFunction)))
            {
                colCodeElements = (objCodeElement as EnvDTE.CodeFunction).Parameters;
            }

            return colCodeElements;
        }

        private DTE2 _applicationObject;
        private AddIn _addInInstance;
    }
}