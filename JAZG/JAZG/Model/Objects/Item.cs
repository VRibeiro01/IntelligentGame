using System;
using Mars.Components.Environments.Cartesian;
using Mars.Interfaces.Environments;

namespace JAZG.Model.Objects
{
    public abstract class Item : IObstacle, IPositionable
    {
        public Guid ID { get; set; }


        public abstract bool IsRoutable(ICharacter character);


        public abstract CollisionKind? HandleCollision(ICharacter character);


        public abstract VisibilityKind? HandleExploration(ICharacter explorer);

        public Position Position { get; set; }

        public static FieldLayer Layer;

        public Item(FieldLayer layer)
        {
            Item.Layer = layer;
        }
    }
}