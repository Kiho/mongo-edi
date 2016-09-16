namespace MsgBoxSamples
{
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.Data;
  using System.Linq;
  using System.Windows;
  using System.Windows.Threading;
  using MsgBox;

  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    public App()
    {
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
      string message = string.Empty;

      try
      {
        MsgBoxBase.GetService<IMsgBoxService>().Show(
            e.Exception, "An unexpected Error occured",
            MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
            "http://www.codeproject.com/script/Articles/MemberArticles.aspx?amid=7799028",
            "http://www.codeproject.com/script/Articles/MemberArticles.aspx?amid=7799028",
            "Please click on the link to check if this is a known problem (and report it if not):");
      }
      catch
      {
        MessageBox.Show(message, "Error Report", MessageBoxButton.OK, MessageBoxImage.Error);
      }

      e.Handled = true;
    }

  }
}
