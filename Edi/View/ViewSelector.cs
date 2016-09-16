namespace Edi.View
{
  using System;
  using System.Windows;
  using Edi.View.Dialogs;

  class ViewSelector
  {
    public static Window GetDialogView(object viewModel)
    {
      if (viewModel == null)
        throw new Exception("The viewModel parameter cannot be null.");

      if (viewModel is Edi.View.Dialogs.GotoLine.GotoLineViewModel)
        return new GotoLineDlg();

      if (viewModel is Edi.View.Dialogs.FindReplace.FindReplaceViewModel)
        return new FindReplaceDialog();

      throw new NotSupportedException(viewModel.GetType().ToString());
    }
  }
}
