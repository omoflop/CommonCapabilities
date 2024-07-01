using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;

namespace ECS;

public class EcsManager<TComponent> where TComponent : IDisposable {
    private readonly FrozenDictionary<Type, TComponent?[]> _entityComponentStorage;
    private readonly bool[] _entities;
    
    public EcsManager(int entityCap, HashSet<Type> componentTypes) {
        _entityComponentStorage = componentTypes.ToFrozenDictionary(key => key, _ => new TComponent?[entityCap]);
        _entities = new bool[entityCap];
    }

    public bool EntityExists(int id) => _entities[id];
    public int MaxEntities => _entities.Length;

    public TComponent?[] GetComponentsForType(Type t) => _entityComponentStorage[t];
    public ImmutableArray<Type> GetComponentTypes() => _entityComponentStorage.Keys;
    
    public void ForEachEntity(Action<int, bool> action) {
        for (int i = 0; i < _entities.Length; i++) {
            action(i, _entities[i]);
        }
    }
    
    public void ForEachComponent(Action<int, TComponent> action) {
        foreach (TComponent?[] components in _entityComponentStorage.Values) {
            for (int id = 0; id < components.Length; id++) {
                TComponent? cur = components[id];
                if (cur != null) action(id, cur);
            }
        }
    }

    public T GetComponent<T>(int id) where T : TComponent => (T) (_entityComponentStorage[typeof(T)][id] ??= Activator.CreateInstance<T>());
    public TComponent GetComponent(Type t, int id) => _entityComponentStorage[t][id] ??= (TComponent)Activator.CreateInstance(t)!;
    public bool HasComponent<T>(int id) where T : TComponent => _entityComponentStorage[typeof(T)][id] != null;
    public bool HasComponent(Type t, int id) => _entityComponentStorage[t][id] != null;

    public bool TryGetComponent<T>(int id, out T component) where T : TComponent {
        TComponent? t = _entityComponentStorage[typeof(T)][id];
        if (t == null) {
            component = default!;
            return false;
        }

        component = (T)t;
        
        return true;
    }
    
    public virtual void RemoveEntity(int id) {
        foreach ((Type componentType, TComponent?[] instances) in _entityComponentStorage) {
            instances[id]?.Dispose();
            instances[id] = default;
        }
    }

    public virtual int AddEntity(params Type[] startingComponents) {
        for (int i = 0; i < _entities.Length; i++) {
            if (_entities[i]) continue;
            _entities[i] = true;
            
            foreach (Type componentType in startingComponents) {
                _entityComponentStorage[componentType][i] = (TComponent) Activator.CreateInstance(componentType)!;
            }

            return i;
        }
        return -1;
    }
    
    public readonly ref struct Builder(int entityCap) {
        private readonly HashSet<Type> _componentTypes = [];

        public Builder Add<T>() where T : TComponent {
            _componentTypes.Add(typeof(T));
            return this;
        }
        
        public Builder AddAssembly(Assembly assembly) {
            foreach (Type t in assembly.GetTypes()) {
                if (!typeof(TComponent).IsAssignableFrom(t) || t == typeof(TComponent)) continue;
                _componentTypes.Add(t);
            }
            return this;
        }

        public EcsManager<TComponent> Build() {
            return new EcsManager<TComponent>(entityCap, _componentTypes);
        }
    }
}