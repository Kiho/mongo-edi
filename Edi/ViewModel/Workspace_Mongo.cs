using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using AvalonDock.Layout;
using MongoData;

namespace Edi.ViewModel
{
    partial class Workspace
    {
        const string outFileName = "Output.js";

        private bool IsOutputFile(string fileName)
        {
            return fileName.ToLower() != outFileName.ToLower();
        }

        internal bool OnRunScript(EdiViewModel fileToRun, bool saveAsFlag = true)
        {
            var target = new MongoTarget();
            var start = new ProcessStartInfo();

            var vm = ShowOutput();
            vm.Document.Text = "";
            if (vm.Document.TextEditor == null)
            {
                MessageBox.Show("Output window was not open, can't redirect output.\nPlease try again.", "Status",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            if (!Directory.Exists(target.Directory))
            {
                MessageBox.Show("Mongo bin directory was not found.\nPlease configure path in app.config.", "Status",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            start.FileName = target.Executable;
            start.Arguments = target.Arguments + "\"" + fileToRun.FilePath + "\"";
            start.WorkingDirectory = target.Directory;
            start.UseShellExecute = false;
            start.CreateNoWindow = target.CreateNoWindow;
            start.RedirectStandardOutput = true;

            using (var process = Process.Start(start))
            {
                if (process != null)
                    Redirect(process.StandardOutput, vm);
            }
            return true;
        }

        internal EdiViewModel ShowOutput()
        {
            var vm = this.mFiles.FirstOrDefault(x => IsOutputFile(x.FileName));
            if (vm == null)
            {
                LoadOutputWindow();
            }
            return vm;
        }

        internal EdiViewModel LoadOutputWindow(string fileName = "")
        {
            // Don't need to load output window when output.js is opening
            if (IsOutputFile(fileName))
            {                
                return null;
            }
            var firstPanel = FindFirstLayoutDocumentPane(Win.dockManager.Layout.RootPanel);
            var paneGroup = firstPanel.Parent as LayoutDocumentPaneGroup;
            if (paneGroup == null)
            {
                return null;
            }
            var vm = this.mFiles.FirstOrDefault(x => IsOutputFile(x.FileName));
            if (vm == null)
            {                
                string outFilePath = GetOutFilePath();
                vm = EdiViewModel.LoadFile(outFilePath);
                this.mFiles.Add(vm);
                var outputContent = firstPanel.Children.Last();
                // var layoutContent = firstPanel.Children.First();
                // Console.WriteLine(outputContent.ContentId);
                var layOutPanel = paneGroup.Parent as LayoutPanel;
                if (layOutPanel != null)
                {
                    var newPane = new LayoutDocumentPane(outputContent);
                    paneGroup.InsertChildAt(1, newPane);
                }
            }
            return vm;
        }

        internal string GetOutFilePath()
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            exePath = exePath.Substring(0, exePath.LastIndexOf(@"\", StringComparison.Ordinal));
            string outFilePath = Path.Combine(exePath, outFileName);
            if (!File.Exists(outFilePath))
            {
                using (var file = new StreamWriter(outFilePath, true))
                {
                    file.WriteLine("");
                }
            }
            return outFilePath;
        }

        #region FindFirstLayoutDocumentPane
        public LayoutDocumentPane FindFirstLayoutDocumentPane(LayoutDocumentPaneGroup layoutDocumentPaneGroup)
        {
            foreach (ILayoutDocumentPane layoutDocumentPaneInterface in layoutDocumentPaneGroup.Children)
            {
                if (layoutDocumentPaneInterface is LayoutDocumentPane)
                {
                    return layoutDocumentPaneInterface as LayoutDocumentPane;
                }

                if (layoutDocumentPaneInterface is LayoutDocumentPaneGroup)
                {
                    var layoutDocumentPane = FindFirstLayoutDocumentPane(layoutDocumentPaneInterface as LayoutDocumentPaneGroup);

                    if (layoutDocumentPane != null)
                    {
                        return layoutDocumentPane;
                    }
                }
            }

            return null;
        }

        public LayoutDocumentPane FindFirstLayoutDocumentPane(LayoutPanel layoutPanel)
        {
            foreach (ILayoutPanelElement layoutPanelElementInterace in layoutPanel.Children)
            {
                if (layoutPanelElementInterace is LayoutPanel)
                {
                    return FindFirstLayoutDocumentPane(layoutPanelElementInterace as LayoutPanel);
                }

                if (layoutPanelElementInterace is LayoutDocumentPane)
                {
                    return layoutPanelElementInterace as LayoutDocumentPane;
                }

                if (layoutPanelElementInterace is LayoutDocumentPaneGroup)
                {
                    var layoutDocumentPane = FindFirstLayoutDocumentPane(layoutPanelElementInterace as LayoutDocumentPaneGroup);

                    if (layoutDocumentPane != null)
                    {
                        return layoutDocumentPane;
                    }
                }
            }

            return null;
        }
        #endregion

        private void Redirect(StreamReader input, EdiViewModel output)
        {
            var sb = new StringBuilder();
            var bg = new Thread(a =>
            {
                var buffer = new char[1];
                while (input.Read(buffer, 0, 1) > 0)
                {
                    output.Document.TextEditor.Dispatcher.Invoke(new Action(delegate
                    {
                        if (buffer[0] != '\0')
                        {
                            sb.Append(new string(buffer));
                        }
                        if (buffer[0] == '\r' || buffer[0] == '\n' && sb.Length > 0)
                        {
                            output.Document.Insert(output.Document.Text.Length, sb.ToString());
                            sb.Clear();
                        }
                    }));
                }
            });
            bg.Start();
        }
    }
}
