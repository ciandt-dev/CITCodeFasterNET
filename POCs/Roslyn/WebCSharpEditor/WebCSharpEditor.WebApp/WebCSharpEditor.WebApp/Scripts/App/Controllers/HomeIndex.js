/// <reference path="../../typings/codemirror/codemirror.d.ts" />
/// <reference path="../../typings/jquery/jquery.d.ts" />
/// <reference path="../../typings/bootstrap/bootstrap.d.ts" />
var App;
(function (App) {
    (function (Controllers) {
        var HomeIdex = (function () {
            function HomeIdex() {
                this.initEventHandlers();
            }
            HomeIdex.prototype.initEventHandlers = function () {
                var _this = this;
                $("#create-console-project").on("click", function () {
                    _this.createConsoleProject();
                });

                $("#build-project").on("click", function () {
                    _this.buildProject();
                });

                $("#run-project").on("click", function () {
                    _this.runProject();
                });
            };

            HomeIdex.prototype.createConsoleProject = function () {
                var _this = this;
                $.post("http://localhost:54820/api/CSharpCompiler/CreateConsoleProject", {}, function (data, textStatus) {
                    $("#init-toolbar").hide();

                    _this.initializeProject();
                });
            };

            HomeIdex.prototype.initializeProject = function () {
                $("#build-run-toolbar").show();

                $("#editor-area").show();

                this.initializeEditor();
            };

            HomeIdex.prototype.buildProject = function () {
                $("#console-output").val("");
                $("#console-output-panel").hide();

                $.ajax({
                    url: "http://localhost:54820/api/CSharpCompiler/BuildProject",
                    type: "POST",
                    data: {
                        '': this.cEditor.getValue()
                    },
                    dataType: 'json',
                    success: function (data, textStatus) {
                        $("#build-output-console").val(data.Status);
                        $("#build-output").show();
                    },
                    error: function () {
                    }
                });
            };

            HomeIdex.prototype.runProject = function () {
                $("#build-output-console").val("");
                $("#build-output").hide();

                $.ajax({
                    url: "http://localhost:54820/api/CSharpCompiler/RunProject",
                    type: "Post",
                    data: {},
                    dataType: 'json',
                    success: function (data, textStatus) {
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
                    error: function () {
                    }
                });
            };

            HomeIdex.prototype.initializeEditor = function () {
                this.cEditor = CodeMirror.fromTextArea(document.getElementById("src-area"), {
                    lineNumbers: true,
                    matchBrackets: true,
                    mode: "text/x-csharp"
                });
            };
            return HomeIdex;
        })();
        Controllers.HomeIdex = HomeIdex;
    })(App.Controllers || (App.Controllers = {}));
    var Controllers = App.Controllers;
})(App || (App = {}));

$(function () {
    var homeIndexController = new App.Controllers.HomeIdex();
});
//# sourceMappingURL=HomeIndex.js.map
