using System;
using JAZG.Model.Players;
using Mars.Components.Environments.Cartesian;
using Mars.Interfaces.Environments;

namespace JAZG.Model.Objects
{
    public class Weapon : Item
    {
        public Weapon() : base(Layer)
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
                Console.WriteLine("i got a Gun");
                //TODO remove weapon form environment
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