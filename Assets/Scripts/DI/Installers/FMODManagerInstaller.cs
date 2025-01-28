using Attributes;
using FMODScripts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class FMODManagerInstaller : LifetimeScope {
    [SerializeField, Required] private DebugLoggerSO fmodManagerDebugLogger;

    protected override void Configure(IContainerBuilder builder) {
        builder.Register<FMODManagerDI>(Lifetime.Transient)
            .As<IAudioManager>()
            .WithParameter(fmodManagerDebugLogger);
    }
}