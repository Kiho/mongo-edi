using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using AvalonDock.Layout;
using MongoData;

namespace Edi.ViewModel
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Globalization;
  using System.Linq;
  using System.Reflection;
  using System.Windows.Input;
  using System.Windows.Threading;
  using AvalonDock.Layout.Serialization;
  using Edi.ViewModel.Base;
  using Microsoft.Win32;
  using MsgBox;
  using SimpleControls.MRU.ViewModel;

  partial class Workspace : ViewModelBase
  {
    #region fields
    public const string LayoutFileName = "Layout.config";

    protected static Workspace mThis = new Workspace();

    private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    private bool? mDialogCloseResult;
    private bool mShutDownInProgress;
    private bool mShutDownInProgress_Cancel;

    private ObservableCollection<EdiViewModel> mFiles = new ObservableCollection<EdiViewModel>();
    private ReadOnlyObservableCollection<EdiViewModel> mReadonyFiles = null;
    private ToolViewModel[] mTools = null;
    private FileStatsViewModel mFileStats = null;
    private RecentFilesViewModel mRecentFiles = null;

    private EdiViewModel mActiveDocument = null;

    #endregion fields

    #region constructor
    /// <summary>
    /// Constructor
    /// </summary>
    protected Workspace()
    {
      this.mDialogCloseResult = null;
      this.mShutDownInProgress = mShutDownInProgress_Cancel = false;
    }
    #endregion constructor

    #region Properties
    #region ActiveDocument
    public EdiViewModel ActiveDocument
    {
      get
      {
        return mActiveDocument;
      }

      set
      {
        if (mActiveDocument != value)
        {
          mActiveDocument = value;

          this.NotifyPropertyChanged(() => this.ActiveDocument);

          // Ensure that no pending calls are in the dispatcher queue
          // This makes sure that we are blocked until bindings are re-established
          // (Bindings are, for example, required to scroll a selection into view)
          Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.SystemIdle, (Action)delegate
          {
            if (ActiveDocumentChanged != null)
            {
              ActiveDocumentChanged(this, EventArgs.Empty);

              if (value != null && this.mShutDownInProgress == false)
              {
                if (value.IsFilePathReal == true)
                  this.Config.LastActiveFile = value.FilePath;
              }
            }
          });
        }
      }
    }

    public event EventHandler ActiveDocumentChanged;

    #endregion


    /// <summary>
    /// Global static <seealso cref="Workspace" property to make this app root global accessible/>
    /// </summary>
    public static Workspace This
    {
      get { return Workspace.mThis; }
    }

    public ReadOnlyObservableCollection<EdiViewModel> Files
    {
      get
      {
        if (mReadonyFiles == null)
          mReadonyFiles = new ReadOnlyObservableCollection<EdiViewModel>(mFiles);

        return mReadonyFiles;
      }
    }

    public IEnumerable<ToolViewModel> Tools
    {
      get
      {
        if (mTools == null)
          mTools = new ToolViewModel[] { RecentFiles, FileStats };

        return mTools;
      }
    }

    public FileStatsViewModel FileStats
    {
      get
      {
        if (mFileStats == null)
          mFileStats = new FileStatsViewModel();

        return mFileStats;
      }
    }

    /// <summary>
    /// This property manages the data visible in the Recent Files ViewModel.
    /// </summary>
    public RecentFilesViewModel RecentFiles
    {
      get
      {
        if (mRecentFiles == null)
          mRecentFiles = new RecentFilesViewModel();

        return mRecentFiles;
      }
    }

    public bool ShutDownInProgress_Cancel
    {
      get
      {
        return this.mShutDownInProgress_Cancel;
      }

      set
      {
        if (this.mShutDownInProgress_Cancel != value)
        {
          this.mShutDownInProgress_Cancel = value;
          this.NotifyPropertyChanged(() => this.mShutDownInProgress_Cancel);
        }
      }
    }

    #region ApplicationName
    public string ApplicationTitle
    {
      get
      {
        return Assembly.GetEntryAssembly().GetName().Name;
      }
    }
    #endregion ApplicationName
    #endregion Properties

    #region methods
    /// <summary>
    /// Open a file supplied in <paramref name="filePath"/> (without displaying a file open dialog).
    /// </summary>
    /// <param name="filePath">file to open</param>
    /// <param name="AddIntoMRU">indicate whether file is to be added into MRU or not</param>
    /// <returns></returns>
    public EdiViewModel Open(string filePath, bool AddIntoMRU = true)
    {
      // Verify whether file is already open in editor, and if so, show it
      var fileViewModel = mFiles.FirstOrDefault(fm => fm.FilePath == filePath);

      if (fileViewModel != null)
      {
        this.ActiveDocument = fileViewModel; // File is already open so shiw it

        return fileViewModel;
      }

      // try to load the file from file system
      fileViewModel = EdiViewModel.LoadFile(filePath);

      if (fileViewModel == null)
      {
        if (this.Config.MruList.FindMRUEntry(filePath) != null)
        {
          if (Edi.Msg.Box.Show(string.Format(CultureInfo.CurrentCulture,
                                 "The file:\n\n'{0}'\n\ndoes not exist or cannot be loaded.\n\nDo you want to remove this file from the list of recent files?", filePath),
                                 "Error Loading file", MsgBoxButtons.YesNo) == MsgBoxResult.Yes)
          {
            this.Config.MruList.RemoveEntry(filePath);
          }
        }

        return null;
      }

      mFiles.Add(fileViewModel);

      // reset viewmodel options in accordance to current program settings
      SetActiveDocumentOnNewFileOrOpenFile(ref fileViewModel);

      if (AddIntoMRU == true)
        this.RecentFiles.AddNewEntryIntoMRU(filePath);

      LoadOutputWindow(fileViewModel.FileName);
      return fileViewModel;
    }

    #region NewCommand
    private void OnNew()
    {
      try
      {
        var vm = new EdiViewModel();

        this.mFiles.Add(vm);
        SetActiveDocumentOnNewFileOrOpenFile(ref vm);
        LoadOutputWindow();
      }
      catch (Exception exp)
      {
        logger.ErrorException(exp.Message, exp);
        Edi.Msg.Box.Show(exp, "Unhandled Error", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                         App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
      }
    }
    #endregion

    #region OpenCommand
    private void OnOpen()
    {
      try
      {
        var dlg = new OpenFileDialog();
        dlg.InitialDirectory = this.GetDefaultPath();

        if (dlg.ShowDialog().GetValueOrDefault())
        {
          var fileViewModel = this.Open(dlg.FileName);
        }
      }
      catch (Exception exp)
      {
        logger.ErrorException(exp.Message, exp);
        Edi.Msg.Box.Show(exp, App.IssueTrackerTitle, MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                         App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
      }
    }
    #endregion OnOpen

    #region Application_Exit_Command
    private void AppExit_CommandExecuted()
    {
      try
      {
        if (this.Closing_CanExecute() == true)
        {
          this.mShutDownInProgress_Cancel = false;
          this.OnRequestClose();
        }
      }
      catch (Exception exp)
      {
        logger.ErrorException(exp.Message, exp);
        Edi.Msg.Box.Show(exp, "Unhandled Error", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                         App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
      }
    }
    #endregion Application_Exit_Command

    #region Recent File List Pin Unpin Commands
    private void PinCommand_Executed(object o, ExecutedRoutedEventArgs e)
    {
      try
      {
        MRUEntryVM cmdParam = o as MRUEntryVM;

        if (cmdParam == null)
          return;

        if (e != null)
          e.Handled = true;

        this.RecentFiles.MruList.PinUnpinEntry(!cmdParam.IsPinned, cmdParam);
      }
      catch (Exception exp)
      {
        logger.ErrorException(exp.Message, exp);
        Edi.Msg.Box.Show(exp, "Unhandled Error", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                         App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
      }
    }

    private void AddMRUEntry_Executed(object o, ExecutedRoutedEventArgs e)
    {
      try
      {
        MRUEntryVM cmdParam = o as MRUEntryVM;

        if (cmdParam == null)
          return;

        if (e != null)
          e.Handled = true;

        this.RecentFiles.MruList.AddMRUEntry(cmdParam);
      }
      catch (Exception exp)
      {
        logger.ErrorException(exp.Message, exp);
        Edi.Msg.Box.Show(exp, "Unhandled Error", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                         App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
      }
    }

    private void RemoveMRUEntry_Executed(object o, ExecutedRoutedEventArgs e)
    {
      try
      {
        MRUEntryVM cmdParam = o as MRUEntryVM;

        if (cmdParam == null)
          return;

        if (e != null)
          e.Handled = true;

        this.RecentFiles.MruList.RemovePinEntry(cmdParam);
      }
      catch (Exception exp)
      {
        logger.ErrorException(exp.Message, exp);
        Edi.Msg.Box.Show(exp, "Unhandled Error", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                         App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
      }
    }
    #endregion Recent File List Pin Unpin Commands

    #region RequestClose [event]

    /// <summary>
    /// Raised when this workspace should be removed from the UI.
    /// </summary>
    public event EventHandler RequestClose;

    /// <summary>
    /// Method to be executed when user (or program) tries to close the application
    /// </summary>
    public void OnRequestClose()
    {
      try
      {
        if (this.mShutDownInProgress == false)
        {
          if (this.DialogCloseResult == null)
            this.DialogCloseResult = true;      // Execute Close event via attached property

          if (this.mShutDownInProgress_Cancel == true)
          {
            this.mShutDownInProgress = false;
            this.mShutDownInProgress_Cancel = false;
            this.DialogCloseResult = null;
          }
          else
          {
            this.mShutDownInProgress = true;

            CommandManager.InvalidateRequerySuggested();

            EventHandler handler = this.RequestClose;

            if (handler != null)
              handler(this, EventArgs.Empty);
          }
        }
      }
      catch (Exception exp)
      {
        this.mShutDownInProgress = false;

        logger.ErrorException(exp.Message, exp);
        Edi.Msg.Box.Show(exp, "Unhandled Error", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                         App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
      }
    }
    #endregion // RequestClose [event]

    /// <summary>
    /// Reset file view options in accordance with current program settings
    /// whenever a new file is internally created (on File Open or New File)
    /// </summary>
    /// <param name="vm"></param>
    private void SetActiveDocumentOnNewFileOrOpenFile(ref EdiViewModel vm)
    {
      try
      {
        // Set scale factor in default size of text font
        vm.FontSize.SelectedFontSizeCompute(this.Config.DocumentZoomView);

        ActiveDocument = vm;
      }
      catch (Exception exp)
      {
        logger.ErrorException(exp.Message, exp);
        Edi.Msg.Box.Show(exp, "Unhandled Error", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                         App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
      }
    }

    /// <summary>
    /// Implement part of requirement § 3.1.0 
    /// 
    /// The Open/SaveAs file dialog opens in the location of the currently active document (if any).
    /// 
    /// Otherwise, if there is no active document or the active document has never been saved before,
    /// the location of the last file open or file save/save as (which ever was last)
    /// is displayed in the Open/SaveAs File dialog.
    /// 
    /// The Open/SaveAs file dialog opens in the MyDocuments Windows user folder
    /// if none of the above conditions are true. (eg.: Open file for the very first
    /// time or last location does not exist).
    /// 
    /// The Open/Save/SaveAs file dialog opens in "C:\" if none of the above requirements
    /// can be implemented (eg.: MyDocuments folder does not exist or user has no access).
    /// 
    /// The last Open/Save/SaveAs file location used is stored and recovered between user sessions.
    /// </summary>
    /// <returns></returns>
    private string GetDefaultPath()
    {
      string sPath = string.Empty;

      try
      {
        // Generate a default path from cuurently or last active document
        if (this.ActiveDocument != null)
          sPath = this.ActiveDocument.GetFilePath();

        if (sPath == string.Empty)
          sPath = this.Config.GetLastActivePath();

        if (sPath == string.Empty)
          sPath = App.MyDocumentsUserDir;
        else
        {
          try
          {
            if (System.IO.Directory.Exists(sPath) == false)
              sPath = App.MyDocumentsUserDir;
          }
          catch
          {
            sPath = App.MyDocumentsUserDir;
          }
        }
      }
      catch (Exception exp)
      {
        logger.ErrorException(exp.Message, exp);
        Edi.Msg.Box.Show(exp, "Unhandled Error", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                         App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
      }

      return sPath;
    }

    internal bool OnSave(EdiViewModel fileToSave, bool saveAsFlag = false)
    {
      string filePath = (fileToSave == null ? string.Empty : fileToSave.FilePath);

      // Offer SaveAs file dialog if file has never been saved before (was created with new command)
      if (fileToSave != null)
        saveAsFlag = saveAsFlag | !fileToSave.IsFilePathReal;

      try
      {
        if (filePath == string.Empty || saveAsFlag == true)   // Execute SaveAs function
        {
          var dlg = new SaveFileDialog();

          try
          {
            dlg.FileName = System.IO.Path.GetFileName(filePath);
          }
          catch
          {
          }

          dlg.InitialDirectory = this.GetDefaultPath();

          if (dlg.ShowDialog().GetValueOrDefault() == true)     // SaveAs file if user OK'ed it so
          {
            filePath = dlg.FileName;

            fileToSave.SaveFile(filePath);
          }
          else
            return false;
        }
        else                                                  // Execute Save function
          fileToSave.SaveFile(filePath);

        this.RecentFiles.AddNewEntryIntoMRU(filePath);

        return true;
      }
      catch (Exception Exp)
      {
        string sMsg = "An unexpected problem has occurred while saving the file";

        if (filePath.Length > 0)
          sMsg = string.Format(CultureInfo.CurrentCulture, "'{0}'\n" +
                              "has occurred while saving the file\n:'{1}'", Exp.Message, filePath);
        else
          sMsg = string.Format(CultureInfo.CurrentCulture, "'{0}'\n" +
                              "has occurred while saving a file", Exp.Message);

        Edi.Msg.Box.Show(sMsg, "An unexpected problem has occurred while saving the file", MsgBoxButtons.OK);
      }

      return false;
    }

    internal bool OnCloseSaveDirtyFile(EdiViewModel fileToClose)
    {
      if (fileToClose.IsDirty && fileToClose.FileName != outFileName)
      {
        var res = Edi.Msg.Box.Show(string.Format(CultureInfo.CurrentCulture, "Save changes for file '{0}'?",
                                    fileToClose.FileName), this.ApplicationTitle,
                                    MsgBoxButtons.YesNoCancel,
                                    MsgBoxImage.Question,
                                    MsgBoxResult.Yes);

        if (res == MsgBoxResult.Cancel)
          return false;

        if (res == MsgBoxResult.Yes)
        {
          return OnSave(fileToClose);
        }
      }

      return true;
    }

    internal bool Close(EdiViewModel fileToClose)
    {
      try
      {
        if (this.OnCloseSaveDirtyFile(fileToClose) == false)
          return false;

        mFiles.Remove(fileToClose);

        if (this.mFiles.Count == 0)
          this.ActiveDocument = null;
        else
          this.ActiveDocument = this.mFiles[0];
      }
      catch (Exception exp)
      {
        logger.ErrorException(exp.Message, exp);
        Edi.Msg.Box.Show(exp, "Unhandled Error", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                         App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
      }

      return true;
    }

    /// <summary>
    /// This can be used to close the attached view via ViewModel
    /// 
    /// Source: http://stackoverflow.com/questions/501886/wpf-mvvm-newbie-how-should-the-viewmodel-close-the-form
    /// </summary>
    public bool? DialogCloseResult
    {
      get
      {
        return this.mDialogCloseResult;
      }

      private set
      {
        if (this.mDialogCloseResult != value)
        {
          this.mDialogCloseResult = value;
          this.NotifyPropertyChanged(() => this.DialogCloseResult);
        }
      }
    }

    /// <summary>
    /// Check if pre-requisites for closing application are available.
    /// Save session data on closing and cancel closing process if necessary.
    /// </summary>
    /// <returns>true if application is OK to proceed closing with closed, otherwise false.</returns>
    internal bool Exit_CheckConditions(object sender)
    {
      if (this.mShutDownInProgress == true)
        return true;

      try
      {
        try
        {
          App.CreateAppDataFolder();
          this.SerializeLayout(sender);            // Store the current layout for later retrieval
        }
        catch
        {
        }

        if (this.mFiles != null)               // Close all open files and make sure there are no unsaved edits
        {                                     // If there are any: Ask user if edits should be saved
          for (int i = 0; i < this.mFiles.Count; i++)
          {
            EdiViewModel f = this.mFiles[i];

            if (this.OnCloseSaveDirtyFile(f) == false)
            {
              this.mShutDownInProgress = false;
              return false;               // Cancel shutdown process (return false) if user cancels saving edits
            }
          }
        }
      }
      catch (Exception exp)
      {
        logger.ErrorException(exp.Message, exp);
        Edi.Msg.Box.Show(exp, "Unhandled Error", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                         App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
      }

      return true;
    }

    internal void SerializeLayout(object sender)
    {
      XmlLayoutSerializer xmlLayout = null;
      MainWindow mainWin = null;

      if (sender != null)
        mainWin = sender as MainWindow;

      // Create XML Layout, close documents, and save layout if closing went OK
      if (mainWin != null)
      {
        xmlLayout = new XmlLayoutSerializer(mainWin.dockManager);

        xmlLayout.Serialize(System.IO.Path.Combine(App.DirAppData, LayoutFileName));
      }
    }

    /// <summary>
    /// Set the active document to the file in <seealso cref="fileNamePath"/>
    /// if this is currently open.
    /// </summary>
    /// <param name="fileNamePath"></param>
    internal bool SetActiveDocument(string fileNamePath)
    {
      try
      {
        if (this.Files.Count >= 0)
        {
          EdiViewModel fi = this.Files.SingleOrDefault(f => f.FilePath == fileNamePath);

          if (fi != null && !IsOutputFile(fi.FileName))
          {
            this.ActiveDocument = fi;
            return true;
          }
        }
      }
      catch (Exception exp)
      {
        logger.ErrorException(exp.Message, exp);
        Edi.Msg.Box.Show(exp, "Unhandled Error", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                         App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
      }

      return false;
    }
    #endregion methods


  }
}
