namespace Edi.AvalonEdit
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using ICSharpCode.AvalonEdit;
  using ICSharpCode.AvalonEdit.Rendering;
  using System.Windows.Media;
  using System.Windows;

  /// <summary>
  /// Source: http://stackoverflow.com/questions/5072761/avalonedit-highlight-current-line-even-when-not-focused
  /// </summary>
  public class HighlightCurrentLineBackgroundRenderer : IBackgroundRenderer
  {
    private TextEditor mEditor;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="editor"></param>
    public HighlightCurrentLineBackgroundRenderer(TextEditor editor,
                                                  SolidColorBrush HighlightBackgroundColorBrush = null)
    {
      this.mEditor = editor;

      // Light Blue 0x100000FF
      this.BackgroundColorBrush = new SolidColorBrush((HighlightBackgroundColorBrush == null ? Color.FromArgb(0x10, 0x00, 0x00, 0xFF) :
                                                                                               HighlightBackgroundColorBrush.Color));
    }

    public KnownLayer Layer
    {
      get { return KnownLayer.Background; }
    }

    /// <summary>
    /// Get/Set color brush to show for highlighting current line
    /// </summary>
    public SolidColorBrush BackgroundColorBrush { get; set; }

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
      if (this.mEditor.Document == null)
        return;

      textView.EnsureVisualLines();
      var currentLine = mEditor.Document.GetLineByOffset(mEditor.CaretOffset);

      foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, currentLine))
      {
        drawingContext.DrawRectangle(new SolidColorBrush(this.BackgroundColorBrush.Color), null,
                                     new Rect(rect.Location, new Size(textView.ActualWidth, rect.Height)));
      }
    }
  }
}
