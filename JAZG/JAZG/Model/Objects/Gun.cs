using System;
using JAZG.Model.Players;
using Mars.Common.Core.Random;
using NetTopologySuite.Geometries;
using Position = Mars.Interfaces.Environments.Position;

namespace JAZG.Model.Objects
{
    public class Gun : Weapon
    {
        private int _ammo;

        public override void Init(FieldLayer layer)
        {
            base.Init(layer);
            var point = Layer.FindRandomPoint();
            Coordinate[] coordinates =
            {
                new(point.X - 1, point.Y - 1), new(point.X - 1, point.Y + 1), new(point.X + 1, point.Y + 1),
                new(point.X + 1, point.Y - 1), new(point.X - 1, point.Y - 1)
            };
            Geometry geometry = new Polygon(new LinearRing(coordinates));
            Layer.Environment.Insert(this, geometry);
            Position = new Position(point.X, point.Y);
        }

        public int GetAmmo()
        {
            return _ammo;
        }

        public void SetAmmo(int newVal)
        {
            if (newVal + _ammo > 10) return;
            _ammo = _ammo + newVal;
        }

        public override void Use(Zombie zombie)
        {
            Shoot(zombie);
        }

        private void Shoot(Zombie zombie)
        {
            Console.WriteLine("BAM");
            _ammo--;
            if (RandomHelper.Random.Next(101) > 50)
            {
                Console.WriteLine("Hit.");
                zombie.Energy -= 15;
            }
            else Console.WriteLine("Missed.");
            //TODO target is hit with a degree of randomness
            //TODO what happens if zombie is hit
        }
    }
}