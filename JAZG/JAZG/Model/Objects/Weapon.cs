using System;
using System.Linq;
using JAZG.Model.Players;
using Mars.Components.Environments.Cartesian;
using ServiceStack;

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
                var gunhuman = human.weapons.Exists(i => i is Gun);
                var m16human = human.weapons.Exists(i => i is M16);
                
                if ( !gunhuman && this is Gun)
                {
                    human.weapons.Add(this);
                    Layer.Environment.Remove(this);
                    UnregisterHandle.Invoke(Layer, this);
                    Console.WriteLine("I got a Gun.");
                    human.HasWeapon = 4;
                    return CollisionKind.Pass;

                }
                if(!m16human && this is M16)
                {
                    human.weapons.Add(this);
                    Layer.Environment.Remove(this);
                    UnregisterHandle.Invoke(Layer, this);
                    Console.WriteLine("I got a M16.");
                    human.HasWeapon = 7;
                    return CollisionKind.Pass;

                }
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