namespace Edi.ViewModel
{
  using System;
  using System.Globalization;
  using System.IO;
  using System.Windows.Input;

  using Edi.Command;
  using Edi.ViewModel.TextBoxControl;
  using ICSharpCode.AvalonEdit.Document;
  using ICSharpCode.AvalonEdit.Highlighting;
  using MsgBox;

  class EdiViewModel : Edi.ViewModel.PaneViewModel, View.Dialogs.FindReplace.IEditor
  {
    public const string ToolContentId = "FileView";

    private IHighlightingDefinition mHighlightingDefinition;
    private static int iNewFileCounter = 1;
    private string defaultFileType = "txt";
    private string defaultFileName = "Untitled";
    private bool mIsFilePathReal = false;

    /// <summary>
    /// Standard constructor. See also static <seealso cref="LoadFile"/> method
    /// for construction from file saved on disk.
    /// </summary>
    public EdiViewModel()
    {
      this.TxtControl = new TextBoxController();
      
      this.FontSize = new ScaleViewModel(12);

      this.FilePath = string.Format(CultureInfo.InvariantCulture, "{0} {1}.{2}", this.defaultFileName,
                                    EdiViewModel.iNewFileCounter++,
                                    this.defaultFileType);

      this.IsDirty = false;
      this.mHighlightingDefinition = null;

      this.mDocument = new TextDocument();
        
      this.TextEditorSelectionStart = 0;
      this.TextEditorSelectionLength = 0;
    }

    internal bool IsFilePathReal
    {
      get{ return this.mIsFilePathReal; }
      set
      {
        this.mIsFilePathReal = value;
      }
    }

    #region FilePath
    private string mFilePath = null;

    /// <summary>
    /// Get/set complete path including file name to where this stored.
    /// This string is never null or empty.
    /// </summary>
    public string FilePath
    {
      get
      {
        if (this.mFilePath == null || this.mFilePath == String.Empty)
          return string.Format(CultureInfo.CurrentCulture, "New.{1}", this.defaultFileType);

        return this.mFilePath;
      }

      private set
      {
        if (this.mFilePath != value)
        {
          this.mFilePath = value;

          this.NotifyPropertyChanged(() => this.FilePath);
          this.NotifyPropertyChanged(() => this.FileName);
          this.NotifyPropertyChanged(() => this.Title);

          this.HighlightingDefinition = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(this.mFilePath));
        }
      }
    }
    #endregion

    #region Title
    /// <summary>
    /// Title is the string that is usually displayed - with or without dirty mark '*' - in the docking environment
    /// </summary>
    public override string Title
    {
      get
      {
        return this.FileName + (IsDirty == true ? "*" : string.Empty);
      }
    }
    #endregion

    #region FileName
    /// <summary>
    /// FileName is the string that is displayed whenever the application refers to this file, as in:
    /// string.Format(CultureInfo.CurrentCulture, "Would you like to save the '{0}' file", FileName)
    /// 
    /// Note the absense of the dirty mark '*'. Use the Title property if you want to display the file
    /// name with or without dirty mark when the user has edited content.
    /// </summary>
    public string FileName
    {
      get
      {
        // This option should never happen - its an emergency break for those cases that never occur
        if (FilePath == null || FilePath == String.Empty)
          return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", this.defaultFileName, this.defaultFileType);

        return System.IO.Path.GetFileName(FilePath);
      }
    }

    public override Uri IconSource
    {
      get
      {
        // This icon is visible in AvalonDock's Document Navigator window
        return new Uri("pack://application:,,,/Edi;component/Images/document.png", UriKind.RelativeOrAbsolute);
      }
    }
    #endregion FileName

    #region IsReadOnly
    private bool mIsReadOnly = false;
    public bool IsReadOnly
    {
      get
      {
        return this.mIsReadOnly;
      }

      protected set
      {
        if (this.mIsReadOnly != value)
        {
          this.mIsReadOnly = value;
          this.NotifyPropertyChanged(() => this.IsReadOnly);
        }
      }
    }

    private string mIsReadOnlyReason = string.Empty;
    public string IsReadOnlyReason
    {
      get
      {
        return this.mIsReadOnlyReason;
      }

      protected set
      {
        if (this.mIsReadOnlyReason != value)
        {
          this.mIsReadOnlyReason = value;
          this.NotifyPropertyChanged(() => this.IsReadOnlyReason);
        }
      }
    }
    #endregion IsReadOnly


    #region TextContent
    TextDocument mDocument;

    /// <summary>
    /// This property wraps the document class provided by AvalonEdit. The actual text is inside
    /// the document and can be accessed at save, load or other processing times.
    /// 
    /// The Text property itself cannot be bound in AvalonEdit since binding this would result
    /// in updating the text (via binding) each time a user enters a key on the keyboard
    /// (which would be a design error resulting in huge performance problems)
    /// </summary>
    public TextDocument Document
    {
      get { return mDocument; }
      set
      {
        if (mDocument != value)
        {
          mDocument = value;
          RaisePropertyChanged("Document");
        }
      }
    }
    #endregion

    #region IsDirty
    private bool _isDirty = false;

    /// <summary>
    /// IsDirty indicates whether the file currently loaded
    /// in the editor was modified by the user or not.
    /// </summary>
    public bool IsDirty
    {
      get { return _isDirty; }
      set
      {
        if (_isDirty != value)
        {
          Console.WriteLine("Setting IsDirty from " + _isDirty.ToString() + " to " + value.ToString());
          _isDirty = value;

          this.NotifyPropertyChanged(() => this.IsDirty);
          this.NotifyPropertyChanged(() => this.Title);
        }
      }
    }
    #endregion

    #region AvalonEdit properties
    /// <summary>
    /// AvalonEdit exposes a Highlighting property that controls whether keywords,
    /// comments and other interesting text parts are colored or highlighted in any
    /// other visual way. This property exposes the highlighting information for the
    /// text file managed in this viewmodel class.
    /// </summary>
    public IHighlightingDefinition HighlightingDefinition
    {
      get
      {
        return this.mHighlightingDefinition;
      }

      set
      {
        if(this.mHighlightingDefinition != value)
        {
          this.mHighlightingDefinition = value;
          this.NotifyPropertyChanged(() => this.HighlightingDefinition);
        }
      }
    }

    private bool mWordWrap = false;            // Toggle state command
    public bool WordWrap
    {
      get
      {
        return this.mWordWrap;
      }

      set
      {
        if (this.mWordWrap != value)
        {
          this.mWordWrap = value;
          this.NotifyPropertyChanged(() => this.WordWrap);
        }
      }
    }

    private bool mShowLineNumbers = true;     // Toggle state command
    public bool ShowLineNumbers
    {
      get
      {
        return this.mShowLineNumbers;
      }

      set
      {
        if (this.mShowLineNumbers != value)
        {
          this.mShowLineNumbers = value;
          this.NotifyPropertyChanged(() => this.ShowLineNumbers);
        }
      }
    }

    public bool ShowEndOfLine               // Toggle state command
    {
      get
      {
        return this.TextOptions.ShowEndOfLine;
      }

      set
      {
        if (this.TextOptions.ShowEndOfLine != value)
        {
          this.TextOptions.ShowEndOfLine = value;
          this.NotifyPropertyChanged(() => this.ShowEndOfLine);
        }
      }
    }

    public bool ShowSpaces               // Toggle state command
    {
      get
      {
        return this.TextOptions.ShowSpaces;
      }

      set
      {
        if (this.TextOptions.ShowSpaces != value)
        {
          this.TextOptions.ShowSpaces = value;
          this.NotifyPropertyChanged(() => this.ShowSpaces);
        }
      }
    }

    public bool ShowTabs               // Toggle state command
    {
      get
      {
        return this.TextOptions.ShowTabs;
      }

      set
      {
        if (this.TextOptions.ShowTabs != value)
        {
          this.TextOptions.ShowTabs = value;
          this.NotifyPropertyChanged(() => this.ShowTabs);
        }
      }
    }

    private ICSharpCode.AvalonEdit.TextEditorOptions mTextOptions = new ICSharpCode.AvalonEdit.TextEditorOptions();
    public ICSharpCode.AvalonEdit.TextEditorOptions TextOptions
    {
      get
      {
        return this.mTextOptions;
      }

      set
      {
        if (this.mTextOptions != value)
        {
          this.mTextOptions = value;
          this.NotifyPropertyChanged(() => this.TextOptions);
        }
      }
    }
    #endregion AvalonEdit properties

    #region LoadFile
    public static EdiViewModel LoadFile(string filePath)
    {
      bool IsFilePathReal = false;

      try 
	    {	        
        IsFilePathReal = File.Exists(filePath);
	    }
	    catch
	    {
	    }

      if (IsFilePathReal == false)
        return null;

      EdiViewModel vm = new EdiViewModel();

      if (vm.OpenFile(filePath) == true)
        return vm;

      return null;
    }

    /// <summary>
    /// Attempt to open a file and load it into the viewmodel if it exists.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns>True if file exists and was succesfully loaded. Otherwise false.</returns>
    protected bool OpenFile(string filePath)
    {
      try
      {
        if ((this.mIsFilePathReal = File.Exists(filePath)) == true)
        {
          this.FilePath = filePath;
          this.ContentId = mFilePath;
          IsDirty = false; // Mark document loaded from persistence as unedited copy (display without dirty mark '*' in name)

          // Check file attributes and set to read-only if file attributes indicate that
          if ((System.IO.File.GetAttributes(filePath) & FileAttributes.ReadOnly) != 0)
          {
            this.IsReadOnly = true;
            this.IsReadOnlyReason = "This file cannot be edit because you do not have write access.\n" +
                                    "Change the file access permissions or save the file in a different location if you want to edit it.";
          }

          try
          {
            this.mDocument = new TextDocument(System.IO.File.ReadAllText(mFilePath));
          }
          catch                 // File may be blocked by another process
          {                    // Try read-only shared method and set file access to read-only
            try
            {
              this.IsReadOnly = true;  // Open file in readonly mode
              this.IsReadOnlyReason = "This file cannot be edit because another process is currently writting to it.\n" +
                                      "Change the file access permissions or save the file in a different location if you want to edit it.";

              var fs = new FileStream(mFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
              using (var sr = new StreamReader(fs))
              {
                this.mDocument = new TextDocument(sr.ReadToEnd());
              }
            }
            catch (Exception ex)
            {
              Msg.Box.Show(ex.Message, "An error has occurred", MsgBoxButtons.OK);

              return false;
            }
          }
        }
        else
          return false;
      }
      catch (Exception exp)
      {
        Msg.Box.Show(exp.Message, "An error has occurred", MsgBoxButtons.OK);

        return false;
      }

      return true;
    }
    #endregion LoadFile

    #region SaveCommand
    /// <summary>
    /// Save the document viewed in this viewmodel.
    /// </summary>
    public bool CanSave()
    {
      return true;  // IsDirty
    }

    /// <summary>
    /// Write text content to disk and (re-)set associated properties
    /// </summary>
    /// <param name="filePath"></param>
    internal void SaveFile(string filePath)
    {
      File.WriteAllText(filePath, this.Document.Text);

      this.IsFilePathReal = true;
      this.FilePath = filePath;
      this.ContentId = filePath;
      this.IsDirty = false;
    }
    #endregion

    #region SaveAsCommand
    public bool CanSaveAs()
    {
      return true;  // IsDirty
    }

    public bool OnSaveAs()
    {
      return (Workspace.This.OnSave(this, true));
    }
    #endregion

    #region CloseCommand
    RelayCommand _closeCommand = null;

    /// <summary>
    /// This command cloases a single file. The binding for this is in the AvalonDock LayoutPanel Style.
    /// </summary>
    public ICommand CloseCommand
    {
      get
      {
        if (_closeCommand == null)
        {
          _closeCommand = new RelayCommand((p) => OnClose(), (p) => CanClose());
        }

        return _closeCommand;
      }
    }

    internal bool CanClose()
    {
      return true;
    }

    private void OnClose()
    {
      Workspace.This.Close(this);
    }
    #endregion

    #region OpenContainingFolder
    RelayCommand _openContainingFolderCommand = null;
    public ICommand OpenContainingFolderCommand
    {
      get
      {
        if (_openContainingFolderCommand == null)
          _openContainingFolderCommand = new RelayCommand((p) => this.OnOpenContainingFolderCommand(),
                                                          (p) => this.CanOpenContainingFolderCommand());

        return _openContainingFolderCommand;
      }
    }

    public bool CanOpenContainingFolderCommand()
    {
      return true;
    }

    private void OnOpenContainingFolderCommand()
    {
      try
      {
        if (System.IO.File.Exists(this.FilePath) == true)
        {
          // combine the arguments together it doesn't matter if there is a space after ','
          string argument = @"/select, " + this.FilePath;

          System.Diagnostics.Process.Start("explorer.exe", argument);
        }
        else
        {
          string parentDir = System.IO.Directory.GetParent(this.FilePath).FullName;

          if (System.IO.Directory.Exists(parentDir) == false)
            Msg.Box.Show(string.Format(CultureInfo.CurrentCulture, "The directory '{0}' does not exist or cannot be accessed.", parentDir),
                         "Error finding file", MsgBoxButtons.OK, MsgBoxImage.Error);
          else
          {
            string argument = @"/select, " + parentDir;

            System.Diagnostics.Process.Start("EXPLORER.EXE", argument);
          }
        }
      }
      catch (System.Exception ex)
      {
        Msg.Box.Show(string.Format(CultureInfo.CurrentCulture, "{0}\n'{1}'.", ex.Message, (this.FilePath == null ? string.Empty : this.FilePath)),
                     "Error finding file:", MsgBoxButtons.OK, MsgBoxImage.Error);
      }
    }
    #endregion OpenContainingFolder

    #region CopyFullPathtoClipboard
    RelayCommand _copyFullPathtoClipboard = null;
    public ICommand CopyFullPathtoClipboard
    {
      get
      {
        if (_copyFullPathtoClipboard == null)
          _copyFullPathtoClipboard = new RelayCommand((p) => this.OnCopyFullPathtoClipboardCommand(), (p) => this.CanCopyFullPathtoClipboardCommand());

        return _copyFullPathtoClipboard;
      }
    }

    public bool CanCopyFullPathtoClipboardCommand()
    {
      return true;
    }

    private void OnCopyFullPathtoClipboardCommand()
    {
      try
      {
        System.Windows.Clipboard.SetText(this.FilePath);
      }
      catch
      {
      }
    }
    #endregion CopyFullPathtoClipboard

    /// <summary>
    /// Get/Set viewmodel to display a customizable font size
    /// </summary>
    public ScaleViewModel FontSize { get; set; }

    #region CaretPosition
    // These properties are used to display the current column/line
    // of the cursor in the user interface
    private int mLine = 0;
    public int Line
    {
      get
      {
        return this.mLine;
      }

      set
      {
        if (this.mLine != value)
        {
          this.mLine = value;
          this.NotifyPropertyChanged(() => this.Line);
        }
      }
    }

    private int mColumn = 0;
    public int Column
    {
      get
      {
        return this.mColumn;
      }

      set
      {
        if (this.mColumn != value)
        {
          this.mColumn = value;
          this.NotifyPropertyChanged(() => this.Column);
        }
      }
    }
    #endregion CaretPosition

    #region EditorStateProperties
    // These properties are used to save and restore the editor state when CTRL+TABing between documents
    private int mTextEditorCaretOffset = 0;
    private int mTextEditorSelectionStart = 0;
    private int mTextEditorSelectionLength = 0;
    private bool mTextEditorIsRectangularSelection = false;
    private double mTextEditorScrollOffsetX = 0;
    private double mTextEditorScrollOffsetY = 0;

    /// <summary>
    /// Get/set editor carret position
    /// for CTRL-TAB Support http://avalondock.codeplex.com/workitem/15079
    /// </summary>
    public int TextEditorCaretOffset
    {
      get
      {
        return this.mTextEditorCaretOffset;
      }

      set
      {
        if (this.mTextEditorCaretOffset != value)
        {
          this.mTextEditorCaretOffset = value;
          this.NotifyPropertyChanged(() => this.TextEditorCaretOffset);
        }
      }
    }

    /// <summary>
    /// Get/set editor start of selection
    /// for CTRL-TAB Support http://avalondock.codeplex.com/workitem/15079
    /// </summary>
    public int TextEditorSelectionStart
    {
      get
      {
        return this.mTextEditorSelectionStart;
      }

      set
      {
        if (this.mTextEditorSelectionStart != value)
        {
          this.mTextEditorSelectionStart = value;
          this.NotifyPropertyChanged(() => this.TextEditorSelectionStart);
        }
      }
    }

    /// <summary>
    /// Get/set editor length of selection
    /// for CTRL-TAB Support http://avalondock.codeplex.com/workitem/15079
    /// </summary>
    public int TextEditorSelectionLength
    {
      get
      {
        return this.mTextEditorSelectionLength;
      }

      set
      {
        if (this.mTextEditorSelectionLength != value)
        {
          this.mTextEditorSelectionLength = value;
          this.NotifyPropertyChanged(() => this.TextEditorSelectionLength);
        }
      }
    }

    public bool TextEditorIsRectangularSelection
    {
      get
      {
        return this.mTextEditorIsRectangularSelection;
      }

      set
      {
        if (this.mTextEditorIsRectangularSelection != value)
        {
          this.mTextEditorIsRectangularSelection = value;
          this.NotifyPropertyChanged(() => this.TextEditorIsRectangularSelection);
        }
      }
    }

    #region EditorScrollOffsetXY
    /// <summary>
    /// Current editor view scroll X position
    /// </summary>
    public double TextEditorScrollOffsetX
    {
      get
      {
        return this.mTextEditorScrollOffsetX;
      }

      set
      {
        if (this.mTextEditorScrollOffsetX != value)
        {
          this.mTextEditorScrollOffsetX = value;
          this.NotifyPropertyChanged(() => this.TextEditorScrollOffsetX);
        }
      }
    }

    /// <summary>
    /// Current editor view scroll Y position
    /// </summary>
    public double TextEditorScrollOffsetY
    {
      get
      {
        return this.mTextEditorScrollOffsetY;
      }

      set
      {
        if (this.mTextEditorScrollOffsetY != value)
        {
          this.mTextEditorScrollOffsetY = value;
          this.NotifyPropertyChanged(() => this.TextEditorScrollOffsetY);
        }
      }
    }
    #endregion EditorScrollOffsetXY
    #endregion EditorStateProperties

    private TextBoxController mTxtControl = null;
    public TextBoxController TxtControl
    {
      get
      {
        return this.mTxtControl;
      }

      private set
      {
        if (this.mTxtControl != value)
        {
          this.mTxtControl = value;
          this.NotifyPropertyChanged(() => this.TxtControl);
        }
      }
    }

    #region IEditorInterface
    public string Text
    {
      get
      {
        if (this.Document == null)
          return string.Empty;

        return this.Document.Text;
      }
    }

    public int SelectionStart
    {
      get
      {
        int start = 0, length = 0;
        bool IsRectSelect = false;

        if (this.TxtControl != null)
          this.TxtControl.CurrentSelection(out start, out length, out IsRectSelect);

        return start;
      }
    }

    public int SelectionLength
    {
      get
      {
        int start=0, length=0;
        bool IsRectSelect = false;

        if (this.TxtControl != null)
          this.TxtControl.CurrentSelection(out start, out length, out IsRectSelect);

        return length;
      }
    }

    /// <summary>
    /// Selects the specified portion of Text and scrolls that part into view.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="length"></param>
    public void Select(int start, int length)
    {
      if (this.TxtControl != null)
        this.TxtControl.SelectText(start, length);
    }

    public void Replace(int start, int length, string ReplaceWith)
    {
      if (this.Document != null)
        this.Document.Replace(start, length, ReplaceWith);
    }

    /// <summary>
    /// This method is called before a replace all operation.
    /// </summary>
    public void BeginChange()
    {
      if (this.TxtControl != null)
        this.TxtControl.BeginChange();
    }

    /// <summary>
    /// This method is called after a replace all operation.
    /// </summary>
    public void EndChange()
    {
      if (this.TxtControl != null)
        this.TxtControl.EndChange();
    }
    #endregion IEditorInterface

    /// <summary>
    /// Get the path of the file or empty string if file does not exists on disk.
    /// </summary>
    /// <returns></returns>
    internal string GetFilePath()
    {
      try
      {
        if (System.IO.File.Exists(this.FilePath))
          return System.IO.Path.GetDirectoryName(this.FilePath);
      }
      catch
      {
      }

      return string.Empty;
    }
  }
}
