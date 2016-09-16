namespace Edi.ViewModel
{
  using System;
  using System.Threading;
  using System.Windows;
  using System.Windows.Input;
  using System.Windows.Threading;

  using Edi.ViewModel.Base;
  using Themes;
  using MsgBox;

  internal partial class Workspace : ViewModelBase
  {
    private bool Closing_CanExecute()
    {
      if (this.mShutDownInProgress == true)
        return false;

      // Check if conditions within the WorkspaceViewModel are suitable to close the application
      // eg.: Prompt to Cancel long running background tasks such as Search - Replace in Files (if any)

      return true;
    }

    /// <summary>
    /// Bind a window to some commands to be executed by the viewmodel.
    /// </summary>
    /// <param name="win"></param>
    public void InitCommandBinding(Window win)
    {
      this.InitEditCommandBinding(win);

      win.CommandBindings.Add(new CommandBinding(AppCommand.Exit,
      (s, e) =>
      {
        this.AppExit_CommandExecuted();
      }));

      // Standard File New command binding via ApplicationCommands enumeration
      win.CommandBindings.Add(new CommandBinding(ApplicationCommands.New,
      (s, e) =>
      {
        if (e != null)
          e.Handled = true;

        this.OnNew();
      },
      (s, e) =>
      {
        e.CanExecute = true;
      }
      ));

      // Standard File Open command binding via ApplicationCommands enumeration
      win.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open,
      (s, e) =>
      {
        if (e != null)
          e.Handled = true;

        this.OnOpen();
      },
      (s, e) =>
      {
        e.CanExecute = true;
      }));

      // Close Document command
      // Closes the FileViewModel document supplied in e.parameter
      // or the Active document
      win.CommandBindings.Add(new CommandBinding(AppCommand.CloseFile,
      (s, e) =>
      {
        try
        {
          EdiViewModel f = null;

          if (e != null)
          {
            e.Handled = true;
            f = e.Parameter as EdiViewModel;
          }

          if (f != null)
            this.Close(f);
          else
          {
            if (this.ActiveDocument != null)
              this.Close(this.ActiveDocument);
          }
        }
        catch (Exception exp)
        {
          logger.ErrorException(exp.Message, exp);
          Edi.Msg.Box.Show(exp, App.IssueTrackerText, MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                           App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
        }
      },
      (s, e) =>
      {
        try
        {
          if (e != null)
          {
            e.Handled = true;
            e.CanExecute = false;

            EdiViewModel f = null;

            if (e != null)
            {
              e.Handled = true;
              f = e.Parameter as EdiViewModel;
            }

            if (f != null)
              e.CanExecute = f.CanClose();
            else
            {
              if (this.ActiveDocument != null)
                e.CanExecute = this.ActiveDocument.CanClose();
            }
          }
        }
        catch (Exception exp)
        {
          logger.ErrorException(exp.Message, exp);
          Edi.Msg.Box.Show(exp, App.IssueTrackerText, MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                           App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
        }
      }));


      win.CommandBindings.Add(new CommandBinding(AppCommand.ViewTheme,
                          (s, e) => this.ChangeThemeCmd_Executed(s, e, win.Dispatcher)));

      win.CommandBindings.Add(new CommandBinding(AppCommand.LoadFile,
      (s, e) =>
      {
        try
        {
          if (e == null)
            return;

          string filename = e.Parameter as string;

          if (filename == null)
            return;

          this.Open(filename);
        }
        catch (Exception exp)
        {
          logger.ErrorException(exp.Message, exp);
          Edi.Msg.Box.Show(exp, App.IssueTrackerText, MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                           App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
        }
      }));

      win.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save,
      (s, e) =>
      {
        try
        {
          if (e != null)
            e.Handled = true;

          if (this.ActiveDocument != null)
            this.OnSave(this.ActiveDocument, false);
        }
        catch (Exception exp)
        {
          logger.ErrorException(exp.Message, exp);
          Edi.Msg.Box.Show(exp, "Unhandled Error", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                           App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
        }
      },
      (s, e) =>
      {
        if (e != null)
        {
          e.Handled = true;

          if (this.ActiveDocument != null)
            e.CanExecute = this.ActiveDocument.CanSave();
        }
      }));

      win.CommandBindings.Add(new CommandBinding(ApplicationCommands.SaveAs,
      (s, e) =>
      {
        try
        {
          if (e != null)
            e.Handled = true;

          if (this.ActiveDocument != null)
          {
            if (this.ActiveDocument.OnSaveAs() == true)
              this.Config.MruList.AddMRUEntry(this.ActiveDocument.FilePath);
          }
        }
        catch (Exception exp)
        {
          logger.ErrorException(exp.Message, exp);
          Edi.Msg.Box.Show(exp, App.IssueTrackerText, MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                           App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
        }
      },
      (s, e) =>
      {
        try
        {
          if (e != null)
          {
            e.Handled = true;
            e.CanExecute = false;

            if (this.ActiveDocument != null)
              e.CanExecute = this.ActiveDocument.CanSaveAs();
          }
        }
        catch (Exception exp)
        {
          logger.ErrorException(exp.Message, exp);
          Edi.Msg.Box.Show(exp, App.IssueTrackerText, MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                           App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
        }
      }
      ));

      // Execute a command to save all edited files and current program settings
      win.CommandBindings.Add(new CommandBinding(AppCommand.SaveAll,
      (s, e) =>
      {
        try
        {
          // Save all edited documents
          if (this.mFiles != null)               // Close all open files and make sure there are no unsaved edits
          {                                     // If there are any: Ask user if edits should be saved
            EdiViewModel activeDoc = this.ActiveDocument;

            try
            {
              for (int i = 0; i < this.mFiles.Count; i++)
              {
                EdiViewModel f = this.mFiles[i];

                if (f != null)
                {
                  if (f.IsDirty == true)
                  {
                    this.ActiveDocument = f;
                    this.OnSave(f);
                  }
                }
              }
            }
            catch (Exception exp)
            {
              Edi.Msg.Box.Show(exp.ToString(), "An error occurred", MsgBoxButtons.OK);
            }
            finally
            {
              if (activeDoc != null)
                this.ActiveDocument = activeDoc;
            }
          }

          // Save program settings
          this.SaveConfigOnAppClosed();
        }
        catch (Exception exp)
        {
          logger.ErrorException(exp.Message, exp);
          Edi.Msg.Box.Show(exp, App.IssueTrackerText, MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                           App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
        }
      }));

      win.CommandBindings.Add(new CommandBinding(AppCommand.PinUnpin,
      (s, e) =>
      {
        this.PinCommand_Executed(e.Parameter, e);
      }));

      win.CommandBindings.Add(new CommandBinding(AppCommand.RemoveMruEntry,
      (s, e) =>
      {
        this.RemoveMRUEntry_Executed(e.Parameter, e);
      }));

      win.CommandBindings.Add(new CommandBinding(AppCommand.AddMruEntry,
      (s, e) =>
      {
        this.AddMRUEntry_Executed(e.Parameter, e);
      }));

      win.CommandBindings.Add(new CommandBinding(AppCommand.AddMruEntry,
        (s, e) =>
        {
            try
            {
                if (e != null)
                    e.Handled = true;

                if (this.ActiveDocument != null)
                    this.OnSave(this.ActiveDocument, false);
            }
            catch (Exception exp)
            {
                logger.ErrorException(exp.Message, exp);
                Edi.Msg.Box.Show(exp, "Unhandled Error", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                                 App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
            }
        },
        (s, e) =>
        {
            if (e != null)
            {
                e.Handled = true;

                if (this.ActiveDocument != null)
                    e.CanExecute = this.ActiveDocument.CanSave();
            }
        }));
    }

    private void ChangeThemeCmd_Executed(object s, ExecutedRoutedEventArgs e, System.Windows.Threading.Dispatcher disp)
    {
      ThemesVM.EnTheme oldTheme = ThemesVM.EnTheme.Generic;

      try
      {
        if (e == null)
        return;

        if (e.Parameter == null)
          return;

        // Check if request is available
        if (e.Parameter is Themes.ThemesVM.EnTheme == false)
          return;

        Themes.ThemesVM.EnTheme t = (Themes.ThemesVM.EnTheme)e.Parameter;

        oldTheme = this.Config.CurrentTheme;

        // The Work to perform on another thread
        ThreadStart start = delegate
        {
          // This works in the UI tread using the dispatcher with highest Priority
          disp.Invoke(DispatcherPriority.Send,
          (Action)(() =>
          {
            try
            {

              this.Config.CurrentTheme = t;
            }
            catch (Exception exp)
            {
              logger.ErrorException(exp.Message, exp);
              Edi.Msg.Box.Show(exp, App.IssueTrackerText, MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                               App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
            }
          }));
        };

        // Create the thread and kick it started!
        Thread thread = new Thread(start);

        thread.Start();
      }
      catch (Exception exp)
      {
        this.Config.CurrentTheme = oldTheme;

        logger.ErrorException(exp.Message, exp);
        Edi.Msg.Box.Show(exp, App.IssueTrackerText, MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                         App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
      }
    }

    #region Editor Cut/Copy/Paste/Delete/WordWrap/ShowLineNumbers/ShowEndOfLine Commands
    /// <summary>
    /// Set command bindings necessary to perform copy/cut/paste operations
    /// </summary>
    /// <param name="win"></param>
    public void InitEditCommandBinding(Window win)
    {
/***
      win.CommandBindings.Add(new CommandBinding(AppCommand.CutItem,
      (s, e) =>
      {
        PerformTextAreaEdit(e, TextAreaEdit.Cut);
      },
      (s, e) => { CanPerformTextAreaEdit(e, TextAreaEdit.Cut); }));

      win.CommandBindings.Add(new CommandBinding(AppCommand.CopyItem,
      (s, e) =>
      {
        PerformTextAreaEdit(e, TextAreaEdit.Copy);
      },
      (s, e) =>
      { CanPerformTextAreaEdit(e, TextAreaEdit.Copy); }));

      win.CommandBindings.Add(new CommandBinding(AppCommand.PasteItem,
      (s, e) =>
      {
        PerformTextAreaEdit(e, TextAreaEdit.Paste);
      },
      (s, e) => { CanPerformTextAreaEdit(e, TextAreaEdit.Paste); }));

      win.CommandBindings.Add(new CommandBinding(AppCommand.DeleteItem,
      (s, e) =>
      {
        PerformTextAreaEdit(e, TextAreaEdit.Delete);
      },
      (s, e) => { CanPerformTextAreaEdit(e, TextAreaEdit.Delete); }));
***/
      win.CommandBindings.Add(new CommandBinding(AppCommand.WordWrap,    // Toggle state command,
      (s, e) =>
      {
        try
        {
          if (e != null)
          {
            EdiViewModel f = this.ActiveDocument as EdiViewModel;

            if (f != null)
              f.WordWrap = !f.WordWrap;
          }
        }
        catch (Exception exp)
        {
          logger.ErrorException(exp.Message, exp);
          Edi.Msg.Box.Show(exp, App.IssueTrackerText, MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                           App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
        }
      },
      (s, e) => { e.CanExecute = CanExecuteIfActiveDocumentIsFileViewModel(); }));

      win.CommandBindings.Add(new CommandBinding(AppCommand.ShowLineNumbers,    // Toggle state command,
      (s, e) =>
      {
        try
        {
          if (e != null)
          {
            EdiViewModel f = this.ActiveDocument as EdiViewModel;

            if (f != null)
              f.ShowLineNumbers = !f.ShowLineNumbers;
          }
        }
        catch (Exception exp)
        {
          logger.ErrorException(exp.Message, exp);
          Edi.Msg.Box.Show(exp, App.IssueTrackerText, MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                           App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
        }
      },
      (s, e) =>{ e.CanExecute = CanExecuteIfActiveDocumentIsFileViewModel(); }));

      win.CommandBindings.Add(new CommandBinding(AppCommand.ShowEndOfLine,    // Toggle state command,
      (s, e) =>
      {
        try
        {
          if (e != null)
          {
            EdiViewModel f = this.ActiveDocument as EdiViewModel;

            if (f != null)
              f.ShowEndOfLine = !f.ShowEndOfLine;
          }
        }
        catch (Exception exp)
        {
          logger.ErrorException(exp.Message, exp);
          Edi.Msg.Box.Show(exp, App.IssueTrackerText, MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                           App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
        }
      },
      (s, e) => { e.CanExecute = CanExecuteIfActiveDocumentIsFileViewModel(); }));

      win.CommandBindings.Add(new CommandBinding(AppCommand.ShowTabs,    // Toggle state command,
      (s, e) =>
      {
        try
        {
          if (e != null)
          {
            EdiViewModel f = this.ActiveDocument as EdiViewModel;

            if (f != null)
              f.ShowTabs = !f.ShowTabs;
          }
        }
        catch (Exception exp)
        {
          logger.ErrorException(exp.Message, exp);
          Edi.Msg.Box.Show(exp, App.IssueTrackerText, MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                           App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
        }
      },
      (s, e) => { e.CanExecute = CanExecuteIfActiveDocumentIsFileViewModel(); }));

      win.CommandBindings.Add(new CommandBinding(AppCommand.ShowSpaces,    // Toggle state command,
      (s, e) =>
      {
        try
        {
          if (e != null)
          {
            EdiViewModel f = this.ActiveDocument as EdiViewModel;

            if (f != null)
              f.ShowSpaces = !f.ShowSpaces;
          }
        }
        catch (Exception exp)
        {
          logger.ErrorException(exp.Message, exp);
          Edi.Msg.Box.Show(exp, App.IssueTrackerText, MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                           App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
        }
      },
      (s, e) => { e.CanExecute = CanExecuteIfActiveDocumentIsFileViewModel(); }));

      win.CommandBindings.Add(new CommandBinding(AppCommand.SelectAll,    // Select all text in a document
      (s, e) =>
      {
        try
        {
          EdiViewModel f = this.ActiveDocument as EdiViewModel;

          if (f != null)
            f.TxtControl.SelectAllText();
        }
        catch (Exception exp)
        {
          logger.ErrorException(exp.Message, exp);
          Edi.Msg.Box.Show(exp, App.IssueTrackerText, MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                           App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
        }
      },
      (s, e) => { e.CanExecute = CanExecuteIfActiveDocumentIsFileViewModel(); }));

/****
    /// <summary>
    /// TextArea edit function to be performed on a <seealso cref="ICSharpCode.AvalonEdit.Editing.TextArea"/> view object.
    /// </summary>
    private enum TextAreaEdit
    {
      Copy=0,
      Cut=1,
      Paste=2,
      Delete=3
    }

    /// <summary>
    /// Perform an edit function (Copy, Pase, Cut, Delete) on an
    /// <seealso cref="ICSharpCode.AvalonEdit.Editing.TextArea"/> view object.
    /// </summary>
    /// <param name="e"></param>
    /// <param name="performEdit"></param>
    private void PerformTextAreaEdit(ExecutedRoutedEventArgs e, TextAreaEdit performEdit)
    {
      if (e == null)
        return;

      DockingManager dockManager = e.Parameter as DockingManager;

      if (dockManager == null)
        return;

      // Some AvalonEdit functions (Cut, Copy, Paste etc) require finding the View and
      // Executing code directly on the view.
      //
      // I am not sure how to find the currently used AvalonEdit TextArea view from here:
      // Question: Does dockManager have an API to return the currently active visual?
      //
      // Using VisualTreeHelper on dockManager does work if the document is docked
      //
      // Issues: Undock your text document and this does no longer work (What should we do instead?):
      ICSharpCode.AvalonEdit.Editing.TextArea textArea = Helpers.FindFocusedElement(dockManager) as ICSharpCode.AvalonEdit.Editing.TextArea;

      if (textArea == null)
        return;

      switch (performEdit)
      {
        case TextAreaEdit.Copy:
          ICSharpCode.AvalonEdit.Editing.EditingCommandHandler.OnCopy(textArea, e);
          break;
        case TextAreaEdit.Cut:
          ICSharpCode.AvalonEdit.Editing.EditingCommandHandler.OnCut(textArea, e);
          break;
        case TextAreaEdit.Paste:
          ICSharpCode.AvalonEdit.Editing.EditingCommandHandler.OnPaste(textArea, e);
          break;
        case TextAreaEdit.Delete:
          ICSharpCode.AvalonEdit.Editing.EditingCommandHandler.OnDelete(textArea, e);
          break;

        default:
          throw new NotImplementedException(performEdit.ToString());
      }
    }

    /// <summary>
    /// Determine whether an edit function (Copy, Pase, Cut, Delete) on an
    /// <seealso cref="ICSharpCode.AvalonEdit.Editing.TextArea"/> view object
    /// can be performed or not.
    /// </summary>
    /// <param name="e"></param>
    /// <param name="performEdit"></param>
    private void CanPerformTextAreaEdit(CanExecuteRoutedEventArgs e, TextAreaEdit performEdit)
    {
      if (e == null)
        return;

      DockingManager dockManager = e.Parameter as DockingManager;

      if (dockManager == null)
        return;

      // Some AvalonEdit functions (Cut, Copy, Paste etc) require finding the View and
      // Executing code directly on the view.
      //
      // I am not sure how to find the currently used AvalonEdit TextArea view from here:
      // Question: Does dockManager have an API to return the currently active visual?
      //
      // Using VisualTreeHelper on dockManager does work if the document is docked
      //
      // Issues: Undock your text document and this does no longer work (What should we do instead?):
      ICSharpCode.AvalonEdit.Editing.TextArea textArea = Helpers.FindFocusedElement(dockManager) as ICSharpCode.AvalonEdit.Editing.TextArea;

      if (textArea == null)
        return;

      switch (performEdit)
      {
        case TextAreaEdit.Copy:
        case TextAreaEdit.Cut:
          ICSharpCode.AvalonEdit.Editing.EditingCommandHandler.CanCutOrCopy(textArea, e);
          break;
        case TextAreaEdit.Paste:
          ICSharpCode.AvalonEdit.Editing.EditingCommandHandler.CanPaste(textArea, e);
          break;
        case TextAreaEdit.Delete:
          ICSharpCode.AvalonEdit.Editing.EditingCommandHandler.CanDelete(textArea, e);
          break;

        default:
          throw new NotImplementedException(performEdit.ToString());
      }
    }
***/
    #endregion Editor Cut/Copy/Paste/Delete/WordWrap/ShowLineNumbers/ShowEndOfLine Commands

    #region GotoLine FindReplace
      win.CommandBindings.Add(new CommandBinding(AppCommand.GotoLine,    // Goto line n in a document
      (s, e) =>
      {
        try
        {
          e.Handled = true;

          this.ShowGotoLineDialog();
        }
        catch (Exception exp)
        {
          logger.ErrorException(exp.Message, exp);
          Edi.Msg.Box.Show(exp, App.IssueTrackerText, MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                           App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
        }
      },
      (s, e) => { e.CanExecute = CanExecuteIfActiveDocumentIsFileViewModel(); }));

      win.CommandBindings.Add(new CommandBinding(AppCommand.FindText,    // Find text in a document
      (s, e) =>
      {
        try
        {
          e.Handled = true;

          this.ShowFindReplaceDialog();
        }
        catch (Exception exp)
        {
          logger.ErrorException(exp.Message, exp);
          Edi.Msg.Box.Show(exp, App.IssueTrackerText, MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                           App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
        }
      },
      (s, e) => { e.CanExecute = CanExecuteIfActiveDocumentIsFileViewModel(); }));

      win.CommandBindings.Add(new CommandBinding(AppCommand.FindPreviousText,    // Find text in a document
      (s, e) =>
      {
        try
        {
          e.Handled = true;

          EdiViewModel f = this.ActiveDocument as EdiViewModel;

          if (f != null)
          {
            if (this.FindReplaceVM != null)
            {
              this.FindReplaceVM.FindNext(this.FindReplaceVM, true);
            }
            else
            {
              this.ShowFindReplaceDialog();
            }
          }
        }
        catch (Exception exp)
        {
          logger.ErrorException(exp.Message, exp);
          Edi.Msg.Box.Show(exp, App.IssueTrackerText, MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                           App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
        }
      },
      (s, e) => { e.CanExecute = CanExecuteIfActiveDocumentIsFileViewModel(); }));

      win.CommandBindings.Add(new CommandBinding(AppCommand.FindNextText,    // Find text in a document
      (s, e) =>
      {
        try
        {
          e.Handled = true;

          EdiViewModel f = this.ActiveDocument as EdiViewModel;

          if (f != null)
          {
            if (this.FindReplaceVM != null)
            {
              this.FindReplaceVM.FindNext(this.FindReplaceVM, false);
            }
            else
            {
              this.ShowFindReplaceDialog();
            }
          }
        }
        catch (Exception exp)
        {
          logger.ErrorException(exp.Message, exp);
          Edi.Msg.Box.Show(exp, App.IssueTrackerText, MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                           App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
        }
      },
      (s, e) => { e.CanExecute = CanExecuteIfActiveDocumentIsFileViewModel(); }));

      win.CommandBindings.Add(new CommandBinding(AppCommand.ReplaceText, // Find and replace text in a document
      (s, e) =>
      {
        try
        {
          e.Handled = true;

          this.ShowFindReplaceDialog(false);
        }
        catch (Exception exp)
        {
          logger.ErrorException(exp.Message, exp);
          Edi.Msg.Box.Show(exp, App.IssueTrackerText, MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                           App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
        }
      },
      (s, e) => { e.CanExecute = CanExecuteIfActiveDocumentIsFileViewModel(); }));
    #endregion GotoLine FindReplace

      win.CommandBindings.Add(new CommandBinding(AppCommand.RunScript,
        (s, e) =>
        {
            try
            {
                if (e != null)
                    e.Handled = true;

                if (this.ActiveDocument != null)
                {
                    this.OnSave(this.ActiveDocument, false);
                    this.OnRunScript(this.ActiveDocument, true);
                }                    
            }
            catch (Exception exp)
            {
                logger.ErrorException(exp.Message, exp);
                Edi.Msg.Box.Show(exp, "Unhandled Error", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton,
                                 App.IssueTrackerLink, App.IssueTrackerLink, App.IssueTrackerText, null, true);
            }
        },
        (s, e) =>
        {
            if (e != null)
            {
                e.Handled = true;
                if (this.ActiveDocument != null)
                    e.CanExecute = this.ActiveDocument.CanSave();
            }
        }));
    }

    private bool CanExecuteIfActiveDocumentIsFileViewModel()
    {
      EdiViewModel f = this.ActiveDocument as EdiViewModel;

      if (f != null)
        return true;

      return false;
    }
  }
}
