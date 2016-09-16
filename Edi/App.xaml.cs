namespace Edi
{
  using System;
  using System.Reflection;
  using System.Windows;

  using AvalonDock.Layout.Serialization;
  using Edi.AvalonEdit.Highlighting;
  using Edi.ViewModel;
  using System.Windows.Threading;
  using Edi.Dialogs.Config;
  using System.Windows.Input;
  using MsgBox;
  using System.Globalization;

  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    #region fields
    private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    private Window mMainWin;
    private bool mOKToClose;

    public const string IssueTrackerTitle = "Unhandled Error";
    public const string IssueTrackerText = "Please click on the link to check if this is a known problem\n\n" +
                                           "Please report this problem with the copy button if it is not known, yet.\n" +
                                           "The problem report should contain the error message below (you can use the copy button)\n" +
                                           "and a statement about the function/workflow you were using. Attach screenshots or sample files if applicable.";
    public const string IssueTrackerLink = "https://edi.codeplex.com/workitem/list/basic";
    #endregion fields

    #region constructor
    public App()
    {
      HighlightingExtension.RegisterCustomHighlightingPatterns();
    }
    #endregion constructor

    #region properties
    /// <summary>
    /// Get a path to the directory where the application
    /// can persist/load user data on session exit and re-start.
    /// </summary>
    public static string DirAppData
    {
      get
      {
        return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                         System.IO.Path.DirectorySeparatorChar +
                                         App.Company;
      }
    }

    /// <summary>
    /// Get a path to the directory where the user store his documents
    /// </summary>
    public static string MyDocumentsUserDir
    {
      get
      {
        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
      }
    }

    /// <summary>
    /// Convinience property to get the name of the executing assembly (usually name of *.exe file)
    /// </summary>
    internal static string AssemblyTitle
    {
      get
      {
        return Assembly.GetEntryAssembly().GetName().Name;
      }
    }

    internal static string Company
    {
      get
      {
        return "Edi";
      }
    }

    /// <summary>
    /// Get path and file name to application specific session file
    /// </summary>
    internal static string DirFileAppSessionData
    {
      get
      {
        return System.IO.Path.Combine(App.DirAppData,
                                      string.Format(CultureInfo.InvariantCulture, "{0}.session", App.AssemblyTitle));
      }
    }
    #endregion properties

    #region methods
    /// <summary>
    /// Create a dedicated directory to store program settings and session data
    /// </summary>
    /// <returns></returns>
    static public bool CreateAppDataFolder()
    {
      try
      {
        if (System.IO.Directory.Exists(App.DirAppData) == false)
          System.IO.Directory.CreateDirectory(App.DirAppData);
      }
      catch (Exception exp)
      {
        logger.Error(exp);
        return false;
      }

      return true;
    }

    /// <summary>
    /// Check if end of application session should be canceled or not
    /// (we may have gotten here through unhandled exceptions - so we
    /// display it and attempt CONTINUE so user can save his data.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
    {
      base.OnSessionEnding(e);

      try
      {
        if (this.mMainWin != null && this.mOKToClose == true)
        {
          if (this.mMainWin.DataContext != null)
          {
            e.Cancel = true;

            logger.Error("The {0} application received an fatal error: {1}.\n\nPlease save your data and re-start manually.",
                          Application.ResourceAssembly.GetName(), e.ReasonSessionEnding.ToString());

            Msg.Box.Show(string.Format(CultureInfo.CurrentCulture, "The application received a request to shutdown.\n\n" +
                          "Please save your data and exit manually (click on the link below to report this problem if it persists)."), "Attempt to exit",
                          MsgBoxButtons.OK, MsgBoxImage.Warning,
                          MsgBoxResult.Ok,
                          App.IssueTrackerLink, App.IssueTrackerLink,
                          null, null, true);
          }
        }
      }
      catch (Exception exp)
      {
        logger.Error(exp);
      }
    }

    /// <summary>
    /// This is the first bit of code being executed when the application is invoked (main entry point).
    /// 
    /// Use the <paramref name="e"/> parameter to evaluate command line options.
    /// Invoking a program with an associated file type extension (eg.: *.txt) in Explorer
    /// results, for example, in executing this function with the path and filename being
    /// supplied in <paramref name="e"/>.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Application_Startup(object sender, StartupEventArgs e)
    {
      try
      {
        this.mOKToClose = true;
        this.mMainWin = new MainWindow();

        App.CreateAppDataFolder();

        if (this.mMainWin != null)
        {
          this.mMainWin.Closing += this.OnClosing;

          // When the ViewModel asks to be closed, 
          // close the window.
          // Source: http://msdn.microsoft.com/en-us/magazine/dd419663.aspx
          Workspace.This.RequestClose += delegate
          {
            // Save session data and close application
            this.OnClosed(this.mMainWin.DataContext as ViewModel.Workspace, this.mMainWin);

            this.mOKToClose = true;
          };

          this.LoadSession(Workspace.This, this.mMainWin);
          this.mMainWin.Show();
          this.mOKToClose = true;

          if (e != null)
          {
            if (e.Args.Length > 0)
            {
              for (int i = 0; i < e.Args.Length; i++)
              {
                string sPath = e.Args[i];

                Workspace.This.Open(sPath);
              }
            }
          }
        }
      }
      catch (Exception exp)
      {
        logger.Error(exp);
      }
    }

    private void LoadSession(Workspace workSpace, Window win)
    {
      try
      {
        win.DataContext = workSpace;

        // Establish command binding to accept user input via commanding framework
        workSpace.InitCommandBinding(win);

        workSpace.LoadConfigOnAppStartup();

        workSpace.Config.MainWindowPosSz.SetPos(win);      // (Re)-set mainWindow view coordinates

        string lastActiveFile = workSpace.Config.LastActiveFile;

        MainWindow mainWin = win as MainWindow;
        workSpace.Win = mainWin;

        string layoutFileName = System.IO.Path.Combine(App.DirAppData, "Layout.config");

        if (System.IO.File.Exists(layoutFileName) == true)
        {
          var layoutSerializer = new XmlLayoutSerializer(mainWin.dockManager);

          layoutSerializer.LayoutSerializationCallback += (s, e) =>
          {
            if (e.Model.ContentId == FileStatsViewModel.ToolContentId)
              e.Content = Workspace.This.FileStats;
            else
              if (e.Model.ContentId == RecentFilesViewModel.ToolContentId)
                e.Content = Workspace.This.RecentFiles;
              else
              {
                if (workSpace.Config.ReloadOpenFilesOnAppStart == true)
                {
                  if (!string.IsNullOrWhiteSpace(e.Model.ContentId) && System.IO.File.Exists(e.Model.ContentId))
                  {
                    e.Content = Workspace.This.Open(e.Model.ContentId);
                  }
                  // else
                  //  e.Cancel = true;
                }
              }
          };

          layoutSerializer.Deserialize(layoutFileName);

          workSpace.SetActiveDocument(lastActiveFile);
        }
      }
      catch (Exception exp)
      {
        logger.Error(exp);
      }
    }

    /// <summary>
    /// Save session data on closing
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      try
      {
        Workspace wsVM = Workspace.This;

        if (wsVM != null)
        {
          if (wsVM.Exit_CheckConditions(sender) == true)      // Close all open files and check whether application is ready to close
          {
            wsVM.OnRequestClose();                          // (other than exception and error handling)

            e.Cancel = false;
            //if (wsVM != null)
            //  wsVM.SaveConfigOnAppClosed(); // Save application layout
          }
          else
            e.Cancel = wsVM.ShutDownInProgress_Cancel = true;
        }
      }
      catch (Exception exp)
      {
        logger.Error(exp);
      }
    }

    /// <summary>
    /// Execute closing function and persist session data to be reloaded on next restart
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnClosed(ViewModel.Workspace appVM, Window win)
    {
      try
      {
        Console.WriteLine("  >>> Shuting down application.");

        // Persist window position, width and height from this session
        appVM.Config.MainWindowPosSz = new ViewPosSzViewModel(win);

        // Save/initialize program options that determine global programm behaviour
        appVM.SaveConfigOnAppClosed();
      }
      catch (Exception exp)
      {
        logger.Error(exp);
        Msg.Box.Show(exp.ToString(), "Error in shut-down process", MsgBoxButtons.OK, MsgBoxImage.Error);
      }
    }

    /// <summary>
    /// Handle unhandled exception here
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
      string message = string.Empty;

      try
      {
        if (e.Exception != null)
        {
          message = string.Format(CultureInfo.CurrentCulture, "{0}\n\n{1}", e.Exception.Message, e.Exception.ToString());
        }
        else
          message = "An unknown error occurred.";

        logger.Error(message);

        Msg.Box.Show(e.Exception, "Unhandled Error",
                      MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                      App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);

        e.Handled = true;
      }
      catch (Exception exp)
      {
        logger.Error("An error occured while dispatching unhandled exception", exp);
      }
    }
    #endregion methods
  }
}
