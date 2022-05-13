using System;
using JAZG.Model.Objects;
using Mars.Common;
using Mars.Common.Core.Random;
using Mars.Components.Environments.Cartesian;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Layers;
using Mars.Numerics;

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
        protected bool Dead;
        public int Energy { get; set; }

        public Guid ID { get; set; }
        public int Speed { get; set; }
        public Position Position { get; set; }
        public double Extent { get; set; }

        public virtual void Init(FieldLayer layer)
        {
            Layer = layer;
            // If position not null, set position to a random point in layer
            Position ??= layer.FindRandomPosition();

            // Every Player has random speed
            //TODO reduce zombie speed ??
            Speed = RandomHelper.Random.Next(40);

            // All players have same extent
            Extent = 1;

            while (!Layer.Environment.Insert(this, Position))
            {
                Position = layer.FindRandomPosition();   
            }
        }

        public virtual void Tick()
        {
        }


        public CollisionKind? HandleCollision(ICharacter other)
        {
            // Dummy Implementierung
            return CollisionKind.Block;
        }

        protected void RandomMove()
        {
            var bearing = RandomHelper.Random.Next(360);
            Layer.Environment.Move(this, bearing, 1);
        }

        public virtual void Kill()
        {
            if (Dead) return;
            Dead = true;
            Layer.Environment.Remove(this);
            UnregisterHandle.Invoke(Layer, this);
        }

        protected double GetDistanceFromPlayer(Player other)
        {
            return Distance.Chebyshev(
                Position.PositionArray, other.Position.PositionArray);
        }

        protected double GetDistanceFromItem(Item item)
        {
            return Distance.Chebyshev(
                Position.PositionArray, item.Position.PositionArray);
        }

        protected double GetDirectionToPlayer(Player other)
        {
            return PositionHelper.CalculateBearingCartesian(
                Position.X, Position.Y, other.Position.X, other.Position.Y);
        }

        protected double GetDirectionToItem(Item item)
        {
            return PositionHelper.CalculateBearingCartesian(
                Position.X, Position.Y, item.Position.X, item.Position.Y);
        }
    }
}