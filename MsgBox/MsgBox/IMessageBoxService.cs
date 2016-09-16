namespace MsgBox
{
  using System;

  public interface IMsgBoxService
  {
    MsgBoxResult Show(string messageBoxText,
                      MsgBoxResult btnDefault = MsgBoxResult.None,
                      object helpLink = null,
                      string helpLinkTitle = "",
                      string helpLabel = "",
                      Func<object, bool> navigateHelplinkMethod = null,
                      bool showCopyMessage = false);

    MsgBoxResult Show(string messageBoxText, string caption,
                      MsgBoxResult btnDefault = MsgBoxResult.None,
                      object helpLink = null,
                      string helpLinkTitle = "",
                      string helpLabel = "",
                      Func<object, bool> navigateHelplinkMethod = null,
                      bool showCopyMessage = false);

    MsgBoxResult Show(string messageBoxText, string caption,
                      MsgBoxButtons buttonOption,
                      MsgBoxResult btnDefault = MsgBoxResult.None,
                      object helpLink = null,
                      string helpLinkTitle = "",
                      string helpLabel = "",
                      Func<object, bool> navigateHelplinkMethod = null,
                      bool showCopyMessage = false);

    MsgBoxResult Show(string messageBoxText, string caption,
                      MsgBoxButtons buttonOption, MsgBoxImage image,
                      MsgBoxResult btnDefault = MsgBoxResult.None,
                      object helpLink = null,
                      string helpLinkTitle = "",
                      string helpLabel = "",
                      Func<object, bool> navigateHelplinkMethod = null,
                      bool showCopyMessage = false);

    MsgBoxResult Show(string messageBoxText, string caption,
                      string details,
                      MsgBoxButtons buttonOption, MsgBoxImage image,
                      MsgBoxResult btnDefault = MsgBoxResult.None,
                      object helpLink = null,
                      string helpLinkTitle = "",
                      string helpLabel = "",
                      Func<object, bool> navigateHelplinkMethod = null,
                      bool showCopyMessage = false);

    MsgBoxResult Show(Exception exp, string caption,
                      MsgBoxButtons buttonOption, MsgBoxImage image,
                      MsgBoxResult btnDefault = MsgBoxResult.None,
                      object helpLink = null,
                      string helpLinkTitle = "",
                      string helpLabel = "",
                      Func<object, bool> navigateHelplinkMethod = null,
                      bool showCopyMessage = false);
  }
}
