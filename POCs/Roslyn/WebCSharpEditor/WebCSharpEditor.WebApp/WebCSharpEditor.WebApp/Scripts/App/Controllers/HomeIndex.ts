/// <reference path="../../typings/jquery/jquery.d.ts" />
/// <reference path="../../typings/bootstrap/bootstrap.d.ts" />

module App.Controllers {

    export class HomeIdex {

        private cEditor: any;

        constructor() {

            this.initEventHandlers();
        }

        public initEventHandlers() {

            $("#create-console-project").on("click", () => {

                this.createConsoleProject();
            });

            $("#build-project").on("click", () => {

                this.buildProject();
            });

            $("#run-project").on("click", () => {

                this.runProject();
            });
        }

        public createConsoleProject() {

            $.post("http://localhost:54820/api/CSharpCompiler/CreateConsoleProject",
                {},
                (data, textStatus) => {

                    $("#init-toolbar").hide();

                    this.initializeProject();
                }
            );
        }

        public initializeProject(): void {

            $("#build-run-toolbar").show();

            $("#editor-area").show();

            this.initializeEditor();
        }

        public buildProject(): void {

            $("#console-output").val("");
            $("#console-output-panel").hide();

            $.ajax({
                url: "http://localhost:54820/api/CSharpCompiler/BuildProject",
                type: "POST",
                data: {
                    '': this.cEditor.getValue()
                },
                dataType: 'json',
                success: (data, textStatus) => {

                    $("#build-output-console").val(data.Status);
                    $("#build-output").show();
                },
                error: () => {
                }
            });
        }

        public runProject(): void {

            $("#build-output-console").val("");
            $("#build-output").hide();

            $.ajax({
                url: "http://localhost:54820/api/CSharpCompiler/RunProject",
                type: "Post",
                data: {},
                dataType: 'json',
                success: (data, textStatus) => {

                    if (data.Status) {

                        $("#build-output-console").val(data.Status);
                        $("#build-output").show();
                    } else {

                        $("#build-output-console").val("");
                        $("#build-output").hide();

                        $("#console-output").val(data.Output);
                        $("#console-output-panel").show();
                    }
                },
                error: () => {
                }
            });
        }

        public initializeEditor(): void {

            this.cEditor = CodeMirror.fromTextArea(document.getElementById("src-area"), {
                lineNumbers: true,
                matchBrackets: true,
                mode: "text/x-csharp"
            });
        }
    }
}

$(function () {

    var homeIndexController = new App.Controllers.HomeIdex();
});