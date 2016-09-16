using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EdiViews.FindReplace
{
  /// <summary>
  /// Implement a view that supports text Find/Replace functionality in an editor
  /// </summary>
  public class FindReplaceView : Control
  {
    private TextBox mTxtFind = null;
    private TextBox mTxtFind2 = null;
    private TextBox mTxtReplace = null;

    static FindReplaceView()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(FindReplaceView), new FrameworkPropertyMetadata(typeof(FindReplaceView)));
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      try
      {
        this.mTxtFind = this.GetTemplateChild("PART_TxtFind") as TextBox;
        this.mTxtFind2 = this.GetTemplateChild("PART_TxtFind2") as TextBox;
        this.mTxtReplace = this.GetTemplateChild("PART_Replace") as TextBox; ; ;

        // Setting focus into each textbox control is controlled via viewmodel and attached property
        // Each textbox selects all content (by default) when it aquires the focus
        if (this.mTxtFind != null)
        {
          this.mTxtFind.GotKeyboardFocus += (s, e) =>
          {
            this.mTxtFind.SelectAll();
          };
        }

        if (this.mTxtFind2 != null)
        {
          this.mTxtFind2.GotKeyboardFocus += (s, e) =>
          {
            this.mTxtFind2.SelectAll();
          };
        }

        if (this.mTxtReplace != null)
        {
          this.mTxtReplace.GotKeyboardFocus += (s, e) =>
          {
            this.mTxtReplace.SelectAll();
          };
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
      }
    }
  }
}
