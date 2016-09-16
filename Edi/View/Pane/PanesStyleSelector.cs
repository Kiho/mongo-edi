namespace Edi.View.Pane
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Windows.Controls;
  using System.Windows;
  using Edi.ViewModel;
  using Edi.ViewModel.Base;

  class PanesStyleSelector : StyleSelector
  {
    public Style ToolStyle
    {
      get;
      set;
    }

    public Style FileStyle
    {
      get;
      set;
    }

    public Style RecentFilesStyle
    {
      get;
      set;
    }

    public override System.Windows.Style SelectStyle(object item, System.Windows.DependencyObject container)
    {
      if (item is ToolViewModel)
        return ToolStyle;

      if (item is EdiViewModel)
        return FileStyle;

      if (item is RecentFilesViewModel)
        return FileStyle;

      return base.SelectStyle(item, container);
    }
  }
}
