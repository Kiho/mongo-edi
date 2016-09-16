namespace Edi.View.Dialogs.FindReplace
{
  using SimpleControls.MRU.ViewModel;
  using System.Collections.Generic;
  using Edi.ViewModel.Base;
  using Edi.Command;
  using System.Text.RegularExpressions;
  using System.Windows.Input;
  using System.Windows;
  using System.Collections;
  using System;
  using MsgBox;

  public class FindReplaceViewModel : ViewModelBase
  {
    private RelayCommand mFindCommand;
    private RelayCommand mReplaceCommand;
    private RelayCommand mReplaceAllCommand;

    #region constructor
    public FindReplaceViewModel()
    {
      this.CurrentEditor = null;
    }
    #endregion constructor

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

    private string mTextToFind = string.Empty;
    public string TextToFind
    {
      get
      {
        return this.mTextToFind;
      }

      set
      {
        if (this.mTextToFind != value)
        {
          this.mTextToFind = value;

          this.NotifyPropertyChanged(() => this.TextToFind);
        }
      }
    }

    private string mReplacementText = string.Empty;
    public string ReplacementText
    {
      get
      {
        return this.mReplacementText;
      }

      set
      {
        if (this.mReplacementText != value)
        {
          this.mReplacementText = value;

          this.NotifyPropertyChanged(() => this.ReplacementText);
        }
      }
    }

    private bool mSearchUp = false;
    public bool SearchUp
    {
      get
      {
        return this.mSearchUp;
      }

      set
      {
        if (this.mSearchUp != value)
        {
          this.mSearchUp = value;

          this.NotifyPropertyChanged(() => this.SearchUp);
        }
      }
    }

    private bool mUseWildcards = false;
    public bool UseWildcards
    {
      get
      {
        return this.mUseWildcards;
      }

      set
      {
        if (this.mUseWildcards != value)
        {
          this.mUseWildcards = value;

          this.NotifyPropertyChanged(() => this.UseWildcards);
        }
      }
    }

    private bool mCaseSensitive = false;
    public bool CaseSensitive
    {
      get
      {
        return this.mCaseSensitive;
      }

      set
      {
        if (this.mCaseSensitive != value)
        {
          this.mCaseSensitive = value;

          this.NotifyPropertyChanged(() => this.CaseSensitive);
        }
      }
    }

    private bool mUseRegEx = false;
    public bool UseRegEx
    {
      get
      {
        return this.mUseRegEx;
      }

      set
      {
        if (this.mUseRegEx != value)
        {
          this.mUseRegEx = value;

          this.NotifyPropertyChanged(() => this.UseRegEx);
        }
      }
    }

    private bool mWholeWord = false;
    public bool WholeWord
    {
      get
      {
        return this.mWholeWord;
      }

      set
      {
        if (this.mWholeWord != value)
        {
          this.mWholeWord = value;

          this.NotifyPropertyChanged(() => this.WholeWord);
        }
      }
    }

    private bool mAcceptsReturn = false;
    public bool AcceptsReturn
    {
      get
      {
        return this.mAcceptsReturn;
      }

      set
      {
        if (this.mAcceptsReturn != value)
        {
          this.mAcceptsReturn = value;

          this.NotifyPropertyChanged(() => this.AcceptsReturn);
        }
      }
    }

    private bool mAllowReplace = true;
    public bool AllowReplace
    {
      get
      {
        return this.mAllowReplace;
      }

      set
      {
        if (this.mAllowReplace != value)
        {
          this.mAllowReplace = value;

          this.NotifyPropertyChanged(() => this.AllowReplace);
        }
      }
    }

    private bool mShowAsFind = true;
    /// <summary>
    /// Get/set property to determine whether dialog should show Find UI (true)
    /// or whether it should show Find/Replace UI elements (false).
    /// </summary>
    public bool ShowAsFind
    {
      get
      {
        return this.mShowAsFind;
      }

      set
      {
        if (this.mShowAsFind != value)
        {
          this.mShowAsFind = value;

          if (value == true)            // Focus textbox on find tab or find/replace tab
          {
            this.IsTextToFindFocused = true;
            this.IsTextToFindInReplaceFocused = false;
          }
          else
          {
            this.IsTextToFindFocused = false;
            this.IsTextToFindInReplaceFocused = true;
          }

          this.NotifyPropertyChanged(() => this.ShowAsFind);
        }
      }
    }

    private bool mIsTextToFindFocused = true;
    /// <summary>
    /// </summary>
    public bool IsTextToFindFocused
    {
      get
      {
        return this.mIsTextToFindFocused;
      }

      set
      {
        if (this.mIsTextToFindFocused != value)
        {
          this.mIsTextToFindFocused = value;

          this.NotifyPropertyChanged(() => this.IsTextToFindFocused);
        }
      }
    }

    private bool mIsTextToFindInReplaceFocused = true;
    /// <summary>
    /// </summary>
    public bool IsTextToFindInReplaceFocused
    {
      get
      {
        return this.mIsTextToFindInReplaceFocused;
      }

      set
      {
        if (this.mIsTextToFindInReplaceFocused != value)
        {
          this.mIsTextToFindInReplaceFocused = value;

          this.NotifyPropertyChanged(() => this.IsTextToFindInReplaceFocused);
        }
      }
    }

    private EdiViews.FindReplace.SearchScope mSearchIn = EdiViews.FindReplace.SearchScope.CurrentDocument;
    public EdiViews.FindReplace.SearchScope SearchIn
    {
      get
      {
        return this.mSearchIn;
      }

      set
      {
        if (this.mSearchIn != value)
        {
          this.mSearchIn = value;

          this.NotifyPropertyChanged(() => this.SearchIn);
        }
      }
    }

    /// <summary>
    /// Determines whether to display the Search in choice or not.
    /// 
    /// The user can use this choice to select whether results are search/replaced:
    /// 1> Only in the current document
    /// 2> In all open documents
    /// </summary>
    private bool mShowSearchIn = true;
    public bool ShowSearchIn
    {
      get
      {
        return this.mShowSearchIn;
      }

      private set
      {
        if (this.mShowSearchIn != value)
        {
          this.mShowSearchIn = value;

          this.NotifyPropertyChanged(() => this.ShowSearchIn);
        }
      }
    }

    public IEditor CurrentEditor { get; set; }

    public Func<FindReplaceViewModel, bool, bool> FindNext { get; set; }

    public ICommand FindCommand
    {
      get
      {
        if (this.mFindCommand == null)
          this.mFindCommand = new RelayCommand((p) =>
          {
            if (this.FindNext != null)
              this.FindNext(this, this.mSearchUp);
          });
        
        return this.mFindCommand;
      }
    }

    public ICommand ReplaceCommand
    {
      get
      {
        if (this.mReplaceCommand == null)
          this.mReplaceCommand = new RelayCommand((p) => this.Replace());

        return this.mReplaceCommand;
      }
    }

    public ICommand ReplaceAllCommand
    {
      get
      {
        if (this.mReplaceAllCommand == null)
          this.mReplaceAllCommand = new RelayCommand((p) => this.ReplaceAll());

        return this.mReplaceAllCommand;
      }
    }
    #endregion properties

    #region methods
    /// <summary>
    /// Initialize input states such that user can input information
    /// with a view based GUI (eg.: dialog)
    /// </summary>
    public void InitDialogInputData()
    {
      this.OpenCloseView = new Edi.ViewModel.Base.DialogViewModelBase();
    }

    /// <summary>
    /// Constructs a regular expression according to the currently selected search parameters.
    /// </summary>
    /// <param name="ForceLeftToRight"></param>
    /// <returns>The regular expression.</returns>
    public Regex GetRegEx(bool ForceLeftToRight = false)
    {
      Regex r;
      RegexOptions o = RegexOptions.None;
      if (SearchUp && !ForceLeftToRight)
        o = o | RegexOptions.RightToLeft;
      if (!CaseSensitive)
        o = o | RegexOptions.IgnoreCase;

      if (UseRegEx)
        r = new Regex(TextToFind, o);
      else
      {
        string s = Regex.Escape(TextToFind);
        if (UseWildcards)
          s = s.Replace("\\*", ".*").Replace("\\?", ".");
        if (WholeWord)
          s = "\\W" + s + "\\W";
        r = new Regex(s, o);
      }

      return r;
    }

    public void FindPrevious()
    {
      if (this.FindNext != null)
        this.FindNext(this, true);
    }

    public void Replace()
    {
      IEditor CE = GetCurrentEditor();
      if (CE == null) return;

      // if currently selected text matches -> replace; anyways, find the next match
      Regex r = GetRegEx();
      string s = CE.Text.Substring(CE.SelectionStart, CE.SelectionLength); // CE.SelectedText;
      Match m = r.Match(s);
      if (m.Success && m.Index == 0 && m.Length == s.Length)
      {
        CE.Replace(CE.SelectionStart, CE.SelectionLength, ReplacementText);
        //CE.SelectedText = ReplacementText;
      }

      if (this.FindNext != null)
        this.FindNext(this, false);
    }

    public void ReplaceAll(bool AskBefore = true)
    {
      IEditor CE = GetCurrentEditor();
      if (CE == null) return;

      if (!AskBefore || Edi.Msg.Box.Show("Do you really want to replace all occurences of '" + TextToFind + "' with '" + ReplacementText + "'?",
          "Replace all", MsgBoxButtons.YesNoCancel, MsgBoxImage.Alert) == MsgBoxResult.Yes)
      {
        object InitialEditor = CurrentEditor;
        // loop through all editors, until we are back at the starting editor                
        do
        {
          Regex r = GetRegEx(true);   // force left to right, otherwise indices are screwed up
          int offset = 0;
          CE.BeginChange();
          foreach (Match m in r.Matches(CE.Text))
          {
            CE.Replace(offset + m.Index, m.Length, ReplacementText);
            offset += ReplacementText.Length - m.Length;
          }
          CE.EndChange();

          // XXX TODO CE = GetNextEditor();
        } while (CurrentEditor != InitialEditor);
      }
    }

    public IEditor GetCurrentEditor()
    {
      if (CurrentEditor == null)
        return null;

      if (CurrentEditor is IEditor)
        return CurrentEditor as IEditor;

      ////      if (InterfaceConverter == null)
      ////        return null;
      ////
      ////      return InterfaceConverter.Convert(CurrentEditor, typeof(IEditor), null, CultureInfo.CurrentCulture) as IEditor;

      return null;
    }
    #endregion methods
  }
}
