namespace Edi.View.Dialogs.Config
{
  using System;
  using System.IO;
  using System.Xml;
  using System.Xml.Serialization;

  using Edi.Dialogs.Config;
  using Edi.ViewModel.Base;
  using SimpleControls.MRU.ViewModel;

  [Serializable]
  [XmlRoot(ElementName = "ProgramSettings", IsNullable = false)]
  public class ConfigViewModel : ViewModelBase
  {
    private MRUListVM mMruList;
    private Themes.ThemesVM.EnTheme mCurrentTheme = Themes.ThemesVM.EnTheme.Aero;

    #region constructor
    public ConfigViewModel()
    {
      this.DocumentZoomView = 100;
      this.ReloadOpenFilesOnAppStart = true;

      // Session Data
      this.MainWindowPosSz = new ViewPosSzViewModel();
      this.MainWindowPosSz = new ViewPosSzViewModel(100,100, 1000, 700);

      this.LastActiveFile = string.Empty;

      // Construct MRUListVM ViewModel with parameter to decide whether pinned entries
      // are sorted into the first (pinned list) spot or not (favourites list)
      this.mMruList = new MRUListVM(MRUSortMethod.PinnedEntriesFirst);
    }
    #endregion constructor

    #region properties
    /// <summary>
    /// Percentage Size of data to be viewed by default
    /// </summary>
    [XmlAttribute(AttributeName = "DocumentZoomView")]
    public int DocumentZoomView { get; set; }

    /// <summary>
    /// Get/set whether application re-loads files open in last sesssion or not
    /// </summary>
    [XmlAttribute(AttributeName = "ReloadOpenFilesFromLastSession")]
    public bool ReloadOpenFilesOnAppStart { get; set; }

    #region Session Data
    /// <summary>
    /// Get/set position and size of MainWindow
    /// </summary>
    [XmlElement(ElementName = "MainWindowPos")]
    public ViewPosSzViewModel MainWindowPosSz { get; set; }

    /// <summary>
    /// Remember the last active path and name of last active document.
    /// 
    /// This can be useful when selecting active document in next session or
    /// determining a useful default path when there is no document currently open.
    /// </summary>
    [XmlAttribute(AttributeName = "LastActiveFile")]
    public string LastActiveFile { get; set; }

    #region theming
    /// <summary>
    /// Get/set WPF theme for the complete Application
    /// </summary>
    [XmlAttribute(AttributeName = "CurrentTheme")]
    public Themes.ThemesVM.EnTheme CurrentTheme
    {
      get
      {
        return this.mCurrentTheme;
      }

      set
      {
        if (this.mCurrentTheme != value)
        {
          this.mCurrentTheme = value;    // OnPropertChanged is part of this call

          if (ThemeChanged != null)
            ThemeChanged(this, EventArgs.Empty);
        }
      }
    }

    [XmlIgnore]
    public EventHandler ThemeChanged = null;
    #endregion theming

    /// <summary>
    /// List of most recently used files
    /// </summary>
    public MRUListVM MruList
    {
      get
      {
        if (this.mMruList == null)
          this.mMruList = new MRUListVM();

        return this.mMruList;
      }

      set
      {
        if (this.mMruList != value)
        {
          this.mMruList = value;
          this.NotifyPropertyChanged(() => this.MruList);
        }
      }
    }
    #endregion Session Data
    #endregion properties

    #region methods
    /// <summary>
    /// Get the path of the file or empty string if file does not exists on disk.
    /// </summary>
    /// <returns></returns>
    internal string GetLastActivePath()
    {
      try
      {
        if (System.IO.File.Exists(this.LastActiveFile))
          return System.IO.Path.GetDirectoryName(this.LastActiveFile);
      }
      catch
      {
      }

      return string.Empty;
    }

    /// <summary>
    /// Determine whether program options are valid and corrext
    /// settings if they appear to be invalid on current system
    /// </summary>
    internal void CheckSettingsOnLoad()
    {
      if (MainWindowPosSz == null)
        MainWindowPosSz = new ViewPosSzViewModel(100,100,600, 500);

      if (MainWindowPosSz.DefaultConstruct == true)
        MainWindowPosSz = new ViewPosSzViewModel(100, 100, 600, 500);

      MainWindowPosSz.SetValidPos();
    }

    #region Load Save ProgramOptions ViewModel
    /// <summary>
    /// Save program options into persistence.
    /// See <seealso cref="SaveOptions"/> to save program options on program end.
    /// </summary>
    /// <param name="settingsFileName"></param>
    /// <returns></returns>
    public static ConfigViewModel LoadOptions(string settingsFileName)
    {
      ConfigViewModel loadedViewModel = null;

      if (System.IO.File.Exists(settingsFileName))
      {
        // Create a new file stream for reading the XML file
        using (FileStream readFileStream = new System.IO.FileStream(settingsFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
          try
          {
            // Create a new XmlSerializer instance with the type of the test class
            XmlSerializer serializerObj = new XmlSerializer(typeof(ConfigViewModel));

            // Load the object saved above by using the Deserialize function
            loadedViewModel = (ConfigViewModel)serializerObj.Deserialize(readFileStream);
          }
          catch (Exception e)
          {
            Console.WriteLine(e.ToString());

            loadedViewModel = new ConfigViewModel();  // Just get the defaults if serilization wasn't working here...
          }

          // Cleanup
          readFileStream.Close();
        }
      }

      return loadedViewModel;
    }

    /// <summary>
    /// Save program options into persistence.
    /// See <seealso cref="LoadOptions"/> to load program options on program start.
    /// </summary>
    /// <param name="settingsFileName"></param>
    /// <param name="vm"></param>
    /// <returns></returns>
    public static bool SaveOptions(string settingsFileName, ConfigViewModel vm)
    {
      try
      {
        XmlWriterSettings xws = new XmlWriterSettings();
        xws.NewLineOnAttributes = true;
        xws.Indent = true;
        xws.IndentChars = "  ";
        xws.Encoding = System.Text.Encoding.UTF8;

        // Create a new file stream to write the serialized object to a file
        using (XmlWriter xw = XmlWriter.Create(settingsFileName, xws))
        {
          // Create a new XmlSerializer instance with the type of the test class
          XmlSerializer serializerObj = new XmlSerializer(typeof(ConfigViewModel));

          serializerObj.Serialize(xw, vm);

          xw.Close(); // Cleanup

          return true;
        }
      }
      catch
      {
        throw;
      }
    }
    #endregion Load Save ProgramOptions ViewModel
    #endregion methods
  }
}
