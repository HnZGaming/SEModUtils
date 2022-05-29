using System.Collections.Generic;
using Sandbox.Game.Entities;
using VRage.Game.Entity;
using VRage.ModAPI;

namespace HNZ.Utils
{
    public sealed class SceneEntityCachedCollection<T> where T : class, IMyEntity
    {
        readonly CachedHashSet<T> _hashset;

        public SceneEntityCachedCollection()
        {
            _hashset = new CachedHashSet<T>();
        }

        public void Initialize()
        {
            MyEntities.OnEntityAdd += OnEntityAdd;
            MyEntities.OnEntityRemove += OnEntityRemove;
        }

        public void Close()
        {
            MyEntities.OnEntityAdd -= OnEntityAdd;
            MyEntities.OnEntityRemove -= OnEntityRemove;
        }

        public IEnumerable<T> ApplyChanges()
        {
            _hashset.ApplyChanges();
            return _hashset;
        }

        void OnEntityAdd(MyEntity obj)
        {
            var t = obj as T;
            if (t != null)
            {
                _hashset.Add(t);
            }
        }

        void OnEntityRemove(MyEntity obj)
        {
            var t = obj as T;
            if (t != null)
            {
                _hashset.Remove(t);
            }
        }
    }
}