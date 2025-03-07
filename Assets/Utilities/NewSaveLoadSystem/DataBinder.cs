using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.DataBinding;

public class DataBinder {
    private readonly DebugLoggerSO _debugLogger;
    public List<IDataBind> GameDataBinds;
    
    public DataBinder(DebugLoggerSO debugLogger) {
        _debugLogger = debugLogger;
        GameDataBinds = new List<IDataBind>();
    }
    
    /// <summary>
    /// Binds "TBind" to the "TData". Finds the first object of the "TBind" type.
    /// </summary>
    public virtual void Bind<T, TData>(TData data) where T : MonoBehaviour, IBind<TData> where TData : ISaveable {
        Debug.Log("Binding data parameter 1");
            
        var bind = Object.FindObjectsByType<T>(FindObjectsSortMode.None).FirstOrDefault();
        if (bind == null) return;

        Bind(bind, data);
    }

    /// <summary>
    /// Binds "TBind" to the "TData". "TBind" object is given explicitly.
    /// </summary>
    public virtual void Bind<TBind, TData>(TBind bind, TData data)
        where TBind : IBind<TData> where TData : ISaveable {
        _debugLogger.Log("Binding data parameter 2");
            
        if (bind == null || data == null) {
            _debugLogger.ForceLogError("Bind or data is null. Binding operation aborted.");
            return;
        }
            
        if (!GameDataBinds.Contains(bind)) {
            GameDataBinds.Add(bind);
        }
        bind.SaveSystemData = data;
        // bind.LoadBindData();
        _debugLogger.Log($"{typeof(TBind)} object bind-ed to {typeof(TData)}");
    }

    public virtual void UnBind<TBind, TData>(TBind bind, TData data)
        where TBind : MonoBehaviour, IBind<TData> where TData : ISaveable {
        if (GameDataBinds.Contains(bind)) {
            GameDataBinds.Remove(bind);
        }
        bind.SaveSystemData = default;
        _debugLogger.Log($"{typeof(TBind)} object un-bind-ed from {typeof(TData)}");
    }
}