using System;
using Mars.Components.Agents;
using Mars.Components.Environments.Cartesian;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Environments;

namespace JAZG.Model.Players
{



    /// <summary>
    /// abstract class to define player properties and main behaviour.
    /// Zombies and humans inherit from this class and can specify their behaviour in their concrete classes
    /// </summary>
    public abstract class Player : IAgent<FieldLayer>, ICharacter
    {
        


        public virtual void Init(FieldLayer layer)
        {
            Layer = layer;
        }

        public virtual void Tick()
        {
           
        }

        public Guid ID { get; set; }
        
        protected FieldLayer Layer { get; set; }
        
        // ****** Attributes
        protected int Energy { get; set; }

        public Position Position { get; set; }
        public CollisionKind? HandleCollision(ICharacter other)
        {
            throw new NotImplementedException();
        }

        public double Extent { get; set; }
    }
    
}
