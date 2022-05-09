using System;
using Mars.Components.Environments.Cartesian;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Layers;
using Position = Mars.Interfaces.Environments.Position;

namespace JAZG.Model.Objects
{
    public abstract class Item : IAgent<FieldLayer>, IObstacle, IPositionable
    {
        [PropertyDescription] public UnregisterAgent UnregisterHandle { get; set; }
        
        protected static FieldLayer Layer;

        protected Item(FieldLayer layer)
        {
            Layer = layer;
        }

        public Guid ID { get; set; }

        public abstract bool IsRoutable(ICharacter character);

        public abstract CollisionKind? HandleCollision(ICharacter character);

        public abstract VisibilityKind? HandleExploration(ICharacter explorer);

        public Position Position { get; set; }
        
        public virtual void Init(FieldLayer layer)
        {
            Layer = layer;
        }

        public void Tick()
        {
            //do nothing
        }
    }
}