using System;
using JAZG.Model.Players;
using Mars.Common.Core.Random;
using Mars.Interfaces.Environments;

namespace JAZG.Model.Objects
{
    public class Gun : Weapon
    {
        private int _ammo;

        public override void Init(FieldLayer layer)
        {
            base.Init(layer);
            var point = Layer.FindRandomPoint();
            Layer.Environment.Insert(this, point);
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