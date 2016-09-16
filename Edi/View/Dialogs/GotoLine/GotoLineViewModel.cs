namespace Edi.View.Dialogs.GotoLine
{
  using SimpleControls.MRU.ViewModel;
  using System.Collections.Generic;
  using Edi.ViewModel.Base;
  using Edi.Command;
  using System.Globalization;

  public class GotoLineViewModel : ViewModelBase
  {
    private int mMin, mMax, iCurrentLine;

    public GotoLineViewModel(int iMin, int iMax, int iCurrentLine)
    {
      this.mMin = iMin;
      this.mMax = iMax;
      this.iCurrentLine = iCurrentLine;

      this.mLineNumberInput = iCurrentLine.ToString();
    }

    #region properties
    /// <summary>
    /// Get property to expose elements necessary to evaluate user input
    /// when the user completes his input (eg.: clicks OK in a dialog).
    /// </summary>
    private DialogViewModelBase mOpenCloseView;
    public DialogViewModelBase OpenCloseView
    {
      get
      {
        return this.mOpenCloseView;
      }

      private set
      {
        if (this.mOpenCloseView != value)
        {
          this.mOpenCloseView = value;

          this.NotifyPropertyChanged(() => this.OpenCloseView);
        }
      }
    }

    private string mLineNumberInput;
    public string LineNumberInput
    {
      get
      {
        return (this.mLineNumberInput == null ? string.Empty : this.mLineNumberInput);
      }

      set
      {
        if (this.mLineNumberInput != value)
        {
          this.mLineNumberInput = value;

          this.NotifyPropertyChanged(() => this.LineNumberInput);
        }
      }
    }

    public int LineNumber
    {
      get
      {
        int iNumber = -1;

        try
        {
          iNumber = int.Parse(this.mLineNumberInput);
        }
        catch
        {
        }

        return iNumber;
      }
    }

    public string MinMaxRange
    {
      get
      {
        return "(" + this.mMin.ToString() + " - " + this.mMax.ToString() + ")";
      }
    }

    private string mSelectedText = string.Empty;
    public string SelectedText
    {
      get
      {
        return this.mSelectedText;
      }

      set
      {
        if (this.mSelectedText != value)
        {
          this.mSelectedText = value;
          this.NotifyPropertyChanged(() => this.SelectedText);
        }
      }
    }
    #endregion properties

    #region methods
    /// <summary>
    /// Initilize input states such that user can input information
    /// with a view based GUI (eg.: dialog)
    /// </summary>
    public void InitDialogInputData()
    {
      this.OpenCloseView = new Edi.ViewModel.Base.DialogViewModelBase();

      // Attach delegate method to validate user input on OK
      // Not setting this means that user input is never validated and view will always close on OK
      if (this.mOpenCloseView != null)
        this.mOpenCloseView.EvaluateInputData = this.ValidateData;
    }

    /// <summary>
    /// Delegate method that is call whenever a user OK'es or Cancels the view that is bound to <seealso cref="OpenCloseView"/>
    /// </summary>
    /// <param name="listMsgs"></param>
    /// <returns></returns>
    private bool ValidateData(out List<EdiViews.Msg> listMsgs)
    {
      bool Error = true;
      listMsgs = new List<EdiViews.Msg>();

      try
      {
        int iNumber = 0;
        try
        {
          iNumber = int.Parse(this.mLineNumberInput);
        }
        catch
        {
          listMsgs.Add(new EdiViews.Msg(string.Format(CultureInfo.CurrentCulture, "The entered number '{0}' is not valid. Enter a valid number.", this.mLineNumberInput),
                                        EdiViews.Msg.MsgCategory.Error));

          Error = !(listMsgs.Count > 0);
          return Error ;
        }

        if (iNumber < this.mMin || iNumber > this.mMax)
          listMsgs.Add(new EdiViews.Msg(string.Format(CultureInfo.CurrentCulture, "The entered number '{0}' is not within expected range ({1} - {2}).", iNumber, this.mMin, this.mMax),
                                        EdiViews.Msg.MsgCategory.Error));

        Error = !(listMsgs.Count > 0);
      }
      finally
      {
        if (Error == false)                          // Select complete text on error such that user can re-type string from scratch
          this.SelectedText = this.LineNumberInput;
      }

      return Error;
    }
    #endregion methods
  }
}
