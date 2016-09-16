namespace MsgBox.Internal.ViewModel
{
  using System;
  using System.Diagnostics;
  using System.Reflection;
  using System.Windows;
  using System.Windows.Input;
  using System.Windows.Media;
  using System.Windows.Media.Imaging;

  using Commands;

  /// <summary>
  /// Source:
  /// http://blogsprajeesh.blogspot.de/2009/12/wpf-messagebox-custom-control-updated.html
  /// http://prajeeshprathap.codeplex.com/sourcecontrol/list/patches?ProjectName=prajeeshprathap
  /// 
  /// A viewmodel that drives an advanced message box dialog window through its life cycle.
  /// This message box supports:
  /// - Custom images
  /// - Help Link Navigation for advanced research in online resources (by the user)
  /// - (Expander) section with more textual/technical details
  /// </summary>
  internal class MsgBoxViewModel : ViewModel.Base.BaseViewModel
  {
    #region fields
    private static string[] msgBoxImageResourcesUris =
    {
       "48px-Emblem-important-yellow.svg.png",
       "48px-Help-browser.svg.png",
       "48px-Dialog-error-round.svg.png",
       "48px-Dialog-accept.svg.png",
       "48px-Software-update-urgent.svg.png",
       "48px-Dialog-information_on.svg.png",
       "48px-Emblem-notice.svg.png",

       // Advanced Icon Set
       "48px-Dialog-information.svg.png",
       "48px-Dialog-information_red.svg.png",
       "48px-Emblem-important.svg.png",
       "48px-Emblem-important-red.svg.png",
       "48px-Process-stop.svg.png"
    };

    private string mTitle;
    private string mMessage;
    private string mInnerMessageDetails;

    private bool mYesNoVisibility;
    private bool mCancelVisibility;
    private bool mOKVisibility;
    private bool mCloseVisibility;
    private bool mShowDetails;

    private RelayCommand mYesCommand;
    private RelayCommand mNoCommand;
    private RelayCommand mCancelCommand;
    private RelayCommand<string> mCloseCommand;
    private RelayCommand mOKCommand;
    private RelayCommand<string> mNavigateToUri;
    private RelayCommand<string> mCopyText;

    private bool mEnableCopyFunction;

    private ImageSource mMessageImageSource;
    private ImageSource mCopyImageSource;

    private MsgBoxResult mIsDefaultButton;

    private MsgBoxResult mResult;
    private bool? mDialogCloseResult;

    private string mHyperlinkLabel = string.Empty;
    private object mHelpLink;
    private string mHelpLinkTitle;
    private Func<object, bool> mNavigateHyperlinkMethod = MsgBoxViewModel.NavigateToUniversalResourceIndicator;
    #endregion fields

    #region constructor
    /// <summary>
    /// Class constructor
    /// </summary>
    /// <param name="caption"></param>
    /// <param name="messageBoxText"></param>
    /// <param name="innerMessage"></param>
    /// <param name="buttonOption"></param>
    /// <param name="image"></param>
    /// <param name="defaultButton"></param>
    /// <param name="helpLink"></param>
    /// <param name="helpLinkTitle"></param>
    /// <param name="navigateHelplinkMethod"></param>
    /// <param name="enableCopyFunction"></param>
    internal MsgBoxViewModel(string messageBoxText, 
                              string caption,
                              string innerMessage,
                              MsgBoxButtons buttonOption,
                              MsgBoxImage image,
                              MsgBoxResult defaultButton = MsgBoxResult.None,
                              object helpLink = null,
                              string helpLinkTitle = "",
                              Func<object, bool> navigateHelplinkMethod = null,
                              bool enableCopyFunction = false)
    {
      this.Title = caption;
      this.Message = messageBoxText;
      this.InnerMessageDetails = innerMessage;

      this.SetButtonVisibility(buttonOption);

      this.mIsDefaultButton = this.SetupDefaultButton(buttonOption, defaultButton);

      this.SetImageSource(image);
      this.mHelpLink = helpLink;
      this.mHelpLinkTitle = helpLinkTitle;

      this.mResult = MsgBoxResult.None;
      this.mDialogCloseResult = null;

      if (navigateHelplinkMethod != null)
        this.mNavigateHyperlinkMethod = navigateHelplinkMethod;

      this.EnableCopyFunction = enableCopyFunction;
    }
    #endregion constructor

    #region properties
    /// <summary>
    /// Title of message shown to the user (this is usally the Window title)
    /// </summary>
    public string Title
    {
      get
      {
        return this.mTitle;
      }

      set
      {
        if (this.mTitle != value)
        {
          this.mTitle = value;
          this.NotifyPropertyChanged(() => this.Title);
        }
      }
    }

    /// <summary>
    /// Message content that tells the user what the problem is
    /// (why is it a problem, how can it be fixed,
    ///  and clicking which button will do what resolution [if any] etc...).
    /// </summary>
    public string Message
    {
      get
      {
        return this.mMessage;
      }

      set
      {
        if (this.mMessage != value)
        {
          this.mMessage = value;
          this.NotifyPropertyChanged(() => this.Message);
        }
      }
    }

    /// <summary>
    /// More message details displayed in an expander (this can, for example,
    /// by a stacktrace or other technical information that can be shown for
    /// trouble shooting advanced scenarious via copy button - CSC etc...).
    /// </summary>
    public string InnerMessageDetails
    {
      get
      {
        return this.mInnerMessageDetails;
      }

      set
      {
        if (this.mInnerMessageDetails != value)
        {
          this.mInnerMessageDetails = value;
          this.NotifyPropertyChanged(() => this.InnerMessageDetails);
        }
      }
    }

    /// <summary>
    /// Get/set property to determine a image that is shown to the user.
    /// The image in turn gives an impression whether a messagebox shows
    /// an error, an urgent problem, just an information or anything else...
    /// 
    /// This property represents the actual IMAGE not the enumeration.
    /// </summary>
    public ImageSource MessageImageSource
    {
      get
      {
        return this.mMessageImageSource;
      }

      set
      {
        this.mMessageImageSource = value;
        this.NotifyPropertyChanged(() => this.MessageImageSource);
      }
    }

    /// <summary>
    /// Get/set property to determine a image in the copy message button
    /// of the dialog.
    /// 
    /// This property represents the actual IMAGE not the enumeration.
    /// </summary>
    public ImageSource CopyImageSource
    {
      get
      {
        return this.mCopyImageSource;
      }

      set
      {
        this.mCopyImageSource = value;
        this.NotifyPropertyChanged(() => this.CopyImageSource);
      }
    }

    /// <summary>
    /// Get/set visibility of Yes/No buttons
    /// </summary>
    public bool YesNoVisibility
    {
      get
      {
        return this.mYesNoVisibility;
      }

      set
      {
        if (this.mYesNoVisibility != value)
        {
          this.mYesNoVisibility = value;
          this.NotifyPropertyChanged(() => this.YesNoVisibility);
        }
      }
    }

    /// <summary>
    /// Get/set visibility of Cancel button
    /// </summary>
    public bool CancelVisibility
    {
      get
      {
        return this.mCancelVisibility;
      }

      set
      {
        if (this.mCancelVisibility != value)
        {
          this.mCancelVisibility = value;
          this.NotifyPropertyChanged(() => this.CancelVisibility);
        }
      }
    }

    /// <summary>
    /// Get/set visibility of OK button
    /// </summary>
    public bool OkVisibility
    {
      get
      {
        return this.mOKVisibility;
      }

      set
      {
        if (this.mOKVisibility != value)
        {
          this.mOKVisibility = value;
          this.NotifyPropertyChanged(() => this.OkVisibility);
        }
      }
    }

    /// <summary>
    /// Get/set visibility of Close button
    /// </summary>
    public bool CloseVisibility
    {
      get
      {
        return this.mCloseVisibility;
      }

      set
      {
        if (this.mCloseVisibility != value)
        {
          this.mCloseVisibility = value;
          this.NotifyPropertyChanged(() => this.CloseVisibility);
        }
      }
    }

    /// <summary>
    /// Get/set visibility of Show Details section in dialog
    /// </summary>
    public bool ShowDetails
    {
      get
      {
        return this.mShowDetails;
      }

      set
      {
        if (this.mShowDetails != value)
        {
          this.mShowDetails = value;
          this.NotifyPropertyChanged(() => this.ShowDetails);
        }
      }
    }

    /// <summary>
    /// Get property to determine the default button (if any)
    /// to be used in the dialog (user can hit ENTER key to execute that function).
    /// </summary>
    public MsgBoxResult IsDefaultButton
    {
      get
      {
        return this.mIsDefaultButton;
      }
    }

    /// <summary>
    /// Get/set property to determine whether the copy message
    /// function is available to the user or not (default: available).
    /// </summary>
    public bool EnableCopyFunction
    {
      get
      {
        return this.mEnableCopyFunction;
      }

      set
      {
        if (this.mEnableCopyFunction != value)
        {
          this.mEnableCopyFunction = value;
          this.NotifyPropertyChanged(() => this.EnableCopyFunction);
        }
      }
    }

    /// <summary>
    /// Get property to determine whether the dialog can be closed with
    /// the corresponding result or not. This property is typically used
    /// with an attached behaviour (<seealso cref="DialogCloser"/>) in the Views's XAML.
    /// </summary>
    public bool? DialogCloseResult
    {
      get
      {
        return this.mDialogCloseResult;
      }

      private set
      {
        if (this.mDialogCloseResult != value)
        {
          this.mDialogCloseResult = value;
          this.NotifyPropertyChanged(() => this.DialogCloseResult);
        }
      }
    }

    /// <summary>
    /// Get the resulting button that has been clicked by the user when working with the message box.
    /// </summary>
    public MsgBoxResult Result
    {
      get
      {
        return this.mResult;
      }

      private set
      {
        if (this.mResult != value)
        {
          this.mResult = value;
          this.NotifyPropertyChanged(() => this.Result);
        }
      }
    }

    #region Help Hyperlink
    public string HyperlinkLabel
    {
      get
      {
        return (this.mHyperlinkLabel == null ? string.Empty : this.mHyperlinkLabel);
      }

      set
      {
        if (this.mHyperlinkLabel != value)
        {
          this.mHyperlinkLabel = value;
          this.NotifyPropertyChanged(() => this.HyperlinkLabel);
        }
      }
    }

    /// <summary>
    /// Get/set property to determine the address to browsed to when displaying a help link.
    /// </summary>
    public string HelpLink
    {
      get
      {
        try
        {
          if (this.mHelpLink != null)
            return this.mHelpLink.ToString();
        }
        catch
        {
        }
        
        return string.Empty;
      }

      set
      {
        if (object.Equals(this.mHelpLink, value) != true)
        {
          this.mHelpLink = value;
          this.NotifyPropertyChanged(() => this.HelpLink);
          this.NotifyPropertyChanged(() => this.AllToString);
        }
      }
    }

    /// <summary>
    /// Get/set property to determine the text for displaying a help link.
    /// By default the text is the toString content of the <seealso cref="HelpLink"/>
    /// but it can also be a different text if that text is set in the constructor.
    /// </summary>
    public string HelpLinkTitle
    {
      get
      {
        if ((this.mHelpLinkTitle == null ? string.Empty : this.mHelpLinkTitle).Length <= 0)
          return this.HelpLink;

        return this.mHelpLinkTitle;
      }

      set
      {
        if (this.mHelpLinkTitle != value)
        {
          this.mHelpLinkTitle = value;
          this.NotifyPropertyChanged(() => this.HelpLinkTitle);
          this.NotifyPropertyChanged(() => this.AllToString);
        }
      }
    }

    /// <summary>
    /// Get property to determine whether a helplink should be display or not.
    /// A helplink should not be displayed if there is no HelpLink information
    /// available, and it can be dispalyed otherwise.
    /// </summary>
    public bool DisplayHelpLink
    {
      get
      {
        return ((this.mHelpLink == null ? string.Empty : this.HelpLink).Trim().Length > 0);
      }
    }
    #endregion Help Hyperlink

    /// <summary>
    /// Get property to get all textual information in one text block.
    /// This property is typically used to copy all text (even details)
    /// to the clipboard so users can paste it into their email and send
    /// the problem description off to those who care and know...
    /// </summary>
    public string AllToString
    {
      get
      {
        return string.Format("Title: {0}\n Message: {1}\n Help Link: {2}\nHelp Link Url: {3}\nMore Details: {4}\n",
                              this.Title, this.Message, this.HelpLinkTitle, this.HelpLink, this.InnerMessageDetails);
      }
    }

    #region Commanding
    public ICommand YesCommand
    {
      get
      {
        if (this.mYesCommand == null)
          this.mYesCommand = new RelayCommand(() =>
          {
            this.Result = MsgBoxResult.Yes;
            this.DialogCloseResult = true;
          });

        return this.mYesCommand;
      }
    }

    public ICommand NoCommand
    {
      get
      {
        if (this.mNoCommand == null)
          this.mNoCommand = new RelayCommand(() =>
          {
            this.Result = MsgBoxResult.No;
            this.DialogCloseResult = true;
          });
        return this.mNoCommand;
      }
    }

    public ICommand CancelCommand
    {
      get
      {
        if (this.mCancelCommand == null)
          this.mCancelCommand = new RelayCommand(() =>
          {
            this.Result = MsgBoxResult.Cancel;
            this.DialogCloseResult = true;
          });

        return this.mCancelCommand;
      }
    }

    public ICommand CloseCommand
    {
      get
      {
        if (this.mCloseCommand == null)
          this.mCloseCommand = new RelayCommand<string>((p) =>
          {
            string ComParam = p as string;
            bool bSetResult = true;

            if (ComParam != null)                 // Interpret close window (ESC Key) as Cancel
            {
              if (ComParam.ToLower() == "cancel" && this.CancelVisibility == true)
              {
                this.Result = MsgBoxResult.Cancel;
                bSetResult = false;
              }
            }

            if (bSetResult == true)
              this.Result = MsgBoxResult.Close;

            this.DialogCloseResult = true;
          });

        return this.mCloseCommand;
      }
    }

    public ICommand OkCommand
    {
      get
      {
        if (this.mOKCommand == null)
          this.mOKCommand = new RelayCommand(() =>
          {
            this.Result = MsgBoxResult.Ok;
            this.DialogCloseResult = true;
          });

        return this.mOKCommand;
      }
    }

    /// <summary>
    /// Execute a command that starts a new (browser)
    /// process to navigate to this (web) target
    /// </summary>
    public ICommand NavigateToUri
    {
      get
      {
        if (this.mNavigateToUri == null)
          this.mNavigateToUri = new RelayCommand<string>((param) => this.mNavigateHyperlinkMethod((string)param));

        return this.mNavigateToUri;
      }
    }

    /// <summary>
    /// Execute a command to copy the text string supplied
    /// as parameter into the clipboard
    /// </summary>
    public ICommand CopyText
    {
      get
      {
        if (this.mCopyText == null)
          this.mCopyText = new RelayCommand<string>((param) => MsgBoxViewModel.CopyTextToClipboard((string)param));

        return this.mCopyText;
      }
    }
    #endregion Commanding
    #endregion properties

    #region methods
    /// <summary>
    /// Write the supplied string into the Wiindows Clipboard such that
    /// users can past it into their favourite text editor
    /// </summary>
    /// <param name="textToCopy"></param>
    private static void CopyTextToClipboard(string textToCopy)
    {
      if (textToCopy == null) return;

      System.Windows.Clipboard.SetText(textToCopy);
    }

    /// <summary>
    /// Default method for navigating the hyperlink. A different method can
    /// be invoked if the corresponding constructor was used and
    /// <seealso cref="mNavigateHyperlinkMethod"/> was set
    /// (this method is ignorred in this case).
    /// </summary>
    /// <param name="uriTarget"></param>
    /// <returns></returns>
    private static bool NavigateToUniversalResourceIndicator(object uriTarget)
    {
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

    private void SetButtonVisibility(MsgBoxButtons buttonOption)
    {
      switch (buttonOption)
      {
        case MsgBoxButtons.OKCancel:
          this.YesNoVisibility = false;
          this.CancelVisibility = true;
          this.OkVisibility = true;
          this.CloseVisibility = false;
          break;

        case MsgBoxButtons.YesNo:
          this.YesNoVisibility = true;
          this.CancelVisibility = false;
          this.OkVisibility = false;
          this.CloseVisibility = false;
          break;

        case MsgBoxButtons.YesNoCancel:
          this.YesNoVisibility = true;
          this.CancelVisibility = true;
          this.OkVisibility = false;
          this.CloseVisibility = false;
          break;

        case MsgBoxButtons.OK:
          this.YesNoVisibility = false;
          this.CancelVisibility = false;
          this.OkVisibility = true;
          this.CloseVisibility = false;
          break;

        case MsgBoxButtons.OKClose:
          this.YesNoVisibility = false;
          this.CancelVisibility = false;
          this.OkVisibility = true;
          this.CloseVisibility = true;
          break;

        case MsgBoxButtons.Close:
          this.YesNoVisibility = false;
          this.CancelVisibility = false;
          this.OkVisibility = false;
          this.CloseVisibility = true;
          break;

        default:
          this.YesNoVisibility = false;
          this.CancelVisibility = false;
          this.OkVisibility = true;
          this.CloseVisibility = true;
          break;
      }

      if (string.IsNullOrEmpty(this.InnerMessageDetails))
        this.ShowDetails = false;
      else
        this.ShowDetails = true;
    }

    /// <summary>
    /// Set the image to be displayed in the messagebox
    /// </summary>
    /// <param name="image"></param>
    private void SetImageSource(MsgBoxImage image)
    {
      string resourceAssembly = Assembly.GetAssembly(typeof(MsgBoxViewModel)).GetName().Name;

      string folder = "Images/MsgBoxImages/";

      // Tango Icon set: http://commons.wikimedia.org/wiki/Tango_icons
      // Default image displayed in message box
      string source = string.Format("pack://application:,,,/{0};component/{1}48px-Dialog-information_on.svg.png",
                                    resourceAssembly, folder);

      try
      {
        source = string.Format("pack://application:,,,/{0};component/{1}{2}",
                                 resourceAssembly,
                                 folder,
                                 MsgBoxViewModel.msgBoxImageResourcesUris[(int)image]);
      }
      catch (Exception)
      {
      }

      Uri imageUri = new Uri(source, UriKind.RelativeOrAbsolute);
      this.MessageImageSource = new BitmapImage(imageUri);
    }

    /// <summary>
    /// Determine a default button (such as OK or Yes) to be executed when the user hits the ENTER key.
    /// </summary>
    /// <param name="buttonOption"></param>
    private MsgBoxResult SetupDefaultButton(MsgBoxButtons buttonOption,
                                            MsgBoxResult defaultButton)
    {
      MsgBoxResult ret = defaultButton;

      // Lets define a useful default button (can be executed with ENTER)
      // if caller did not define a button or
      // if did not explicitly told the sub-system to not define a default button via MsgBoxResult.NoDefaultButton
      if (defaultButton == MsgBoxResult.None)
      {
        switch (buttonOption)
        {
          case MsgBoxButtons.Close:
            ret = MsgBoxResult.Close;
            break;

          case MsgBoxButtons.OK:
          case MsgBoxButtons.OKCancel:
          case MsgBoxButtons.OKClose:
            ret = MsgBoxResult.Ok;
            break;

          case MsgBoxButtons.YesNo:
          case MsgBoxButtons.YesNoCancel:
            ret = MsgBoxResult.Yes;
            break;
        }
      }

      return ret;
    }
    #endregion methods
  }
}
