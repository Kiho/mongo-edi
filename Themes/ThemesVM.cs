namespace Themes
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Reflection;
  using System.Windows;
  using System.Globalization;

  /// <summary>
  /// Theme (application skin) viewModel for Exclusive Test enumeration.
  /// That is, each theme in the application is associated with one instance
  /// of the <seealso cref="ThemeVM"/> class. But only one object of that
  /// class has <seealso cref="ThemeVM.IsChecked"/> == true at a time.
  /// </summary>
  public class ThemeVM
  {
    private ThemesVM mParent = null;
    private string mText = string.Empty;

    /// <summary>
    /// Constructor from parent reference to <seealso cref="ThemesVM"/>
    /// collection and <see cref="ThemesVM.EnTheme"/> enumeration entry.
    /// </summary>
    /// <param name="parentObject"></param>
    /// <param name="displayText"></param>
    /// <param name="t"></param>
    public ThemeVM(ThemesVM parentObject, ThemesVM.EnTheme t, string displayText)
    {
      this.mParent = parentObject;
      this.ThemeID = t;
      this.mText = displayText;
    }

    private ThemeVM()
    {
    }

    #region Properties
    /// <summary>
    /// Get/set enumeration type for this theme
    /// </summary>
    public ThemesVM.EnTheme ThemeID { get; set; }

    /// <summary>
    /// Get human-readable name of this theme
    /// </summary>
    public string Text
    {
      get
      {
        return this.mText;
      }
    }

    /// <summary>
    /// Determine whether this theme is currently selected or not.
    /// 
    /// This is can be used when binding to a collection of themes
    /// and trying to determine whether this theme shall be displayed
    /// with check mark or not.
    /// </summary>
    public bool IsChecked
    {
      get
      {
        return this.ThemeID == this.mParent.CurrentTheme;
      }

      set
      {
        if (value == true)
          this.mParent.CurrentTheme = this.ThemeID;
      }
    }
    #endregion Properties
  }

  /// <summary>
  /// Manage a collection of themes at runtime and
  /// determine the theme that is currently in use
  /// </summary>
  public class ThemesVM
  {
    #region Fields
    /// <summary>
    /// Array of resource definition files for each application theme
    /// 
    /// <seealso cref="EnTheme"/>
    /// </summary>
    private static readonly string[] ThemeNames =
    {
      "Aero",
      "VS2010",
      "Generic",
      "Expression Dark"
    };

    private ThemesVM.EnTheme mCurrentTheme = ThemesVM.EnTheme.Aero;
    #endregion Fields

    #region Constructor
    /// <summary>
    /// Default constructor for collection of themes (application skins).
    /// </summary>
    public ThemesVM()
    {
      this.Themes = ThemesVM.ListAllThemes(this);

      this.mCurrentTheme = EnTheme.Aero;    // Just select any theme as default
    }

    /// <summary>
    /// Default constructor for collection of themes (application skins)
    /// from <seealso cref="ThemesVM.EnTheme"/> enumeration entry as theme
    /// to initialize to.
    /// </summary>
    /// <param name="thisCurrentTheme"></param>
    public ThemesVM(EnTheme thisCurrentTheme)
    {
      this.Themes = ThemesVM.ListAllThemes(this);

      this.mCurrentTheme = thisCurrentTheme;    // Construct with this theme as current theme
    }
    #endregion Constructor

    #region enums
    /// <summary>
    /// Enumeration for supported application themes
    /// 
    /// These enumerations AND their indexes
    /// must be consistent with the <seealso cref="ThemeNames"/> array.
    /// </summary>
    public enum EnTheme
    {
      /// <summary>
      /// VS2010 theme selector
      /// </summary>
      Aero = 0,

      /// <summary>
      /// VS2010 theme selector
      /// </summary>
      VS2010 = 1,

      /// <summary>
      /// Generic theme selector
      /// </summary>
      Generic = 2,

      /// <summary>
      /// ExpressionDark theme selector
      /// </summary>
      ExpressionDark = 3
    }
    #endregion enums

    #region properties
    /// <summary>
    /// Get/set WPF theme for the complete Application
    /// </summary>
    public ThemesVM.EnTheme CurrentTheme
    {
      get
      {
        return this.mCurrentTheme;
      }

      set
      {
        this.mCurrentTheme = value;
      }
    }

    /// <summary>
    /// Instances of themes collection managed in this object. Menus or other
    /// GUI controls can bind to this collection
    /// to detect/change the currently selected theme.
    /// </summary>
    public ObservableCollection<ThemeVM> Themes
    {
      get;
      private set;
    }
    #endregion properties

    #region methods
    /// <summary>
    /// Map the name of a theme to its enumeration type
    /// </summary>
    /// <param name="sThemeName"></param>
    /// <returns></returns>
    public static ThemesVM.EnTheme MapNameToEnum(string sThemeName)
    {
      if (sThemeName != null)                 // Select theme by name if one was given
      {                                      // Check if requested theme is available

        foreach (ThemesVM.EnTheme t in Enum.GetValues(typeof(ThemesVM.EnTheme)))
        {
          if (t.ToString().ToLower() == sThemeName.ToLower())
          {
            return t;
          }
        }
      }

      throw new NotImplementedException((sThemeName == null ? "(null)" : sThemeName));
    }

    /// <summary>
    /// Load a resource dictionary from a theming component
    /// </summary>
    /// <param name="theme"></param>
    /// <param name="componentName"></param>
    /// <returns></returns>
    public static ResourceDictionary GetThemeResourceDictionary(string theme, string componentName = "Edi.exe")
    {
      if (theme != null)
      {
        Assembly assembly = Assembly.LoadFrom(componentName);
        string packUri = string.Format(CultureInfo.CurrentCulture, @"/Edi;component/Themes/{0}/Theme.xaml", theme);

        return Application.LoadComponent(new Uri(packUri, UriKind.Relative)) as ResourceDictionary;
      }

      return null;
    }

    /// <summary>
    /// List all themes available in this component
    /// </summary>
    /// <returns></returns>
    private static ObservableCollection<ThemeVM> ListAllThemes(ThemesVM themes)
    {
      // Sort themes by their name before constructing collection of themes
      System.Collections.Generic.SortedDictionary<string, ThemeVM> s = new SortedDictionary<string, ThemeVM>();

      foreach (object t in Enum.GetValues(typeof(ThemesVM.EnTheme)))
      {
        s.Add(ThemesVM.ThemeNames[(int)t], new ThemeVM(themes, (ThemesVM.EnTheme)t, ThemesVM.ThemeNames[(int)t]));
      }

      ObservableCollection<ThemeVM> allThemes = new ObservableCollection<ThemeVM>();
      foreach (KeyValuePair<string, ThemeVM> t in s)
      {
        allThemes.Add(t.Value);
      }

      return allThemes;
    }
    #endregion methods
  }
}
