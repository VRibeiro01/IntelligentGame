using System;
using JAZG.Model.Players;
using Mars.Components.Environments.Cartesian;

namespace JAZG.Model.Objects
{
    public abstract class Weapon : Item
    {
        protected Weapon() : base(Layer)
        {
        }
        
        public override bool IsRoutable(ICharacter character)
        {
            return true;
        }

        public override CollisionKind? HandleCollision(ICharacter character)
        {
            if (character is Human)
            {
                var human = (Human) character;
                human.weapons.Add(this);
                human.HasWeapon = true;
                Console.WriteLine("I got a weapon.");
                Layer.Environment.Remove(this);
                UnregisterHandle.Invoke(Layer, this);
                return CollisionKind.Remove;
            }

            return CollisionKind.Pass;
        }

        public override VisibilityKind? HandleExploration(ICharacter explorer)
        {
            return VisibilityKind.Opaque;
        }

        public virtual void Use(Zombie zombie)
        {
            
        }
    }
}