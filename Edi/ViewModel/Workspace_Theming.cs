namespace Edi.ViewModel
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Collections.ObjectModel;
  using System.Windows.Input;
  using Microsoft.Win32;
  using System.IO;
  using System.Windows;
  using AvalonDock.Layout;
  using Edi.ViewModel;
  using System.Reflection;
  using MsgBox;
  using System.Globalization;
  using AvalonDock.Themes;
  using System.Windows.Threading;

  partial class Workspace
  {
    #region fields
    #endregion fields

    #region Themes
    /// <summary>
    /// Expose the application theme currently applicable
    /// (see Also themining in <seealso cref="ConfigViewModel"/> )
    /// </summary>
    public Themes.ThemesVM Skins { get; set; }
    #endregion Themes

    /// <summary>
    /// Change WPF theme to theme supplied in <paramref name="themeToSwitchTo"/>
    /// 
    /// This method can be called when the theme is to be reseted by all means
    /// (eg.: when powering application up).
    /// 
    /// !!! Use the <seealso cref="CurrentTheme"/> property to change !!!
    /// !!! the theme when App is running                             !!!
    /// </summary>
    /// <param name="themeToSwitchTo"></param>
    public void ResetTheme(object sender, EventArgs args)
    {
      Themes.ThemesVM.EnTheme themeToSwitchTo = Themes.ThemesVM.EnTheme.Aero;

      if (this.Config != null)
      {
        themeToSwitchTo = this.Config.CurrentTheme;

        this.SwitchToSelectedTheme(null, themeToSwitchTo);
      }
    }

    /// <summary>
    /// Attempt to switch to the theme as stated in <paramref name="sParameter"/>.
    /// The given name must map into the <seealso cref="Themes.ThemesVM.EnTheme"/> enumeration.
    /// </summary>
    /// <param name="sParameter"></param>
    /// <param name="thisTheme"></param>
    private bool SwitchToSelectedTheme(string sParameter = null,
                                       Themes.ThemesVM.EnTheme thisTheme = Themes.ThemesVM.EnTheme.Aero)
    {
      const string themesModul = "Themes.dll";
      
      try
      {
        if (sParameter != null)
          thisTheme = Themes.ThemesVM.MapNameToEnum(sParameter); // Select theme by name if one was given

        this.Skins.CurrentTheme = thisTheme;

        string[] Uris = null;

        switch (thisTheme)
        {
          case Themes.ThemesVM.EnTheme.Aero:
            Uris = new string[2];

            Uris[0] = "/Themes;component/Aero/Theme.xaml";
            Uris[1] = "/AvalonDock.Themes.Aero;component/Theme.xaml";
            break;

          case Themes.ThemesVM.EnTheme.ExpressionDark:
            Uris = new string[3];

            Uris[0] = "/Themes;component/ExpressionDark/Theme.xaml";
            Uris[1] = "/Edi;component/Themes/Expressiondark.xaml";
            Uris[2] = "/AvalonDock.Themes.ExpressionDark;component/Theme.xaml";
            break;

          case Themes.ThemesVM.EnTheme.VS2010:
            Uris = new string[2];

            Uris[0] = "/Themes;component/VS2010/Theme.xaml";
            Uris[1] = "/AvalonDock.Themes.VS2010;component/Theme.xaml";
            break;

          case Themes.ThemesVM.EnTheme.Generic:
            Uris = new string[1];

            Uris[0] = "/Themes;component/Generic/Theme.xaml";
            break;

          default:
            break;
        }

        if (Uris != null)
        {
          Application.Current.Resources.MergedDictionaries.Clear();

          string ThemesPathFileName = Assembly.GetEntryAssembly().Location;

          ThemesPathFileName = System.IO.Path.GetDirectoryName(ThemesPathFileName);
          ThemesPathFileName = System.IO.Path.Combine(ThemesPathFileName, themesModul);
          Assembly assembly = Assembly.LoadFrom(ThemesPathFileName);

          if (System.IO.File.Exists(ThemesPathFileName) == false)
          {
            Edi.Msg.Box.Show(string.Format(CultureInfo.CurrentCulture, "Cannot find Path to: '{0}'\n\n" +
                              "Please make sure this module is accesible.", themesModul), "Error",
                              MsgBoxButtons.OK, MsgBoxImage.Error);

            return false;
          }

          for (int i = 0; i < Uris.Length; i++)
          {
            try
            {
              Uri Res = new Uri(Uris[i], UriKind.Relative);

              ResourceDictionary dictionary = Application.LoadComponent(Res) as ResourceDictionary;

              if (dictionary != null)
                Application.Current.Resources.MergedDictionaries.Add(dictionary);
            }
            catch (Exception Exp)
            {
              Edi.Msg.Box.Show(Exp, string.Format(CultureInfo.CurrentCulture, "'{0}'", Uris[i]), MsgBoxButtons.OK, MsgBoxImage.Error);
            }
          }
        }
      }
      catch (Exception exp)
      {
        Edi.Msg.Box.Show(exp, "Error", MsgBoxButtons.OK, MsgBoxImage.Error);

        return false;
      }

      return true;
    }
  }
}
