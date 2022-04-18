using System;
using Mars.Components.Environments.Cartesian;
using NetTopologySuite.Geometries;

namespace JAZG.Model.Objects
{
    public abstract class Item : IObstacle
    {
        public Guid ID { get; set; }
       

        public abstract bool IsRoutable(ICharacter character);


        public abstract CollisionKind? HandleCollision(ICharacter character);


        public abstract VisibilityKind? HandleExploration(ICharacter explorer);

    }
}