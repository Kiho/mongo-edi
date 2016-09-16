namespace MsgBox
{
  /// <summary>
  /// Type of images that can be displayed in a message box
  /// (indexes need to increment in the shown order because they
  /// into a static array: <seealso cref="MessageBoxViewModel.MsgBoxImageResourcesUris"/>
  /// </summary>
  public enum MsgBoxImage
  {
    Information = 0,
    Question = 1,
    Error = 2,
    OK = 3,
    Alert = 4,
    Default = 5,
    Warning = 6,

    // Advanced Icon Set
    Default_OffLight = 7,
    Default_RedLight = 8,
    
    Information_Orange = 9,
    Information_Red = 10,

    Process_Stop = 11,
  }
}
