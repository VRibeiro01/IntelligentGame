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
        // ****** Attributes
        public bool Dead;
        [PropertyDescription] public UnregisterAgent UnregisterHandle { get; set; }

        public FieldLayer Layer { get; set; }

        public int Energy { get; set; }
        protected int Speed { get; set; }

        public Guid ID { get; set; }

        public virtual void Init(FieldLayer layer)
        {
            Layer = layer;
            // If position not null, set position to a random point in layer
            Position ??= layer.FindRandomPosition();

            Speed = 1;

            // All players have same extent
            Extent = 1;

            while (!Layer.Environment.Insert(this, Position)) Position = layer.FindRandomPosition();
        }

        public virtual void Tick()
        {
        }

        public Position Position { get; set; }
        public double Extent { get; set; }


        public CollisionKind? HandleCollision(ICharacter other)
        {
            // Dummy Implementierung
            return CollisionKind.Pass;
        }

        public void RandomMove()
        {
            var bearing = RandomHelper.Random.Next(360);
            Layer.Environment.Move(this, bearing, Speed);
        }

        protected internal virtual void Kill()
        {
            if (Dead) return;
            Dead = true;
            Layer.Environment.Remove(this);
            UnregisterHandle.Invoke(Layer, this);
            if (GetType() == typeof(Zombie))
                DeadPlayer.Spawn(Layer, this);
            else
                Zombie.HumanToZombie(Layer, this);
        }

        public double GetDistanceFromPlayer(Player other)
        {
            if (other is null) return 999;

            return Distance.Chebyshev(
                Position.PositionArray, other.Position.PositionArray);
        }

        public double GetDistanceFromItem(Item item)
        {
            if (item is null) return 999;

            return Distance.Chebyshev(
                Position.PositionArray, item.Position.PositionArray);
        }

        public double GetDirectionToPlayer(Player other)
        {
            return PositionHelper.CalculateBearingCartesian(
                Position.X, Position.Y, other.Position.X, other.Position.Y);
        }

        public double GetDirectionToItem(Item item)
        {
            return PositionHelper.CalculateBearingCartesian(
                Position.X, Position.Y, item.Position.X, item.Position.Y);
        }
    }
}