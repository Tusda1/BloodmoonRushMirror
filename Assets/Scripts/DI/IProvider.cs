using System.Collections.Generic;

public interface IProvider<T> {
    protected Dictionary<string, T> Services { get; }
    
    public T Get(string key) {
        return Services[key];
    }
    
    public void Add(string key, T service) {
        Services.Add(key, service);
    }
}