using System;
using Mars.Components.Environments.Cartesian;
using Mars.Interfaces.Agents;
using NetTopologySuite.Geometries;
using Position = Mars.Interfaces.Environments.Position;

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