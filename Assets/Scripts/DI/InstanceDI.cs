public class InstanceDI<T> {
    public readonly string ID;
    public readonly T Instance;

    public InstanceDI(string id, T instance) {
        ID = id;
        Instance = instance;
    }
}