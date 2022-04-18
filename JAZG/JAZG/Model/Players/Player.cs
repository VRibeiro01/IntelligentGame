using System;
using Mars.Components.Environments.Cartesian;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;

namespace JAZG.Model.Players
{
    /// <summary>
    ///     abstract class to define player properties and main behaviour.
    ///     Zombies and humans inherit from this class and can specify their behaviour in their concrete classes
    /// </summary>
    public abstract class Player : IAgent<FieldLayer>, ICharacter
    {
        // X coordinate where agent will spawn on collisionHashEnvironment
        [PropertyDescription (Name="xSpawn")]
        private int XSpawn { get; set; }
        
        // X coordinate where agent will spawn on collisionHashEnvironment
        [PropertyDescription (Name="ySpawn")]
        private int YSpawn { get; set; }

        protected FieldLayer Layer { get; set; }

        // ****** Attributes
        protected int Energy { get; set; }
        public Guid ID { get; set; }


        public virtual void Init(FieldLayer layer)
        {
            Layer = layer;
            Position = Position.CreatePosition(XSpawn, YSpawn);
           }

        public virtual void Tick()
        {
        }


        public Position Position { get; set; }

        public double Extent { get; set; }

        public CollisionKind? HandleCollision(ICharacter other)
        {
            throw new NotImplementedException();
        }
    }
}