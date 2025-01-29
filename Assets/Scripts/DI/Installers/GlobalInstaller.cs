using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GlobalInstaller : LifetimeScope {
    [SerializeField] private SaveSlots saveSlots;
    
    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterInstance<IPauseManager>(new PauseManager(1));
        builder.RegisterInstance(saveSlots);
        
        builder.Register<SteamAchievementManager>(Lifetime.Scoped).As<IAchievementManager>();
        builder.Register<DebugLoggerProvider>(Lifetime.Singleton);
    }
}

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