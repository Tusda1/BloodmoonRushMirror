using Attributes;
using SaveLoadSystem;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class LevelInstaller : MonoInstaller {
    [SerializeField, Required] private Camera mainCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField, Required] private LevelDataManager levelDataManager;
    
    public override void InstallBindings() {
        BindFromHierarchyIfNull(mainCamera, "MainCamera");
        BindFromHierarchyIfNull(cinemachineBrain);
        BindFromHierarchyIfNull(levelDataManager);
    }

    private void BindFromHierarchyIfNull<T>(T component, string id = null) where T : Component {
        if (component == null) {
            if (id != null) {
                Container.Bind<T>().WithId(id).FromComponentInHierarchy().AsSingle();
                return;
            }

            Container.Bind<T>().FromComponentInHierarchy().AsSingle();
        }
        else {
            if (id != null) {
                Container.BindInstance(component).WithId(id);
                
                return;
            }

            Container.BindInstance(component);
        }
    }
}