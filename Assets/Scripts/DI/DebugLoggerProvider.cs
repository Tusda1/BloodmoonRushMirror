using System.Collections.Generic;

public class DebugLoggerProvider : IProvider<DebugLoggerSO>  {
    private readonly Dictionary<string, DebugLoggerSO> _services = new();

    public DebugLoggerProvider() {
        var debugLoggers = ScrubUtils.GetAllScrubsInResourceFolder<DebugLoggerSO>("DebugLoggers");
        
        foreach (var debugLogger in debugLoggers) {
            _services.Add(debugLogger.name, debugLogger);
        }
    }

    Dictionary<string, DebugLoggerSO> IProvider<DebugLoggerSO>.Services => _services;
}

public interface IProvider<T> {
    protected Dictionary<string, T> Services { get; }
    
    public T Get(string key) {
        return Services[key];
    }
}