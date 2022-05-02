using System;
using Mars.Common.Core.Random;
using Mars.Components.Environments.Cartesian;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Layers;

namespace JAZG.Model.Players
{
    /// <summary>
    ///     abstract class to define player properties and main behaviour.
    ///     Zombies and humans inherit from this class and can specify their behaviour in their concrete classes
    /// </summary>
    public abstract class Player : IAgent<FieldLayer>, ICharacter
    {
        [PropertyDescription] public UnregisterAgent UnregisterHandle { get; set; }

        protected FieldLayer Layer { get; set; }

        // ****** Attributes
        public int Energy { get; set; }

        public Guid ID { get; set; }

        
        public int Speed { get; set; }

        public virtual void Init(FieldLayer layer)
        {
            Layer = layer;
            // If position not null, set position to a random point in layer
            Position ??= layer.FindRandomPosition();

            // Every Player has random speed
            //TODO reduce zombie speed ??
            Speed = RandomHelper.Random.Next(40);

            // All players have same extent
            Extent = 1.8;
            Layer.Environment.Insert(this, Position);
        }

        public virtual void Tick()
        {
        }

        public Position Position { get; set; }

        public double Extent { get; set; }

        public virtual CollisionKind? HandleCollision(ICharacter other)
        {
            // Dummy Implementierung
            return CollisionKind.Pass;
        }

        protected void RandomMove()
        {
            var bearing = RandomHelper.Random.Next(360);
            Position = Layer.Environment.Move(this, bearing, Speed);
        }

        public virtual void Kill()
        {
            Layer.Environment.Remove(this);
            UnregisterHandle.Invoke(Layer, this);
        }
    }
}