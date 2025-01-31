using FMODScripts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GlobalInstaller : LifetimeScope {
    [SerializeField] private SaveSlots saveSlots;
    
    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterInstance<IPauseManager>(new PauseManager(1));
        builder.RegisterInstance(saveSlots);
        
        builder.Register<IProvider<DebugLoggerSO>, DebugLoggerProvider>(Lifetime.Singleton);
        
        builder.Register<IAchievementManager, SteamAchievementManager>(Lifetime.Singleton);
        builder.Register<IAudioManager, FMODManagerDI>(Lifetime.Singleton);
    }
}