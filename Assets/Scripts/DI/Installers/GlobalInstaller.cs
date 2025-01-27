using Zenject;

public class GlobalInstaller : MonoInstaller {
    public override void InstallBindings() {
        Container.Bind<IPauseManager>().FromInstance(new PauseManager(1)).AsSingle();
        Container.Bind<IAchievementManager>().FromInstance(new SteamAchievementManager()).AsSingle();
    }
}