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
        protected void UseWeapon(Player player)
        {
           human.RunFromZombies(player);
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
            Console.WriteLine("Tick from custom human");
            RandomMove();
            throw new System.NotImplementedException();
        }
    }
}