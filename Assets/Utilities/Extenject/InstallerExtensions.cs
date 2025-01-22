using System;
using UnityEngine;
using Zenject;

public static class InstallerExtensions {
    public static void BindFromHierarchyIfNull<T>(this DiContainer container, T component, string id) where T : Component {
        if (string.IsNullOrWhiteSpace(id)) {
            throw new ArgumentNullException(nameof(id));
        }
        
        if (component == null) {
            container.Bind<T>().WithId(id).FromComponentInHierarchy().AsSingle();
        }
        else {
            container.BindInstance(component).WithId(id);
        }
    }
    
    public static void BindFromHierarchyIfNull<T>(this DiContainer container, T component) where T : Component {
        if (component == null) {
            container.Bind<T>().FromComponentInHierarchy().AsSingle();
        }
        else {
            container.BindInstance(component);
        }
    }
}