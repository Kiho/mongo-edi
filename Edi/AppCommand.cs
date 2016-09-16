namespace Edi
{
  using System.Windows.Input;

  public class AppCommand
  {
    #region CommandFramework Fields
    private static RoutedUICommand exit;
    private static RoutedUICommand loadFile;
    private static RoutedUICommand saveAll;
    private static RoutedUICommand pinUnpin;
    private static RoutedUICommand addMruEntry;
    private static RoutedUICommand removeMruEntry;
    private static RoutedUICommand closeFile;
    private static RoutedUICommand viewTheme;

    #region Text Edit Commands
/***
    private static RoutedUICommand copyItem;
    private static RoutedUICommand cutItem;
    private static RoutedUICommand pasteItem;
    private static RoutedUICommand deleteItem;
***/
    private static RoutedUICommand wordWrap;          // Toggle state command
    private static RoutedUICommand showLineNumbers;  // Toggle state command
    private static RoutedUICommand showEndOfLine;   // Toggle state command

    private static RoutedUICommand showTabs;
    private static RoutedUICommand showSpaces;

    private static RoutedUICommand gotoLine;
    private static RoutedUICommand findText;
    private static RoutedUICommand findNextText;
    private static RoutedUICommand findPreviousText;
    private static RoutedUICommand replaceText;

    private static RoutedUICommand selectAll;

    private static RoutedUICommand runScript;

    #endregion Text Edit Commands
    #endregion CommandFramework Fields

    #region Static Constructor (Constructs static application commands)
    /// <summary>
    /// Define custom commands and their key gestures
    /// </summary>
    static AppCommand()
    {
      InputGestureCollection inputs = null;

      // Initialize the exit command
      inputs = new InputGestureCollection();
      inputs.Add(new KeyGesture(Key.F4, ModifierKeys.Alt, "Alt+F4"));
      AppCommand.exit = new RoutedUICommand("Exit", "Exit", typeof(AppCommand), inputs);

      // Execute file open command (without user interaction)
      inputs = new InputGestureCollection();
      AppCommand.loadFile = new RoutedUICommand("Open ...", "LoadFile", typeof(AppCommand), inputs);

      inputs = new InputGestureCollection();
      AppCommand.saveAll = new RoutedUICommand("Save All ...", "SaveAll", typeof(AppCommand), inputs);

      // Initialize pin command (to set or unset a pin in MRU and re-sort list accordingly)
      inputs = new InputGestureCollection();
      AppCommand.pinUnpin = new RoutedUICommand("Pin or Unpin", "Pin", typeof(AppCommand), inputs);

      // Execute add recent files list etnry pin command (to add another MRU entry into the list)
      inputs = new InputGestureCollection();
      AppCommand.addMruEntry = new RoutedUICommand("Add Entry", "AddEntry", typeof(AppCommand), inputs);

      // Execute remove pin command (remove a pin from a recent files list entry)
      inputs = new InputGestureCollection();
      AppCommand.removeMruEntry = new RoutedUICommand("Remove Entry", "RemoveEntry", typeof(AppCommand), inputs);

      inputs = new InputGestureCollection();
      inputs.Add(new KeyGesture(Key.F4, ModifierKeys.Control, "Ctrl+F4"));
      inputs.Add(new KeyGesture(Key.W, ModifierKeys.Control, "Ctrl+W"));
      AppCommand.closeFile = new RoutedUICommand("Close", "Close", typeof(AppCommand), inputs);

      // Initialize the viewTheme command
      inputs = new InputGestureCollection();
      ////inputs.Add(new KeyGesture(Key.F4, ModifierKeys.Alt, "Alt+F4"));
      AppCommand.viewTheme = new RoutedUICommand("View Theme", "ViewTheme", typeof(AppCommand), inputs);

      #region Text Edit Commands
/***  inputs = new InputGestureCollection();
      inputs.Add(new KeyGesture(Key.C, ModifierKeys.Control, "Ctrl+C"));
      AppCommand.copyItem = new RoutedUICommand("Copy", "Copy", typeof(AppCommand), inputs);      

      inputs = new InputGestureCollection();
      inputs.Add(new KeyGesture(Key.X, ModifierKeys.Control, "Ctrl+X"));
      AppCommand.cutItem = new RoutedUICommand("Cut", "Cut", typeof(AppCommand), inputs);

      inputs = new InputGestureCollection();
      inputs.Add(new KeyGesture(Key.V, ModifierKeys.Control, "Ctrl+V"));
      AppCommand.pasteItem = new RoutedUICommand("Paste", "Paste", typeof(AppCommand), inputs);

      inputs = new InputGestureCollection();
      ////inputs.Add(new KeyGesture(Key.X, ModifierKeys.Control, "Ctrl+X"));
      AppCommand.deleteItem = new RoutedUICommand("Delete", "Delete", typeof(AppCommand), inputs);
***/
      inputs = new InputGestureCollection();                                      // Toggle state command
      ////inputs.Add(new KeyGesture(Key.X, ModifierKeys.Control, "Ctrl+X"));
      AppCommand.wordWrap = new RoutedUICommand("Switch word-wrap document display mode on/off", "WordWrap", typeof(AppCommand), inputs);

      inputs = new InputGestureCollection();                                      // Toggle state command
      ////inputs.Add(new KeyGesture(Key.X, ModifierKeys.Control, "Ctrl+X"));
      AppCommand.showLineNumbers = new RoutedUICommand("Show a line number for each line in a document", "ShowLineNumbers", typeof(AppCommand), inputs);

      inputs = new InputGestureCollection();                                     // Toggle state command
      ////inputs.Add(new KeyGesture(Key.X, ModifierKeys.Control, "Ctrl+X"));
      AppCommand.showEndOfLine = new RoutedUICommand("Highlight the end of each line in document", "ShowEndOfLine", typeof(AppCommand), inputs);

      inputs = new InputGestureCollection();                                     // Toggle state command
      AppCommand.showTabs = new RoutedUICommand("Highlight tabulator characters with a special character in a document", "ShowTabs", typeof(AppCommand), inputs);

      inputs = new InputGestureCollection();                                     // Toggle state command
      AppCommand.showSpaces = new RoutedUICommand("Highlight spaces with a special character in a document", "ShowSpaces", typeof(AppCommand), inputs);

      inputs = new InputGestureCollection();                                     // Goto Line n in the current document
      inputs.Add(new KeyGesture(Key.A, ModifierKeys.Control, "Ctrl + A"));
      AppCommand.selectAll = new RoutedUICommand("Select all items in a document", "SelectAll", typeof(AppCommand), inputs);

      inputs = new InputGestureCollection();                                     // Goto Line n in the current document
      inputs.Add(new KeyGesture(Key.G, ModifierKeys.Control, "Ctrl + G"));
      AppCommand.gotoLine = new RoutedUICommand("Go to a specific line in a document", "GotoLine", typeof(AppCommand), inputs);

      inputs = new InputGestureCollection();                                     // Goto Line n in the current document
      inputs.Add(new KeyGesture(Key.F, ModifierKeys.Control, "Ctrl + F"));
      AppCommand.findText = new RoutedUICommand("Find specific text in a document", "FindText", typeof(AppCommand), inputs);

      inputs = new InputGestureCollection();                                     // Goto Line n in the current document
      inputs.Add(new KeyGesture(Key.F3, ModifierKeys.None, "F3"));
      AppCommand.findNextText = new RoutedUICommand("Find next occurrance of specific text in a document", "FindNextText", typeof(AppCommand), inputs);

      inputs = new InputGestureCollection();                                     // Goto Line n in the current document
      inputs.Add(new KeyGesture(Key.F3, ModifierKeys.Shift, "Shift+F3"));
      AppCommand.findPreviousText = new RoutedUICommand("Find previous occurrance of specific text in a document", "FindNextText", typeof(AppCommand), inputs);

      inputs = new InputGestureCollection();                                     // Goto Line n in the current document
      inputs.Add(new KeyGesture(Key.H, ModifierKeys.Control, "Ctrl + H"));
      AppCommand.replaceText = new RoutedUICommand("Find and replace a specific text in a document", "FindReplaceText", typeof(AppCommand), inputs);
      #endregion Text Edit Commands

      inputs = new InputGestureCollection();  
      inputs.Add(new KeyGesture(Key.F5, ModifierKeys.None, "F5"));
      AppCommand.runScript = new RoutedUICommand("Run", "RunScript", typeof(AppCommand), inputs);
    }
    #endregion Static Constructor

    #region CommandFramwork Properties (Exposes Commands to which the UI can bind to)
    /// <summary>
    /// Static property of the correspondong <seealso cref="System.Windows.Input.RoutedUICommand"/>
    /// </summary>
    public static RoutedUICommand Exit
    {
      get { return AppCommand.exit; }
    }

    /// <summary>
    /// Execute file open command (without user interaction)
    /// </summary>
    public static RoutedUICommand LoadFile
    {
      get { return AppCommand.loadFile; }
    }

    /// <summary>
    /// Execute a command to save all edited files and current program settings
    /// </summary>
    public static RoutedUICommand SaveAll
    {
      get { return AppCommand.saveAll; }
    }

    /// <summary>
    /// Execute pin/unpin command (to set or unset a pin in MRU and re-sort list accordingly)
    /// </summary>
    public static RoutedUICommand PinUnpin
    {
      get { return AppCommand.pinUnpin; }
    }

    /// <summary>
    /// Execute add recent files list etnry pin command (to add another MRU entry into the list)
    /// </summary>
    public static RoutedUICommand AddMruEntry
    {
      get { return AppCommand.addMruEntry; }
    }

    /// <summary>
    /// Execute remove pin command (remove a pin from a recent files list entry)
    /// </summary>
    public static RoutedUICommand RemoveMruEntry
    {
      get { return AppCommand.removeMruEntry; }
    }

    public static RoutedUICommand CloseFile
    {
      get { return AppCommand.closeFile; }
    }

    /// <summary>
    /// Static property of the correspondong <seealso cref="System.Windows.Input.RoutedUICommand"/>
    /// </summary>
    public static RoutedUICommand ViewTheme
    {
      get { return AppCommand.viewTheme; }
    }

    public static RoutedUICommand RunScript
    {
        get { return AppCommand.runScript; }
    }

    #region Text Edit Commands
/***
    public static RoutedUICommand CopyItem
    {
      get { return AppCommand.copyItem; }
    }

    public static RoutedUICommand CutItem
    {
      get { return AppCommand.cutItem; }
    }

    public static RoutedUICommand PasteItem
    {
      get { return AppCommand.pasteItem; }
    }

    public static RoutedUICommand DeleteItem
    {
      get { return AppCommand.deleteItem; }
    }
***/
    public static RoutedUICommand WordWrap          // Toggle state command
    {
      get { return AppCommand.wordWrap; }
    }

    public static RoutedUICommand ShowLineNumbers   // Toggle state command
    {
      get { return AppCommand.showLineNumbers; }
    }

    public static RoutedUICommand ShowEndOfLine    // Toggle state command
    {
      get { return AppCommand.showEndOfLine; }
    }


    public static RoutedUICommand ShowTabs    // Toggle state command
    {
      get { return AppCommand.showTabs; }
    }

    public static RoutedUICommand ShowSpaces    // Toggle state command
    {
      get { return AppCommand.showSpaces; }
    }

    /// <summary>
    /// Goto line n in a given document
    /// </summary>
    public static RoutedUICommand GotoLine
    {
      get { return AppCommand.gotoLine; }
    }

    /// <summary>
    /// Find text in a given document
    /// </summary>
    public static RoutedUICommand FindText
    {
      get { return AppCommand.findText; }
    }

    public static RoutedUICommand FindNextText
    {
      get { return AppCommand.findNextText; }
    }

    public static RoutedUICommand FindPreviousText
    {
      get { return AppCommand.findPreviousText; }
    }

    /// <summary>
    /// Find and replace text in a given document
    /// </summary>
    public static RoutedUICommand ReplaceText
    {
      get { return AppCommand.replaceText; }
    }

    /// <summary>
    /// Select all text in a document
    /// </summary>
    public static RoutedUICommand SelectAll
    {
      get { return AppCommand.selectAll; }
    }
    #endregion Text Edit Commands
    #endregion CommandFramwork_Properties
  }
}
