namespace MsgBox
{
  using System;
  using System.Collections.Generic;

  /// <summary>
  /// Source: http://www.codeproject.com/Articles/70223/Using-a-Service-Locator-to-Work-with-MessageBoxes
  /// </summary>
  public class ServiceContainer
  {
    #region Fields
    public static readonly ServiceContainer Instance = new ServiceContainer();

    private readonly Dictionary<Type, object> mServiceMap;
    private readonly object mServiceMapLock;
    #endregion Fields

    #region constructor
    private ServiceContainer()
    {
      this.mServiceMap = new Dictionary<Type, object>();
      this.mServiceMapLock = new object();
    }
    #endregion constructor

    #region methods
    public void AddService<TServiceContract>(TServiceContract implementation)
        where TServiceContract : class
    {
      lock (this.mServiceMapLock)
      {
        this.mServiceMap[typeof(TServiceContract)] = implementation;
      }
    }

    public TServiceContract GetService<TServiceContract>()
        where TServiceContract : class
    {
      object service;

      lock (this.mServiceMapLock)
      {
        this.mServiceMap.TryGetValue(typeof(TServiceContract), out service);
      }

      return service as TServiceContract;
    }
    #endregion methods
  }
}
