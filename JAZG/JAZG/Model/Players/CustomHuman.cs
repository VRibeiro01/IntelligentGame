using System;
using System.Collections.Generic;
using System.Linq;
using JAZG;
using JAZG.Model.Objects;
using JAZG.Model.Players;
using Mars.Components.Agents;
using Mars.Components.Environments.Cartesian;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Environments;
using ServiceStack;

namespace JAZG.Model.Players
{
    public class CustomHuman : Human
    {
        
        public int lastM16Shoot { get; set; }
        public int lastGunShoot { get; set; }
        
        public override void Tick()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Tick from custom human");

            var weapon = FindClosestWeapon();
            CollectItem(weapon);

            var zombie = FindClosestZombie();

            if (weapons.Count > 0 &&  (this.Layer.GetCurrentTick() - lastGunShoot >= 8)  &&  (this.Layer.GetCurrentTick() - lastM16Shoot >= 5))
            {
                if (hasM16())
                {
                    lastM16Shoot = (int) this.Layer.GetCurrentTick();
                    Console.WriteLine("custom human shoots m16");
                }
                else
                {
                    lastGunShoot = (int) this.Layer.GetCurrentTick();
                    Console.WriteLine("custom human shoots gun");
                }
                IsShooting = UseWeapon(zombie);
                
            }
            else
            {
                if (GetDistanceFromItem(weapon) < GetDistanceFromPlayer(zombie))
                {
                    CollectItem(weapon);
                    Console.WriteLine("custom human collects weapon");
                }

                if(GetDistanceFromItem(weapon) > GetDistanceFromPlayer(zombie))
                {
                    RunFromZombies(zombie);
                    Console.WriteLine("custom human runs from zombie");
                }
                else
                {
                    RandomMove();
                    Console.WriteLine("custom human moves random");
                }
            }
            Console.ForegroundColor = ConsoleColor.White;

        }
    }
    
    public class _CustomHuman : Human
    {
        public override void Tick()
        {
            Console.WriteLine("Tick from custom human");
            var zombies = FindZombies();
            var speed = zombies.IsEmpty() ? 0 : Math.Max(1, 
                Math.Floor((double) zombies.OrderByDescending(zombie => zombie.Energy).FirstOrDefault().Energy / 15));
            var cZombie = FindClosestZombie();
            var lZombie = (Zombie)zombies.OrderBy(zombie => zombie.Energy).FirstOrDefault();
            var distance = zombies.IsEmpty() ? Double.MaxValue : GetDistanceFromPlayer(cZombie);
            distance = distance - 2 / speed;
            var sDistance = distance + Math.Floor(distance / speed);
            var wDistance = distance;
            var weapon = FindClosestWeapon();
            var weaponDistance = weapon == null ? Double.MaxValue : GetDistanceFromItem(weapon);
            var cooldown = 0;
            var damage = 0;
            
            if (hasM16())
            {
                cooldown = 8;
                damage = 30;
            }
            else if (weapons.Count > 0)
            {
                cooldown = 5;
                damage = 15;
            }

            if (zombies.IsEmpty())
            {
                if (weaponDistance != Double.MaxValue && damage < 30)
                {
                    CollectItem(weapon);
                }
                else
                {
                    RandomMove();
                }
            } else if (damage > 0)
            {
                if (((sDistance / cooldown) + 1) * damage >= cZombie.Energy)
                {
                    if (!UseWeapon(cZombie)) RunFromZombies(cZombie); 
                } 
                else if (((sDistance / cooldown) + 1) * damage >= lZombie.Energy)
                {
                    if (!UseWeapon(lZombie)) RunFromZombies(cZombie);
                }
                else
                {
                    if (!UseWeapon(cZombie)) RunFromZombies(cZombie);
                }
            }
            else
            {
                if (wDistance > weaponDistance)
                {
                    if (weapon.GetType() == typeof(M16) && cZombie.Energy <= 30 || cZombie.Energy <= 15)
                    {
                        CollectItem(weapon);
                    }
                    else
                    {
                        RunFromZombies(cZombie);
                    }
                }
                else
                {
                    RunFromZombies(cZombie);
                }
            }
        }
    }

}