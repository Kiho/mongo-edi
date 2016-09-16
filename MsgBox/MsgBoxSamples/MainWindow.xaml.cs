namespace MsgBoxSamples
{
  using System;
  using System.Diagnostics;
  using System.Windows;
  using System.Windows.Controls;
  using MsgBox;

  /// <summary>
  /// Interaction logic for Window1.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    #region constructor
    public MainWindow()
    {
      this.InitializeComponent();
    }
    #endregion constructor

    #region methods
    private void Button_Click(object sender, RoutedEventArgs e)
    {
      var btn = sender as Button;
      if (btn == null) return;

      MsgBoxResult result;
      var msgBox = MsgBoxBase.GetService<IMsgBoxService>();
      if (msgBox != null)
      {
        switch (btn.Content.ToString())
        {
          case "Sample 1":
            result = msgBox.Show("This options displays a message box with only message." +
                                 "\nThis is the message box with minimal options (just an OK button and no caption).");
            break;
          case "Sample 2":
            result = msgBox.Show("This options displays a message box with both title and message.\nA default image and OK button are displayed.",
                                 "WPF MessageBox");
            break;
          case "Sample 3":
            result = msgBox.Show("This options displays a message box with YES, NO, CANCEL option.",
                                 "WPF MessageBox",
                                 MsgBoxButtons.YesNoCancel, MsgBoxImage.Question);
            break;
          case "Sample 4":
            {
              Exception exp = this.CreateDemoException();

              result = msgBox.Show(exp.Message, "Unexpected Error",
                                    exp.ToString(), MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                                    "http://www.codeproject.com/script/Articles/MemberArticles.aspx?amid=7799028",
                                   "http://www.codeproject.com/script/Articles/MemberArticles.aspx?amid=7799028",
                                   "Click the copy button and report this problem here:");
            }
            break;
          case "Sample 5":
            result = msgBox.Show("This options displays a message box with YES, NO buttons.",
                                 "WPF MessageBox",
                                 MsgBoxButtons.YesNo, MsgBoxImage.Question);

            break;

          case "Sample 6":
            result = msgBox.Show("This options displays a message box with Yes, No (No as default) options.",
                                 "WPF MessageBox",
                                 MsgBoxButtons.YesNo, MsgBoxImage.Question, MsgBoxResult.No);
            break;

          case "Sample 7":
            result = msgBox.Show("Are you sure? Click the hyperlink to review the get more details.",
                                 "WPF MessageBox with Hyperlink",
                                 MsgBoxButtons.YesNo, MsgBoxImage.Question, MsgBoxResult.Yes,
                                 "http://www.codeproject.com/script/Articles/MemberArticles.aspx?amid=7799028",
                                 "Code Project Articles by Dirkster99");
            break;

          case "Sample 8":
            result = msgBox.Show("Are you sure? Click the hyperlink to review the get more details.",
                                 "WPF MessageBox with Custom Hyperlink Navigation",
                                 MsgBoxButtons.YesNo, MsgBoxImage.Question, MsgBoxResult.Yes,
                                 "http://www.codeproject.com/script/Articles/MemberArticles.aspx?amid=7799028",
                                 "Code Project Articles by Dirkster99", "Help Topic:", this.MyCustomHyperlinkNaviMethod);
            break;

          case "Sample 9":
            result = msgBox.Show("WPF MessageBox without Copy Button (OK and Cancel [default])",
                                 "Are you sure this right?",
                                 MsgBoxButtons.OKCancel, MsgBoxImage.Question, MsgBoxResult.Cancel,
                                 null, string.Empty, string.Empty, null, false);
            break;

          case "Sample 10":
            result = msgBox.Show("Are you sure? Click the hyperlink to review the get more details.",
                                 "WPF MessageBox without Default Button",
                                 MsgBoxButtons.YesNo, MsgBoxImage.Question, MsgBoxResult.NoDefaultButton,
                                 null, string.Empty, string.Empty, null, false);
            break;

          case "Sample 11":
            result = msgBox.Show("...display a messageBox with a close button and TakeNote icon.",
                                 "WPF MessageBox with a close button",
                                 MsgBoxButtons.Close, MsgBoxImage.Warning);
            break;

          case "Sample 12":
            {
              Exception exp = this.CreateDemoException();

              result = msgBox.Show(exp, "Unexpected Error",
                                   MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                                    "http://www.codeproject.com/script/Articles/MemberArticles.aspx?amid=7799028",
                                   "http://www.codeproject.com/script/Articles/MemberArticles.aspx?amid=7799028",
                                   "Please click on the link to check if this is a known problem (and report it if not):", null, true);
            }
            break;
        }
      }
    }

    /// <summary>
    /// This is just a mockup test method to test whether custom
    /// hyperlink navigation will work when using custom hyperlink
    /// navigation methods.
    /// </summary>
    /// <param name="uriTarget"></param>
    /// <returns></returns>
    private bool MyCustomHyperlinkNaviMethod(object uriTarget)
    {
      MessageBox.Show("Starting Navigation to: " + uriTarget.ToString(), "Mockup Test Info");

      string uriTargetString = uriTarget as string;

      try
      {
        if (uriTargetString != null)
        {
          Process.Start(new ProcessStartInfo(uriTargetString));
          return true;
        }
      }
      catch
      {
      }

      return false;
    }

    #region DemoException
    private Exception CreateDemoException()
    {
      int i = 0;

      try
      {
        this.CreateDemoException1();
      }
      catch (Exception exp)
      {
        return exp;
      }

      return null;
    }

    private void CreateDemoException1()
    {
      try
      {
        this.CreateDemoException2();
      }
      catch (Exception exp)
      {
        throw new Exception("A sub-system failure occured.", exp);
      }
    }

    private void CreateDemoException2()
    {
      int i = 0;

      try
      {
        int x = 1 / i;
      }
      catch (Exception exp)
      {
        throw new Exception("A sub-sub-system failure occured.", exp);
      }
    }
    #endregion DemoException
    #endregion methods
  }
}
