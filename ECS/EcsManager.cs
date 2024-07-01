using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;

namespace ECS;

/// <summary>
/// A generic class for handling entity component systems, where <typeparamref name="TComponent"/> is the component type that your entities will use.
/// See the <see cref="EcsManager{TComponent}.Builder"/> class to get started.
/// </summary>
/// <typeparam name="TComponent"></typeparam>
public class EcsManager<TComponent> where TComponent : IDisposable {
    private readonly FrozenDictionary<Type, TComponent?[]> _entityComponentStorage;
    private readonly bool[] _entities;
    
    /// <summary>
    /// Constructor if you want to create an instance manually.
    /// It is reccomended to use the <see cref="EcsManager{TComponent}.Builder"/> class instead.
    /// </summary>
    /// <param name="entityCap">The max amount of entities that can be active at once</param>
    /// <param name="componentTypes">A collection of every component type that can be used</param>
    public EcsManager(int entityCap, HashSet<Type> componentTypes) {
        _entityComponentStorage = componentTypes.ToFrozenDictionary(key => key, _ => new TComponent?[entityCap]);
        _entities = new bool[entityCap];
    }

    /// <summary>
    /// Check if an entity exists with the given <paramref name="id"/> 
    /// </summary>
    /// <param name="id">Id of the entity to test for</param>
    /// <returns>Does it exist?</returns>
    public bool EntityExists(int id) => _entities[id];
    
    /// <summary>
    /// Returns the max amount of entities this ECS can support
    /// </summary>
    public int MaxEntities => _entities.Length;

    /// <summary>
    /// Returns a list of every component based on the given type
    /// </summary>
    /// <param name="t">The target component type</param>
    /// <returns>Every component of the given type</returns>
    public TComponent?[] GetComponentsForType(Type t) => _entityComponentStorage[t];
    
    /// <summary>
    /// Returns a collection of every component type supported by this ECS
    /// </summary>
    /// <returns></returns>
    public ImmutableArray<Type> GetComponentTypes() => _entityComponentStorage.Keys;
    
    /// <summary>
    /// Runs the given function on every entity.
    /// </summary>
    /// <param name="action">Args: (int id, bool active)</param>
    public void ForEachEntity(Action<int, bool> action) {
        for (int i = 0; i < _entities.Length; i++) {
            action(i, _entities[i]);
        }
    }
    
    /// <summary>
    /// Runs the given function on every component
    /// </summary>
    /// <param name="action">Args: (int entityId, <typeparamref name="TComponent"/> component)</param>
    public void ForEachComponent(Action<int, TComponent> action) {
        foreach (TComponent?[] components in _entityComponentStorage.Values) {
            for (int id = 0; id < components.Length; id++) {
                TComponent? cur = components[id];
                if (cur != null) action(id, cur);
            }
        }
    }

    /// <summary>
    /// Gets (or creates if not found) a component of type <typeparamref name="T"/> on the given entity.
    /// <seealso cref="GetComponent"/>
    /// </summary>
    /// <param name="id">Target entity id</param>
    /// <typeparam name="T">Component type</typeparam>
    /// <returns>A new or existing component</returns>
    public T GetComponent<T>(int id) where T : TComponent => (T) (_entityComponentStorage[typeof(T)][id] ??= Activator.CreateInstance<T>());
    
    /// <summary>
    /// Gets (or creates if not found) a component of type <paramref name="t"/> on the given entity
    /// <seealso cref="GetComponent{T}"/>
    /// </summary>
    /// <param name="t">Type of component to be added</param>
    /// <param name="id">Target entity id</param>
    /// <returns>A new or existing component</returns>
    public TComponent GetComponent(Type t, int id) => _entityComponentStorage[t][id] ??= (TComponent)Activator.CreateInstance(t)!;
    
    /// <summary>
    /// Checks whether the target entity has a component of type <typeparamref name="T"/>
    /// <seealso cref="HasComponent"/>
    /// <seealso cref="TryGetComponent{T}"/>
    /// </summary>
    /// <param name="id">Target entity id</param>
    /// <typeparam name="T">Component type</typeparam>
    /// <returns>Whether the entity has the component</returns>
    public bool HasComponent<T>(int id) where T : TComponent => _entityComponentStorage[typeof(T)][id] != null;
    
    /// <summary>
    /// Checks whether the target entity has a component of type <paramref name="t"/>
    /// <seealso cref="HasComponent{T}"/>
    /// <seealso cref="TryGetComponent{T}"/>
    /// </summary>
    /// <param name="t">Component type</param>
    /// <param name="id">Target entity id</param>
    /// <returns>Whether the entity has the component</returns>
    public bool HasComponent(Type t, int id) => _entityComponentStorage[t][id] != null;

    /// <summary>
    /// Returns whether the target entity has a component of type <typeparamref name="T"/>, and provided it does, outputs it to the <paramref name="component"/> parameter.
    /// </summary>
    /// <param name="id">Target entity id</param>
    /// <param name="component">Component</param>
    /// <typeparam name="T">Component type</typeparam>
    /// <returns>Whether the entity has the component</returns>
    public bool TryGetComponent<T>(int id, out T component) where T : TComponent {
        TComponent? t = _entityComponentStorage[typeof(T)][id];
        if (t == null) {
            component = default!;
            return false;
        }

        component = (T)t;
        
        return true;
    }
    
    /// <summary>
    /// Disposes then removes the target entity
    /// </summary>
    /// <param name="id">Target entity id</param>
    public virtual void RemoveEntity(int id) {
        foreach ((Type componentType, TComponent?[] instances) in _entityComponentStorage) {
            instances[id]?.Dispose();
            instances[id] = default;
        }
    }

    /// <summary>
    /// Creates a new entity with the given components, given there are enough slots available.
    /// </summary>
    /// <param name="startingComponents">The components to initalize for this entity</param>
    /// <returns>The id of the created entity. -1 if it could not be created.</returns>
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
    
    /// <summary>
    /// Builder for the <see cref="EcsManager{TComponent}"/> class.
    /// Usage is as follows:
    /// <code> EcsManager{T} manager = new EcsManager{T}.Builder(16).AddAssembly(typeof(Program).Assembly).Build(); </code>
    /// Where 16 is the entity cap, and the added assembly is the assembly that contains all the component types you want to add. They may also be manually added like so:
    /// <code> EcsManager{T} manager = new EcsManager{T}.Builder(16).Add{Transform}().Add{Camera}.Add{WhateverYourComponentNamesAre}.Build(); </code>
    /// </summary>
    /// <param name="entityCap"></param>
    public readonly ref struct Builder(int entityCap) {
        private readonly HashSet<Type> _componentTypes = [];

        /// <summary>
        /// Add a component of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        /// <returns>self</returns>
        public Builder Add<T>() where T : TComponent {
            _componentTypes.Add(typeof(T));
            return this;
        }
        
        /// <summary>
        /// Adds all types that extend <typeparamref name="TComponent"/> in the given assembly
        /// </summary>
        /// <param name="assembly">The assembly to search</param>
        /// <returns>self</returns>
        public Builder AddAssembly(Assembly assembly) {
            foreach (Type t in assembly.GetTypes()) {
                if (!typeof(TComponent).IsAssignableFrom(t) || t == typeof(TComponent)) continue;
                _componentTypes.Add(t);
            }
            return this;
        }

        /// <summary>
        /// Builds the final ECS instance
        /// </summary>
        /// <returns>The <see cref="EcsManager{TComponent}"/> instance</returns>
        public EcsManager<TComponent> Build() {
            return new EcsManager<TComponent>(entityCap, _componentTypes);
        }
    }
}