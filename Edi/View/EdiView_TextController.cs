namespace Edi.View
{
  using System;
  using System.Collections.Generic;
  using System.Windows;
  using Edi.ViewModel.TextBoxControl;
  using ICSharpCode.AvalonEdit;
  using ICSharpCode.AvalonEdit.Document;

  public partial class EdiView
  {
    #region ITextBoxControllerFields
    private static readonly Dictionary<ITextBoxController, TextEditor> elements = new Dictionary<ITextBoxController, TextEditor>();

    private static readonly DependencyProperty TextBoxControllerProperty =
                            DependencyProperty.Register(
                              "TextBoxController",
                              typeof(ITextBoxController),
                              typeof(EdiView),
                              new FrameworkPropertyMetadata(null, OnTextBoxControllerChanged));
    #endregion ITextBoxControllerFields

    #region ITextBoxController_Properties
    public static void SetTextBoxController(UIElement element, ITextBoxController value)
    {
      element.SetValue(EdiView.TextBoxControllerProperty, value);
    }

    public static ITextBoxController GetTextBoxController(UIElement element)
    {
      return (ITextBoxController)element.GetValue(EdiView.TextBoxControllerProperty);
    }
    #endregion ITextBoxController_Properties

    #region ITextBoxController_methods
    /// <summary>
    /// Call corresponding on changed method when the depependency property
    /// for this <seealso cref="FileDocument"/> control is changed.
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    private static void OnTextBoxControllerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var fileDoc = d as EdiView;
      if (fileDoc == null)
        throw new ArgumentNullException("Object of type FileDocument is not available!");

      var txtBox = fileDoc.textEditor as TextEditor;
      if (txtBox == null)
        throw new ArgumentNullException("Object of type TextBox is not available!");

      // Remove event handler from old if OldValue is available
      var oldController = e.OldValue as ITextBoxController;
      if (oldController != null)
      {
        elements.Remove(oldController);
        oldController.SelectAll -= SelectAll;
        oldController.Select -= Select;
        oldController.ScrollToLineEvent -= ScrollToLine;
        oldController.CurrentSelectionEvent -= CurrentSelection;
        oldController.BeginChangeEvent -= EdiView.BeginChange;
        oldController.EndChangeEvent -= EdiView.EndChange;
        oldController.GetSelectedTextEvent -= EdiView.GetSelectedText;
      }

      // Add new eventhandler for each event declared in the interface declaration
      var newController = e.NewValue as ITextBoxController;
      if (newController != null)
      {
        // Sometime the newController is already there but the event handling is not working
        // Remove controller and event handling and install a new one instead.
        TextEditor test;
        if (elements.TryGetValue(newController, out test) == true)
        {
          elements.Remove(newController);

          newController.SelectAll -= EdiView.SelectAll;
          newController.Select -= EdiView.Select;
          newController.ScrollToLineEvent -= EdiView.ScrollToLine;
          newController.CurrentSelectionEvent -= EdiView.CurrentSelection;
          newController.BeginChangeEvent -= EdiView.BeginChange;
          newController.EndChangeEvent -= EdiView.EndChange;
          newController.GetSelectedTextEvent -= EdiView.GetSelectedText;
        }

        elements.Add(newController, txtBox);
        newController.SelectAll += SelectAll;
        newController.Select += Select;
        newController.ScrollToLineEvent += ScrollToLine;
        newController.CurrentSelectionEvent += CurrentSelection;
        newController.BeginChangeEvent += EdiView.BeginChange;
        newController.EndChangeEvent += EdiView.EndChange;
        newController.GetSelectedTextEvent += EdiView.GetSelectedText;
      }
    }

    /// <summary>
    /// Select all text in the editor
    /// </summary>
    /// <param name="sender"></param>
    private static void SelectAll(ITextBoxController sender)
    {
      TextEditor element;
      if (!elements.TryGetValue(sender, out element))
        throw new ArgumentException("sender");

      element.Focus();
      element.SelectAll();
    }

    /// <summary>
    /// Select the text in the editor as indicated by <paramref name="start"/>
    /// and <paramref name="length"/>.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    private static void Select(ITextBoxController sender, int start, int length)
    {
      TextEditor element;
      if (!elements.TryGetValue(sender, out element))
        throw new ArgumentException("sender");

      // element.Focus();

      element.Select(start, length);
      TextLocation loc = element.Document.GetLocation(start);
      element.ScrollTo(loc.Line, loc.Column);
    }

    /// <summary>
    /// Scroll to a specific line in the currently displayed editor text
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="line"></param>
    private static void ScrollToLine(ITextBoxController sender, int line)
    {
      TextEditor element;
      if (!elements.TryGetValue(sender, out element))
        throw new ArgumentException("sender");

      element.Focus();
      element.ScrollToLine(line);
    }

    /// <summary>
    /// Get the state of the current selection start, end and whether its rectangular or not.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <param name="IsRectangularSelection"></param>
    private static void CurrentSelection(ITextBoxController sender,
                                         out int start, out int length, out bool IsRectangularSelection)
    {
      TextEditor element;

      if (!elements.TryGetValue(sender, out element))
        throw new ArgumentException("sender");

      start = element.SelectionStart;
      length = element.SelectionLength;
      IsRectangularSelection = element.TextArea.Selection.EnableVirtualSpace;

      // element.TextArea.Selection = RectangleSelection.Create(element.TextArea, start, length);
    }

    private static void BeginChange(ITextBoxController sender)
    {
      TextEditor element;

      if (!elements.TryGetValue(sender, out element))
        throw new ArgumentException("sender");

      element.BeginChange();
    }

    private static void EndChange(ITextBoxController sender)
    {
      TextEditor element;

      if (!elements.TryGetValue(sender, out element))
        throw new ArgumentException("sender");

      element.EndChange();
    }

    private static void GetSelectedText(ITextBoxController sender, out string selectedText)
    {
      TextEditor element;
      selectedText = string.Empty;

      if (!elements.TryGetValue(sender, out element))
        throw new ArgumentException("sender");

      selectedText = element.SelectedText;
    }
    #endregion ITextBoxController_methods
  }
}
