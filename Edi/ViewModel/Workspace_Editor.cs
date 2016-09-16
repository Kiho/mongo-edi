namespace Edi.ViewModel
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text.RegularExpressions;
  using System.Windows;
  using System.Windows.Threading;

  using Edi.View;
  using Edi.View.Dialogs.FindReplace;
  using ICSharpCode.AvalonEdit.Document;
  using MsgBox;

  partial class Workspace
  {
    private FindReplaceViewModel mFindReplaceVM = null;
    public FindReplaceViewModel FindReplaceVM
    {
      get
      {
        return this.mFindReplaceVM;
      }
      
      protected set
      {
        if (this.mFindReplaceVM != value)
        {
          this.mFindReplaceVM = value;
          this.NotifyPropertyChanged(() => this.FindReplaceVM);
        }
      }
    }

    private IEditor GetNextEditor(FindReplaceViewModel f,
                                  bool previous = false
                                  )
    {
      // There is no next open document if there is none or only one open
      if (this.Files.Count <= 1)
        return f.GetCurrentEditor();

      // There is no next open document If the user wants to search the current document only
      if (f.SearchIn == EdiViews.FindReplace.SearchScope.CurrentDocument)
        return f.GetCurrentEditor();

      List<object> l = new List<object>(this.Files.Cast<object>());

      int idxStart = l.IndexOf(f.CurrentEditor);
      int i = idxStart;

      if (i >= 0)
      {
        Match m = null;

        bool textSearchSuccess = false;
        do
        {
          if (previous == true)                  // Get next/previous document
            i = (i < 1 ? l.Count - 1 : i - 1);
          else
            i = (i >= l.Count-1 ? 0 : i + 1);

          //// i = (i + (previous ? l.Count - 1 : +1)) % l.Count;

          EdiViewModel fTmp = l[i] as EdiViewModel; // Search text in document
          if (fTmp != null)
          {
            Regex r;
            m = this.FindNextMatchInText(0, 0, false, fTmp.Text, ref f, out r);

            textSearchSuccess = m.Success;
          }
        }
        while (i != idxStart && textSearchSuccess != true);

        // Found a match so activate the corresponding document and select the text with scroll into view
        if (textSearchSuccess == true && m != null)
        {
          this.ActiveDocument = l[i] as EdiViewModel;

          // Ensure that no pending calls are in the dispatcher queue
          // This makes sure that we are blocked until bindings are re-established
          // Bindings are required to scroll a selection into view
          Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.SystemIdle, (Action)delegate
          {
            if (this.ActiveDocument != null)
            {
              this.ActiveDocument.TextEditorSelectionStart = m.Index;
              this.ActiveDocument.TextEditorSelectionLength = m.Length;

              // Reset cursor position to make sure we search a document from its beginning
              this.ActiveDocument.TxtControl.SelectText(m.Index, m.Length);

              f.CurrentEditor = l[i] as IEditor;

              IEditor edi = f.GetCurrentEditor();

              if (edi != null)
                edi.Select(m.Index, m.Length);

            }
          });

          return f.GetCurrentEditor();
        }
      }

      return null;
    }

    /// <summary>
    /// Find a match in a given peace of string
    /// </summary>
    /// <param name="SelectionStart"></param>
    /// <param name="SelectionLength"></param>
    /// <param name="InvertLeftRight"></param>
    /// <param name="Text"></param>
    /// <param name="f"></param>
    /// <returns></returns>
    Match FindNextMatchInText(int SelectionStart,             // CE.SelectionStart
                              int SelectionLength,           // CE.SelectionLength
                              bool InvertLeftRight,         // CE.Text
                              string Text,                 // InvertLeftRight
                              ref FindReplaceViewModel f,
                              out Regex r)
    {
      if (InvertLeftRight)
      {
        f.SearchUp = !f.SearchUp;
        r = f.GetRegEx();
        f.SearchUp = !f.SearchUp;
      }
      else
        r = f.GetRegEx();

      return r.Match(Text, r.Options.HasFlag(RegexOptions.RightToLeft) ? SelectionStart : SelectionStart + SelectionLength);    
    }

    private bool FindNext(FindReplaceViewModel f,
                          bool InvertLeftRight = false)
    {
      IEditor CE = f.GetCurrentEditor();

      if (CE == null)
        return false;

      Regex r;
      Match m = this.FindNextMatchInText(CE.SelectionStart, CE.SelectionLength,
                                         InvertLeftRight, CE.Text, ref f, out r);

      if (m.Success)
      {
        CE.Select(m.Index, m.Length);
        
        return true;
      }
      else
      {
        if (f.SearchIn == EdiViews.FindReplace.SearchScope.CurrentDocument)
        {
          Edi.Msg.Box.Show("There are no more search results to display.");

          return false;
        }

        // we have reached the end of the document
        // start again from the beginning/end,
        object OldEditor = f.CurrentEditor;
        do
        {
          if (f.SearchIn == EdiViews.FindReplace.SearchScope.AllDocuments)
          {
            CE = this.GetNextEditor(f, r.Options.HasFlag(RegexOptions.RightToLeft));
            
            if (CE == null)
              return false;

            f.CurrentEditor = CE;

            return true;
          }

          if (r.Options.HasFlag(RegexOptions.RightToLeft))
            m = r.Match(CE.Text, CE.Text.Length - 1);
          else
            m = r.Match(CE.Text, 0);

          if (m.Success)
          {
            CE.Select(m.Index, m.Length);
            break;
          }
          else
          {
            Edi.Msg.Box.Show("No further occurence found.", "Search");
          }
        } while (f.CurrentEditor != OldEditor);
      }

      return false;
    }

    /// <summary>
    /// Gets the current line in which the cursor is currently located
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    private static int GetCurrentEditorLine(EdiViewModel f)
    {
      int iCurrLine = 0;

      try
      {
        int start, length;
        bool IsRectangularSelection = false;

        f.TxtControl.CurrentSelection(out start, out length, out IsRectangularSelection);

        iCurrLine = f.Document.GetLineByOffset(start).LineNumber;
      }
      catch (Exception)
      {
      }
      return iCurrLine;
    }

    private void ShowGotoLineDialog()
    {
      EdiViewModel f = this.ActiveDocument as EdiViewModel;

      if (f != null)
      {
        Window dlg = null;
        Edi.View.Dialogs.GotoLine.GotoLineViewModel dlgVM = null;

        try
        {
          int iCurrLine = Workspace.GetCurrentEditorLine(f);

          dlgVM = new Edi.View.Dialogs.GotoLine.GotoLineViewModel(1, f.Document.LineCount, iCurrLine);
          dlg = ViewSelector.GetDialogView((object)dlgVM);

          // It is important to either:
          // 1> Use the InitDialogInputData methode here or
          // 2> Reset the WindowCloseResult=null property
          // because otherwise ShowDialog will not work twice
          // (Symptom: The dialog is closed immeditialy by the attached behaviour)
          dlgVM.InitDialogInputData();

          dlg.DataContext = dlgVM;

          dlg.Closing += dlgVM.OpenCloseView.OnClosing;

          dlg.Owner = Application.Current.MainWindow; // Make sure that dialog window appears in front of main window

          dlg.ShowDialog();

          // Copy input if user OK'ed it. This could also be done by a method, equality operator, or copy constructor
          if (((Edi.View.Dialogs.GotoLine.GotoLineViewModel)dlg.DataContext).OpenCloseView.WindowCloseResult == true)
          {
            DocumentLine line = f.Document.GetLineByNumber(dlgVM.LineNumber);

            f.TxtControl.SelectText(line.Offset, 0);      // Select text with length 0 and scroll to where
            f.TxtControl.ScrollToLine(dlgVM.LineNumber); // we are supposed to be at
          }
        }
        catch (Exception exc)
        {
          Edi.Msg.Box.Show(exc, "An unexpected error occured.", MsgBoxButtons.OK, MsgBoxImage.Error);
        }
        finally
        {
          if (dlg != null)
          {
            dlg.Closing -= dlgVM.OpenCloseView.OnClosing;
            dlg.Close();
          }
        }
      }
    }

    private void ShowFindReplaceDialog(bool ShowFind = true)
    {
      EdiViewModel f = this.ActiveDocument as EdiViewModel;

      if (f != null)
      {
        Window dlg = null;

        try
        {
          if (this.FindReplaceVM == null)
          {
            this.FindReplaceVM = new Edi.View.Dialogs.FindReplace.FindReplaceViewModel();
          }

          this.FindReplaceVM.FindNext = this.FindNext;

          // determine whether Find or Find/Replace is to be executed
          this.FindReplaceVM.ShowAsFind = ShowFind;

          if (f.TxtControl != null)      // Search by default for currently selected text (if any)
          {
            string textToFind;
            f.TxtControl.GetSelectedText(out textToFind);

            if (textToFind.Length > 0)
              this.FindReplaceVM.TextToFind = textToFind;
          }

          this.FindReplaceVM.CurrentEditor = f;

          dlg = ViewSelector.GetDialogView((object)this.FindReplaceVM);

          // It is important to either:
          // 1> Use the InitDialogInputData methode here or
          // 2> Reset the WindowCloseResult=null property
          // because otherwise ShowDialog will not work twice
          // (Symptom: The dialog is closed immeditialy by the attached behaviour)
          this.FindReplaceVM.InitDialogInputData();

          dlg.DataContext = this.FindReplaceVM;

          dlg.Closing += this.FindReplaceVM.OpenCloseView.OnClosing;

          dlg.Owner = Application.Current.MainWindow; // Make sure that dialog window appears in front of main window

          dlg.ShowDialog();
        }
        catch (Exception exc)
        {
          Edi.Msg.Box.Show(exc, "An unexpected error occured.", MsgBoxButtons.OK, MsgBoxImage.Error);
        }
        finally
        {
          if (dlg != null)
          {
            dlg.Closing -= this.FindReplaceVM.OpenCloseView.OnClosing;
            dlg.Close();
          }
        }
      }
    }
  }
}
