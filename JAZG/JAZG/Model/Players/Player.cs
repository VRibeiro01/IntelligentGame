using System;
using Mars.Components.Agents;
using Mars.Interfaces.Agents;

namespace JAZG.Model.Players
{



    /// <summary>
    /// abstract class to define player properties and main behaviour.
    /// Zombies and humans inherit from this class and can specify their behaviour in their concrete classes
    /// </summary>
    public abstract class Player : IAgent<FieldLayer>
    {


        public virtual void Init(FieldLayer layer)
        {
            Layer = layer;
        }

        public virtual void Tick()
        {
            Console.WriteLine("Hello world From Player!");
        }

        public Guid ID { get; set; }
        
        protected FieldLayer Layer { get; set; }
        
        // ****** Attributes
        protected int Energy { get; set; } 
        
    }
    
}
