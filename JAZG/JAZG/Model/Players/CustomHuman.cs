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
using Mars.Numerics;
using ServiceStack;

namespace JAZG.Model.Players
{
    public abstract class AbstractCustomHuman : IAgent<FieldLayer>, ICharacter
    {
        //protected FieldLayer Layer { get; set; }

        private Human human { get; set; }

        public void Init(FieldLayer layer)
        {
            human = new Human();
            human.Init(layer);
        }

        public abstract void Tick();
        
        protected void RunFromZombies(Player player)
        {
            human.RunFromZombies(player);
        }
        protected Weapon FindClosestWeapon()
        {
          return human.FindClosestWeapon();
        }
        protected Zombie FindClosestZombie()
        {
            return human.FindClosestZombie();
        }
        protected List<Player> FindZombies()
        {
           return human.FindZombies();
        }
        protected void CollectItem(Item item)
        {
           human.CollectItem(item);
        }
        protected void RandomMove()
        {
           human.RandomMove();
        }
        protected void UseWeapon(Zombie player)
        {
           human.UseWeapon(player);
        }
        protected double GetDistanceFromPlayer(Player player)
        {
           return human.GetDistanceFromPlayer(player);
        }
        protected double GetDistanceFromItem(Item item)
        {
            return human.GetDistanceFromItem(item);
        }
        protected double GetDirectionToItem(Item item)
        {
            return human.GetDirectionToItem(item);
        }
        protected double GetDirectionToPlayer(Player player)
        {
            return human.GetDirectionToPlayer(player);
        }
        
        
        public Guid ID { get; set; }
        public Position Position { get => human.Position; set => human.Position = value; }
        public CollisionKind? HandleCollision(ICharacter other)
        {
            return human.HandleCollision(other);
        }

        public double Extent { get => human.Extent; set => human.Extent = value; }
    }


    public class CustomHuman : Human
    {
        public override void Tick()
        {
            //Console.WriteLine("Tick from custom human");
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
                    if (!UseWeapon(cZombie))
                    {
                        RunFromZombies(cZombie);
                    }
                } else if (((sDistance / cooldown) + 1) * damage >= lZombie.Energy)
                {
                    if (!UseWeapon(lZombie))
                    {
                        RunFromZombies(cZombie);
                    }
                }
                else
                {
                    RunFromZombies(cZombie);
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