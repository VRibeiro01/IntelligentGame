using System;
using JAZG.Model.Players;
using Mars.Interfaces.Environments;

namespace JAZG.Model.Objects
{
    public class Gun : Weapon
    {
        private int ammo;

        public int GetAmmo()
        {
            return ammo;
        }

        public void SetAmmo(int newVal)
        {
            if (newVal + ammo > 10) return;

            ammo = ammo + newVal;
        }

        public override void Use(Zombie zombie)
        {
            ammo--;
            Console.WriteLine("Shoot Shoot");
            //TODO target is hit with a degree of randomness
            //TODO what happens if zombie is hit
        }
        
        
    }
}