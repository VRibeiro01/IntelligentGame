using System;
using JAZG.Model.Players;
using Mars.Common.Core.Random;
using NetTopologySuite.Geometries;
using Position = Mars.Interfaces.Environments.Position;

namespace JAZG.Model.Objects
{
    public class M16 : Weapon
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
            return Shoot(zombie);
        }

        
        // Die Treffenwahrscheinlichkeit und schaden mehr als bei Gun
        private bool Shoot(Zombie zombie)
        {
            if (_cooldown == 0)
            {
                Console.WriteLine("BAAAM");
                // _ammo--;
                if (RandomHelper.Random.Next(101) > 30)
                {
                    Console.WriteLine("Hit.");
                    zombie.Energy -= 30;
                }
                else
                {
                    Console.WriteLine("M16 Missed.");
                }

                _cooldown = 5;
                return true;
            }

            _cooldown--;
            return false;
        }
    }
}