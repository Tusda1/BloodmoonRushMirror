public abstract class InstanceDI<T> {
    public string ID;
    public T Instance;
    
    protected InstanceDI(string id, T instance) {
        ID = id;
        Instance = instance;
    }
}