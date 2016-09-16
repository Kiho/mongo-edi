namespace MsgBox.Internal
{
  using System;

  /// <summary>
  /// A service that shows message boxes.
  /// 
  /// Source: http://www.codeproject.com/Articles/70223/Using-a-Service-Locator-to-Work-with-MessageBoxes
  /// </summary>
  internal class MessageBoxService : IMsgBoxService
  {
    MsgBoxResult IMsgBoxService.Show(string messageBoxText,
                                         MsgBoxResult btnDefault,
                                         object helpLink,
                                         string helpLinkTitle, string helpLinkLabel,
                                         Func<object, bool> navigateHelplinkMethod,
                                         bool showCopyMessage)
    {
      return View.MsgBox.Show(messageBoxText, btnDefault,
                                  helpLink, helpLinkTitle, helpLinkLabel, navigateHelplinkMethod, showCopyMessage);
    }

    MsgBoxResult IMsgBoxService.Show(string messageBoxText,
                                         string caption,
                                         MsgBoxResult btnDefault,
                                         object helpLink,
                                         string helpLinkTitle, string helpLinkLabel,
                                         Func<object, bool> navigateHelplinkMethod,
                                         bool showCopyMessage)
    {
      return View.MsgBox.Show(messageBoxText, caption, btnDefault,
                                  helpLink, helpLinkTitle, helpLinkLabel, navigateHelplinkMethod, showCopyMessage);
    }

    MsgBoxResult IMsgBoxService.Show(string messageBoxText,
                                         string caption,
                                         MsgBoxButtons buttonOption,
                                         MsgBoxResult btnDefault,
                                         object helpLink,
                                         string helpLinkTitle, string helpLinkLabel,
                                         Func<object, bool> navigateHelplinkMethod,
                                         bool showCopyMessage)
    {
      return View.MsgBox.Show(messageBoxText, caption, buttonOption, btnDefault,
                                  helpLink, helpLinkTitle, helpLinkLabel, navigateHelplinkMethod, showCopyMessage);
    }

    MsgBoxResult IMsgBoxService.Show(string messageBoxText,
                                         string caption,
                                         MsgBoxButtons buttonOption,
                                         MsgBoxImage image,
                                         MsgBoxResult btnDefault,
                                         object helpLink,
                                         string helpLinkTitle, string helpLinkLabel,
                                         Func<object, bool> navigateHelplinkMethod,
                                         bool showCopyMessage)
    {
      return View.MsgBox.Show(messageBoxText, caption, buttonOption, image, btnDefault,
                                  helpLink, helpLinkTitle, helpLinkLabel, navigateHelplinkMethod, showCopyMessage);
    }

    /// <summary>
    /// Show a messagebox with details (such as a stacktrace or other information that can be kept in an expander).
    /// </summary>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <param name="details"></param>
    /// <param name="buttonOption"></param>
    /// <param name="image"></param>
    /// <returns></returns>
    MsgBoxResult IMsgBoxService.Show(string messageBoxText,
                                         string caption,
                                         string details,
                                         MsgBoxButtons buttonOption,
                                         MsgBoxImage image,
                                         MsgBoxResult btnDefault,
                                         object helpLink,
                                         string helpLinkTitle, string helpLinkLabel,
                                         Func<object, bool> navigateHelplinkMethod,
                                         bool showCopyMessage)
    {
      return View.MsgBox.Show(messageBoxText, caption, details,
                                  buttonOption, image, btnDefault,
                                  helpLink, helpLinkTitle,  helpLinkLabel, navigateHelplinkMethod,
                                  showCopyMessage);
    }

    MsgBoxResult IMsgBoxService.Show(Exception exp, string caption,
                                         MsgBoxButtons buttonOption, MsgBoxImage image,
                                         MsgBoxResult btnDefault = MsgBoxResult.None,
                                         object helpLink = null,
                                         string helpLinkTitle = "",
                                         string helpLabel = "",
                                         Func<object, bool> navigateHelplinkMethod = null,
                                         bool showCopyMessage = false)
    {
      return View.MsgBox.Show(exp, caption, 
                              buttonOption, image, btnDefault,
                              helpLink, helpLinkTitle, helpLabel, navigateHelplinkMethod,
                              showCopyMessage);
    }
  }
}
