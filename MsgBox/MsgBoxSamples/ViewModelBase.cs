namespace MsgBoxSamples
{
  using MsgBox;

  public static class MsgBoxBase
  {
    static MsgBoxBase()
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
  }
}
