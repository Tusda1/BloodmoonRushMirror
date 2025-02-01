using System.Collections.Generic;

public interface IProvider<T> {
    protected Dictionary<string, T> Services { get; }
    
    public T Get(string key) {
        return Services[key];
    }
}