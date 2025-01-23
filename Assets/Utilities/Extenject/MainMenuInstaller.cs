using UnityEngine;
using Zenject;

public class MainMenuInstaller : MonoInstaller {
    [SerializeField] private Camera mainCamera;
    
    public override void InstallBindings() {
        Container.BindFromHierarchyIfNull(mainCamera, "MainCamera");
    }
}