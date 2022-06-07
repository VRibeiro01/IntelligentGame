using System;
using JAZG.Model.Players;
using Mars.Components.Environments.Cartesian;

namespace JAZG.Model.Objects
{
    public abstract class Weapon : Item
    {
        protected int _cooldown = 0;
        
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
                if (this is Gun)
                {
                    human.HasWeapon = 4;
                }
                else
                {
                    human.HasWeapon = 7;
                }
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

        public virtual bool Use(Zombie zombie)
        {
            return false;
        }
    }
}