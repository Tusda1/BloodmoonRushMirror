using FMODScripts;
using UnityEngine;
using Zenject;

public class FMODManagerInstaller : MonoInstaller {
    [SerializeField] private DebugLoggerSO fmodManagerDebugLogger;
    
    public override void InstallBindings() {
        if (fmodManagerDebugLogger == null) {
            fmodManagerDebugLogger = ScrubUtils.GetAllScrubsInResourceFolder<DebugLoggerSO>("DebugLoggers")
                .GetByName("FMODManager");
            
            Debug.Log(nameof(fmodManagerDebugLogger) + " found manually");
        }
        
        Container.Bind<IAudioManager>().FromInstance(new FMODManagerDI(fmodManagerDebugLogger)).AsSingle().NonLazy();
    }
}