using System.Collections.Generic;
using Sandbox.Game.Entities;
using VRage.Game.Entity;
using VRageMath;

namespace HNZ.Utils
{
    public static class PlanetCollection
    {
        static readonly HashSet<MyPlanet> _planets;

        static PlanetCollection()
        {
            _planets = new HashSet<MyPlanet>();
        }

        public static void Initialize()
        {
            MyEntities.OnEntityAdd += OnEntityAdd;
            MyEntities.OnEntityRemove += OnEntityRemove;
        }

        public static void Close()
        {
            MyEntities.OnEntityAdd -= OnEntityAdd;
            MyEntities.OnEntityRemove -= OnEntityRemove;
        }

        static void OnEntityAdd(MyEntity entity)
        {
            var planet = entity as MyPlanet;
            if (planet != null)
            {
                _planets.Add(planet);
            }
        }

        static void OnEntityRemove(MyEntity entity)
        {
            var planet = entity as MyPlanet;
            if (planet != null)
            {
                _planets.Remove(planet);
            }
        }

        public static MyPlanet GetClosestPlanet(Vector3D position)
        {
            var closestPlanet = default(MyPlanet);
            var shortestDistance = double.MaxValue;
            foreach (var planet in _planets)
            {
                var pos = planet.PositionComp.GetPosition();
                var distance = Vector3D.Distance(pos, position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestPlanet = planet;
                }
            }

            return closestPlanet;
        }
    }
}