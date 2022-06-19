using System;
using JAZG.Model.Players;
using Mars.Common.Core.Random;
using NetTopologySuite.Geometries;
using Position = Mars.Interfaces.Environments.Position;

namespace JAZG.Model.Objects
{
    public class Gun : Weapon
    {
        public int _ammo;

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

        public override bool Use(Zombie zombie)
        {
            if (zombie is null) return false;
            return Shoot(zombie);
        }

        private bool Shoot(Zombie zombie)
        {
            if (_cooldown == 0)
            {
                Console.WriteLine("BAM");
                // _ammo--;
                if (RandomHelper.Random.Next(101) > 50)
                {
                    Console.WriteLine("Hit.");
                    zombie.Energy -= 15;
                }
                else
                {
                    Console.WriteLine("Missed.");
                }

                _cooldown = 8;
                return true;
            }

            _cooldown--;
            return false;
        }
    }
}