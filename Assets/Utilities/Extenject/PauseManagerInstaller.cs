using Zenject;

public class PauseManagerInstaller : MonoInstaller {
    public override void InstallBindings() {
        Container.Bind<IPauseManager>().FromInstance(new PauseManager(1)).AsSingle();
    }
}