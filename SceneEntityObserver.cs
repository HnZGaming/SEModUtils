using System;
using Sandbox.Game.Entities;
using VRage.ModAPI;

namespace HNZ.Utils
{
    // synchronous access to `MyAPIGateway.Entities.OnEntityCreate/Delete` events
    public sealed class SceneEntityObserver<T> where T : class, IMyEntity
    {
        readonly AddRemoveObserver<T> _self;

        public SceneEntityObserver()
        {
            _self = new AddRemoveObserver<T>();
        }

        public event Action<T> OnCreated
        {
            add { _self.OnAdded += value; }
            remove { _self.OnAdded -= value; }
        }

        public event Action<T> OnDeleted
        {
            add { _self.OnRemoved += value; }
            remove { _self.OnRemoved -= value; }
        }

        public void Initialize()
        {
            MyEntities.OnEntityCreate += OnEntityCrate;
            MyEntities.OnEntityDelete += OnEntityDelete;
        }

        public void Close()
        {
            _self.Close();
            MyEntities.OnEntityCreate -= OnEntityCrate;
            MyEntities.OnEntityDelete -= OnEntityDelete;
        }

        void OnEntityCrate(IMyEntity entity)
        {
            var element = entity as T;
            if (element != null)
            {
                _self.Add(element);
            }
        }

        void OnEntityDelete(IMyEntity entity)
        {
            var element = entity as T;
            if (element != null)
            {
                _self.Remove(element);
            }
        }

        public void Update()
        {
            _self.Update();
        }
    }
}