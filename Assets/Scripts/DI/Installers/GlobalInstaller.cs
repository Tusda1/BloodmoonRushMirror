using VContainer;
using VContainer.Unity;

public class GlobalInstaller : LifetimeScope {
    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterInstance<IPauseManager>(new PauseManager(1));
        builder.RegisterInstance<IAchievementManager>(new SteamAchievementManager());
    }
}