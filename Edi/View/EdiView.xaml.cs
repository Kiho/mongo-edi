namespace Edi.View
{
  using System;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Input;
  using System.Windows.Media;
  using System.Windows.Threading;

  using Edi.AvalonEdit;
  using Edi.AvalonEdit.Folding;
  using Edi.AvalonEdit.Intellisense;
  using ICSharpCode.AvalonEdit;
  using ICSharpCode.AvalonEdit.CodeCompletion;
  using ICSharpCode.AvalonEdit.Folding;
  using ICSharpCode.AvalonEdit.Highlighting;
  using ICSharpCode.AvalonEdit.Rendering;
  using ICSharpCode.AvalonEdit.Editing;

  /// <summary>
  /// Interaction logic for Document.xaml
  /// </summary>
  public partial class EdiView : UserControl
  {
    #region fields
    // Dependency property that can be set on this control and be forwarded to the texteditor control inside
    private static readonly DependencyProperty OptionsProperty =
      DependencyProperty.Register("Options",
                                  typeof(TextEditorOptions),
                                  typeof(EdiView),
                                  new FrameworkPropertyMetadata(new TextEditorOptions(),
                                  EdiView.OnOptionsChanged));

    // Dependency property that can be set on this control and be forwarded to the texteditor control inside
    private static readonly DependencyProperty SyntaxHighlightingProperty =
      DependencyProperty.Register("SyntaxHighlighting",
                                  typeof(IHighlightingDefinition),
                                  typeof(EdiView),
                                  new FrameworkPropertyMetadata(null,
                                  EdiView.OnSyntaxHighlightingChanged));

    // Dependency property that can be set on this control and be forwarded to the texteditor control inside
    public static readonly DependencyProperty WordWrapProperty = 
        DependencyProperty.Register("WordWrap", typeof(bool), typeof(EdiView), new UIPropertyMetadata(false));

    // Dependency property that can be set on this control and be forwarded to the texteditor control inside
    public static readonly DependencyProperty ShowLineNumbersProperty =
        DependencyProperty.Register("ShowLineNumbers",
                                    typeof(bool),
                                    typeof(EdiView),
                                    new UIPropertyMetadata(false));

    // Determine whether text can be edit or not
    private static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(EdiView), new UIPropertyMetadata(false));

    

    // Dependency property that can be set on this control and be forwarded to the texteditor control inside
    public static readonly DependencyProperty IsModifiedProperty =
        DependencyProperty.Register("IsModified", typeof(bool), typeof(EdiView), new UIPropertyMetadata(false));

    private DispatcherTimer mFoldingUpdateTimer = null;

    #region CaretPosition
    private static readonly DependencyProperty ColumnProperty =
        DependencyProperty.Register("Column", typeof(int), typeof(EdiView), new UIPropertyMetadata(1));


    public static readonly DependencyProperty LineProperty =
        DependencyProperty.Register("Line", typeof(int), typeof(EdiView), new UIPropertyMetadata(1));
    #endregion CaretPosition

    #region EditorStateProperties
    /// <summary>
    /// Editor selection start
    /// </summary>
    private static readonly DependencyProperty EditorSelectionStartProperty =
        DependencyProperty.Register("EditorSelectionStart", typeof(int), typeof(EdiView),
                                    new FrameworkPropertyMetadata(0,
                                                  FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>
    /// Editor selection length
    /// </summary>
    private static readonly DependencyProperty EditorSelectionLengthProperty =
        DependencyProperty.Register("EditorSelectionLength", typeof(int), typeof(EdiView),
                                    new FrameworkPropertyMetadata(0));

    /// <summary>
    /// Selected text (if any) in text editor
    /// </summary>
    private static readonly DependencyProperty EditorSelectedTextProperty =
        DependencyProperty.Register("EditorSelectedText", typeof(string), typeof(EdiView),
                                            new FrameworkPropertyMetadata(string.Empty));

    /// <summary>
    /// TextEditor caret position
    /// </summary>
    private static readonly DependencyProperty EditorCaretOffsetProperty =
        DependencyProperty.Register("EditorCaretOffset", typeof(int), typeof(EdiView),
                                    new FrameworkPropertyMetadata(0));

    /// <summary>
    /// Determine whether current selection is a rectangle or not
    /// </summary>
    private static readonly DependencyProperty EditorIsRectangleSelectionProperty =
        DependencyProperty.Register("EditorIsRectangleSelection", typeof(bool), typeof(EdiView), new UIPropertyMetadata(false));

    /// <summary>
    /// Style the background color of the current editor line
    /// </summary>
    public SolidColorBrush EditorCurrentLineBackground
    {
      get { return (SolidColorBrush)GetValue(EditorCurrentLineBackgroundProperty); }
      set { SetValue(EditorCurrentLineBackgroundProperty, value); }
    }

    // Style the background color of the current editor line
    private static readonly DependencyProperty EditorCurrentLineBackgroundProperty =
        DependencyProperty.Register("EditorCurrentLineBackground",
                                     typeof(SolidColorBrush),
                                     typeof(EdiView),
                                     new UIPropertyMetadata(null, EdiView.OnCurrentLineBackgroundChanged));

    #region EditorScrollOffsetXY
    /// <summary>
    /// Current editor view scroll X position
    /// </summary>
    public static readonly DependencyProperty EditorScrollOffsetXProperty =
        DependencyProperty.Register("EditorScrollOffsetX", typeof(double), typeof(EdiView), new UIPropertyMetadata(0.0));

    /// <summary>
    /// Current editor view scroll Y position
    /// </summary>
    public static readonly DependencyProperty EditorScrollOffsetYProperty =
        DependencyProperty.Register("EditorScrollOffsetY", typeof(double), typeof(EdiView), new UIPropertyMetadata(0.0));
    #endregion EditorScrollOffsetXY
    #endregion EditorStateProperties
    #endregion fields

    #region constructor
    public EdiView()
    {
      this.InitializeComponent();

      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Unloaded += new RoutedEventHandler(this.OnUnloaded);

      // Highlight current line in editor (even if editor is not focused) via themable dp-property
      this.AdjustCurrentLineBackground(this.EditorCurrentLineBackground);

      // Update highlighting of current line when caret position is changed
      this.textEditor.TextArea.Caret.PositionChanged += (sender, e) =>
      {
        this.textEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);

        if (this.textEditor.TextArea != null)
        {
          this.Column = this.textEditor.TextArea.Caret.Column;
          this.Line = this.textEditor.TextArea.Caret.Line;
        }
        else
          this.Column = this.Line = 0;
      };
   }
    #endregion constructor

    #region properties
    /// <summary>
    /// Dependency property that can be set on this control and be forwarded to the texteditor control inside
    /// </summary>
    public TextEditorOptions Options
    {
      get { return (TextEditorOptions)GetValue(EdiView.OptionsProperty); }
      set { SetValue(EdiView.OptionsProperty, value); }
    }

    /// <summary>
    /// Dependency property that can be set on this control and be forwarded to the texteditor control inside
    /// </summary>
    public IHighlightingDefinition SyntaxHighlighting
    {
      get { return (IHighlightingDefinition)GetValue(EdiView.SyntaxHighlightingProperty); }
      set { SetValue(EdiView.SyntaxHighlightingProperty, value); }
    }

    /// <summary>
    /// Dependency property that can be set on this control and be forwarded to the texteditor control inside
    /// </summary>
    public bool WordWrap
    {
      get { return (bool)GetValue(WordWrapProperty); }
      set { SetValue(WordWrapProperty, value); }
    }

    /// <summary>
    /// Dependency property that can be set on this control and be forwarded to the texteditor control inside
    /// </summary>
    public bool ShowLineNumbers
    {
      get { return (bool)GetValue(ShowLineNumbersProperty); }
      set { SetValue(ShowLineNumbersProperty, value); }
    }

    /// <summary>
    /// Get/set property to determine whether text can be edit or not
    /// </summary>
    public bool IsReadOnly
    {
      get { return (bool)GetValue(IsReadOnlyProperty); }
      set { SetValue(IsReadOnlyProperty, value); }
    }

    /// <summary>
    /// Dependency property that can be set on this control and be forwarded to the texteditor control inside
    /// </summary>
    public bool IsModified
    {
      get { return (bool)GetValue(IsModifiedProperty); }
      set { SetValue(IsModifiedProperty, value); }
    }

    #region CaretPosition
    public int Column
    {
      get { return (int)GetValue(ColumnProperty); }
      set { SetValue(ColumnProperty, value); }
    }    

    public int Line
    {
      get { return (int)GetValue(LineProperty); }
      set { SetValue(LineProperty, value); }
    }
    #endregion CaretPosition

    #region EditorStateProperties
    /// <summary>
    /// Dependency property to allow ViewModel binding
    /// </summary>
    public int EditorSelectionStart
    {
      get { return (int)GetValue(EdiView.EditorSelectionStartProperty); }
      set { SetValue(EdiView.EditorSelectionStartProperty, value); }
    }

    /// <summary>
    /// Dependency property to allow ViewModel binding
    /// </summary>
    public int EditorSelectionLength
    {
      get { return (int)GetValue(EdiView.EditorSelectionLengthProperty); }
      set { SetValue(EdiView.EditorSelectionLengthProperty, value); }
    }

    /// <summary>
    /// Selected text (if any) in text editor
    /// </summary>
    public string EditorSelectedText
    {
      get { return (string)GetValue(EditorSelectedTextProperty); }
      set { SetValue(EditorSelectedTextProperty, value); }
    }

    /// <summary>
    /// Dependency property to allow ViewModel binding
    /// </summary>
    public int EditorCaretOffset
    {
      get { return (int)GetValue(EdiView.EditorCaretOffsetProperty); }
      set { SetValue(EdiView.EditorCaretOffsetProperty, value); }
    }

    public bool EditorIsRectangleSelection
    {
      get { return (bool)GetValue(EditorIsRectangleSelectionProperty); }
      set { SetValue(EditorIsRectangleSelectionProperty, value); }
    }

    #region EditorScrollOffsetXY
    public double EditorScrollOffsetX
    {
      get { return (double)GetValue(EditorScrollOffsetXProperty); }
      set { SetValue(EditorScrollOffsetXProperty, value); }
    }

    public double EditorScrollOffsetY
    {
      get { return (double)GetValue(EditorScrollOffsetYProperty); }
      set { SetValue(EditorScrollOffsetYProperty, value); }
    }
    #endregion EditorScrollOffsetXY
    #endregion EditorStateProperties
    #endregion properties

    #region methods
    private CompletionWindow _completionWindow;
    void TextEditorTextAreaTextEntered(object sender, TextCompositionEventArgs e)
    {
      ICompletionWindowResolver resolver = new CompletionWindowResolver(textEditor.Text, textEditor.CaretOffset, e.Text, textEditor);
      _completionWindow = resolver.Resolve();
    }

    void TextEditorTextAreaTextEntering(object sender, TextCompositionEventArgs e)
    {
      if (e.Text.Length > 0 && _completionWindow != null)
      {
        if (!char.IsLetterOrDigit(e.Text[0]))
        {
          _completionWindow.CompletionList.RequestInsertion(e);
        }
      }
    }

    /// <summary>
    /// Hock event handlers and restore editor states (if any) or defaults
    /// when the control is fully loaded.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="args"></param>
    private void OnLoaded(object obj, RoutedEventArgs args)
    {
      // Call folding once upon loading so that first run is executed right away
      this.foldingUpdateTimer_Tick(null, null);

      this.mFoldingUpdateTimer = new DispatcherTimer();
      mFoldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
      mFoldingUpdateTimer.Tick += foldingUpdateTimer_Tick;
      mFoldingUpdateTimer.Start();

      textEditor.TextArea.TextEntering += TextEditorTextAreaTextEntering;
      textEditor.TextArea.TextEntered += TextEditorTextAreaTextEntered;
      textEditor.Focus();
      textEditor.ForceCursor = true;

      // Restore cusor position for CTRL-TAB Support http://avalondock.codeplex.com/workitem/15079
      textEditor.ScrollToHorizontalOffset(this.EditorScrollOffsetX);
      textEditor.ScrollToVerticalOffset(this.EditorScrollOffsetY);

      if (this.EditorIsRectangleSelection == true)
      {
        textEditor.CaretOffset = this.EditorCaretOffset;

        textEditor.SelectionStart = this.EditorSelectionStart;
        textEditor.SelectionLength = this.EditorSelectionLength;

        // See OnMouseCaretBoxSelection in Editing\CaretNavigationCommandHandler.cs
        // Convert normal selection to rectangle selection
        textEditor.TextArea.Selection = new ICSharpCode.AvalonEdit.Editing.RectangleSelection(textEditor.TextArea,
                                                                                              textEditor.TextArea.Selection.StartPosition,
                                                                                              textEditor.TextArea.Selection.EndPosition);
      }
      else
      {
        textEditor.CaretOffset = this.EditorCaretOffset;

        // http://www.codeproject.com/Articles/42490/Using-AvalonEdit-WPF-Text-Editor?msg=4388281#xx4388281xx
        textEditor.Select(this.EditorSelectionStart, this.EditorSelectionLength);
      }
      textEditor.Document.TextEditor = textEditor;
    }

    /// <summary>
    /// Unhock event handlers and save editor states (to be recovered later)
    /// when the control is unloaded.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="args"></param>
    private void OnUnloaded(object obj, RoutedEventArgs args)
    {
      if (this.mFoldingUpdateTimer != null)
        this.mFoldingUpdateTimer = null;
 
      this.Loaded -= this.OnLoaded;
      this.Unloaded -= this.OnUnloaded;

      textEditor.TextArea.TextEntering -= TextEditorTextAreaTextEntering;
      textEditor.TextArea.TextEntered -= TextEditorTextAreaTextEntered;

      // Save cusor position for CTRL-TAB Support http://avalondock.codeplex.com/workitem/15079
      this.EditorCaretOffset = textEditor.CaretOffset;
      this.EditorSelectionStart = textEditor.SelectionStart;
      this.EditorSelectionLength = textEditor.SelectionLength;
      this.EditorIsRectangleSelection = textEditor.TextArea.Selection is RectangleSelection;

      // http://stackoverflow.com/questions/11863273/avalonedit-how-to-get-the-top-visible-line
      this.EditorScrollOffsetX = textEditor.TextArea.TextView.ScrollOffset.X;
      this.EditorScrollOffsetY = textEditor.TextArea.TextView.ScrollOffset.Y;
    } 

    /// <summary>
    /// Raises the <see cref="OptionChanged"/> event.
    /// </summary>
    private static void OnOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      EdiView view = d as EdiView;

      if (view != null && e != null)
      {
        TextEditorOptions newValue = e.NewValue as TextEditorOptions;

        if (newValue != null)
          view.Options = newValue;
      }
    }

    /// <summary>
    /// The dependency property for has changed.
    /// Chnage the <seealso cref="SolidColorBrush"/> to be used for highlighting the current editor line
    /// in the particular <seealso cref="EdiView"/> control.
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    private static void OnCurrentLineBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      EdiView view = d as EdiView;

      if (view != null && e != null)
      {
        SolidColorBrush newValue = e.NewValue as SolidColorBrush;

        if (newValue != null)
	      {
          view.AdjustCurrentLineBackground(newValue);
	      }
      }
    }

    #region Folding
    FoldingManager foldingManager;
    AbstractFoldingStrategy foldingStrategy;

    private static void OnSyntaxHighlightingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      EdiView view = d as EdiView;

      if (view == null || e == null)
        return;

      IHighlightingDefinition newValue = e.NewValue as IHighlightingDefinition;

      if (newValue != null)
      {
        view.SyntaxHighlighting = newValue;
        view.SetFolding(newValue);
      }
    }

    private bool _InstallFoldingManager = false;

    public void SetFolding(IHighlightingDefinition SyntaxHighlighting)
    {
      if (SyntaxHighlighting == null)
      {
        foldingStrategy = null;
      }
      else
      {
        switch (SyntaxHighlighting.Name)
        {
          case "XML":
            foldingStrategy = new XmlFoldingStrategy();
            textEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
            break;
          case "C#":
          case "C++":
          case "PHP":
          case "Java":
            textEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy(textEditor.Options);
            foldingStrategy = new BraceFoldingStrategy();
            break;
          default:
            textEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
            foldingStrategy = null;
            break;
        }
      }

      if (foldingStrategy != null)
      {
        if (textEditor.Document != null)
        {
          if (foldingManager == null)
            foldingManager = FoldingManager.Install(textEditor.TextArea);

          foldingStrategy.UpdateFoldings(foldingManager, textEditor.Document);
        }
        else
          _InstallFoldingManager = true;
      }
      else
      {
        if (foldingManager != null)
        {
          FoldingManager.Uninstall(foldingManager);
          foldingManager = null;
        }
      }
    }

    /// <summary>
    /// Update the folding in the text editor when requested per call on this method
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void foldingUpdateTimer_Tick(object sender, EventArgs e)
    {
      if(this.IsVisible == true)
      {
        if (_InstallFoldingManager == true)
        {
          if (textEditor.Document != null)
          {
            if (foldingManager == null)
            {
              foldingManager = FoldingManager.Install(textEditor.TextArea);

              _InstallFoldingManager = false;
            }
          }
          else
            return;
        }

        if (foldingStrategy != null)
        {
          foldingStrategy.UpdateFoldings(foldingManager, textEditor.Document);
        }
      }
    }
    #endregion

    /// <summary>
    /// Reset the <seealso cref="SolidColorBrush"/> to be used for highlighting the current editor line.
    /// </summary>
    /// <param name="newValue"></param>
    private void AdjustCurrentLineBackground(SolidColorBrush newValue)
    {
      if (newValue != null)
      {
        this.textEditor.TextArea.TextView.BackgroundRenderers.Clear();

        this.textEditor.TextArea.TextView.BackgroundRenderers.Add(
                new HighlightCurrentLineBackgroundRenderer(this.textEditor, newValue.Clone()));
      }
    }
    #endregion methods
  }
}
