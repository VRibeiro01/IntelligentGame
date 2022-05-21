using System;
using Mars.Components.Environments.Cartesian;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Layers;

namespace JAZG.Model.Objects
{
    public abstract class Item : IAgent<FieldLayer>, IObstacle, IPositionable
    {
        protected static FieldLayer Layer;

        protected Item(FieldLayer layer)
        {
            Layer = layer;
        }

        [PropertyDescription] public UnregisterAgent UnregisterHandle { get; set; }


        public Guid ID { get; set; }

        public virtual void Init(FieldLayer layer)
        {
            Layer = layer;
        }

        public void Tick()
        {
            //do nothing
        }

        public abstract bool IsRoutable(ICharacter character);

        public abstract CollisionKind? HandleCollision(ICharacter character);

        public abstract VisibilityKind? HandleExploration(ICharacter explorer);

        public Position Position { get; set; }
    }
}