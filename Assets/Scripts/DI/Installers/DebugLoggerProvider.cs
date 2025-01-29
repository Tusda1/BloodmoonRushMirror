using System.Collections.Generic;

public class DebugLoggerProvider {
    private readonly Dictionary<string, DebugLoggerSO> _debugLoggers = new();

    public DebugLoggerProvider() {
        var debugLoggers = ScrubUtils.GetAllScrubsInResourceFolder<DebugLoggerSO>("DebugLoggers");
        
        foreach (var debugLogger in debugLoggers) {
            _debugLoggers.Add(debugLogger.name, debugLogger);
        }
    }

    public DebugLoggerSO GetDebugLogger(string key) {
        return _debugLoggers[key];
    }
}