using System.Collections;
using System.Collections.Generic;
using Sandbox.Game.Entities;
using VRage.Collections;
using VRage.Game.Entity;
using VRage.ModAPI;

namespace HNZ.Utils
{
    // type-filter to `MyEntities.GetEntities()`
    public sealed class SceneEntityTracker<T> : IEnumerable<T> where T : class, IMyEntity
    {
        readonly MyConcurrentHashSet<T> _entities;

        public SceneEntityTracker()
        {
            _entities = new MyConcurrentHashSet<T>();
        }

        public IEnumerator<T> GetEnumerator() => _entities.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Initialize()
        {
            MyEntities.OnEntityCreate += OnEntityCreated;
            MyEntities.OnEntityDelete += OnEntityDeleted;
        }

        public void Close()
        {
            MyEntities.OnEntityCreate -= OnEntityCreated;
            MyEntities.OnEntityDelete -= OnEntityDeleted;
            _entities.Clear();
        }

        void OnEntityCreated(MyEntity obj)
        {
            var entity = obj as T;
            if (entity != null)
            {
                _entities.Add(entity);
            }
        }

        void OnEntityDeleted(MyEntity obj)
        {
            var entity = obj as T;
            if (entity != null)
            {
                _entities.Remove(entity);
            }
        }

        // less locking
        public void CopyTo(List<T> collection)
        {
            collection.AddRange(_entities);
        }

        public bool TryGetEntityByPrefix(string prefix, out T entity)
        {
            foreach (var e in _entities)
            {
                if (e.Name.StartsWith(prefix))
                {
                    entity = e;
                    return true;
                }
            }

            entity = default(T);
            return false;
        }
    }
}