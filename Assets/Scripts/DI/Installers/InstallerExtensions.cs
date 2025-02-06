using System.Collections.Generic;
using System.Linq;
using VContainer;

public static class DIExtensions {
    public static void RegisterInstanceID<T>(this IContainerBuilder builder, T instance, string id) {
        builder.RegisterInstance(new InstanceDI<T>(id, instance));
    }

    public static T ResolveInstanceID<T>(this IObjectResolver resolver, string id) {
        return resolver.Resolve<IEnumerable<InstanceDI<T>>>()
            .Single(x => x.ID == id)
            .Instance;
    }
}