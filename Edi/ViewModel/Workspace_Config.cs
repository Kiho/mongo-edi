namespace Edi.ViewModel
{
  using System;
  using Edi.View.Dialogs.Config;
  using MsgBox;

  internal partial class Workspace
  {
    private ConfigViewModel _config = null;

    /// <summary>
    /// Get/set program settings for entire application
    /// </summary>
    public ConfigViewModel Config
    {
      get
      {
        if (_config == null)
          _config = new ConfigViewModel();

        return _config;
      }

      internal set
      {
        if (_config != value)
        {
          _config = value;
          this.NotifyPropertyChanged(() => this.Config);
        }
      }
    }

    /// <summary>
    /// Save application settings when the application is being closed down
    /// </summary>
    public void SaveConfigOnAppClosed()
    {
      try
      {
        App.CreateAppDataFolder();

        // Save/initialize program options that determine global program behaviour
        ConfigViewModel.SaveOptions(App.DirFileAppSessionData, this.Config);
      }
      catch (Exception exp)
      {
        Edi.Msg.Box.Show(exp.ToString(), "Unhandled Exception", MsgBoxButtons.OK);
      }
    }

    /// <summary>
    /// Load configuration from persistence on startup of application
    /// </summary>
    public void LoadConfigOnAppStartup()
    {
      ConfigViewModel retOpts = null;

      try
      {
        // Re/Load program options to control global behaviour of program
        if ((retOpts = ConfigViewModel.LoadOptions(App.DirFileAppSessionData)) == null)
          retOpts = new ConfigViewModel();
      }
      catch
      {
      }
      finally
      {
        if (retOpts == null)
          retOpts = new ConfigViewModel();
      }

      this.Config = retOpts;

      this.Skins = new Themes.ThemesVM(this.Config.CurrentTheme);
      this.ResetTheme(null, null);
      this.Config.ThemeChanged += new EventHandler(this.ResetTheme);
    }
      public MainWindow Win { get; set; }
  }
}
