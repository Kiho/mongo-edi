namespace Edi
{
  using MsgBox;

  public static class Msg
  {
    static Msg()
    {
      ServiceInjector.InjectServices();
    }

    /// <summary>
    /// Retrieves a service object identified by <typeparamref name="TServiceContract"/>.
    /// </summary>
    /// <typeparam name="TServiceContract">The type identifier of the service.</typeparam>
    public static TServiceContract GetService<TServiceContract>()
                  where TServiceContract : class
    {
      return ServiceContainer.Instance.GetService<TServiceContract>();
    }
/*
    public static MsgBox.IMsgBoxService Box()
    {
      return Msg.GetService<IMsgBoxService>();
    }
*/
    public static IMsgBoxService Box
    {
      get
      {
        return Msg.GetService<IMsgBoxService>();
      }
    }
  }
}
