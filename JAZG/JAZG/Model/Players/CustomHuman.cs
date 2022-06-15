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


        public Position Position
        {
            get => human.Position;
            set => human.Position = value;
        }

        public CollisionKind? HandleCollision(ICharacter other)
        {
            return human.HandleCollision(other);
        }

        public double Extent
        {
            get => human.Extent;
            set => human.Extent = value;
        }
    }

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
                UseWeapon(zombie);
                
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
}