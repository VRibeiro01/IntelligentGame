using System;
using Mars.Components.Environments.Cartesian;

namespace JAZG.Model.Objects
{
    public abstract class Item : IObstacle
    {
        public Guid ID { get; set; }
        public bool IsRoutable(ICharacter character)
        {
            throw new NotImplementedException();
        }

        public CollisionKind? HandleCollision(ICharacter character)
        {
            throw new NotImplementedException();
        }

        public VisibilityKind? HandleExploration(ICharacter explorer)
        {
            throw new NotImplementedException();
        }
    }
}