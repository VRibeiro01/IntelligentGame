using System;
using Mars.Components.Environments.Cartesian;
using Mars.Interfaces.Annotations;
using NetTopologySuite.Geometries;
using Position = Mars.Interfaces.Environments.Position;

namespace JAZG.Model.Objects
{
    public class Wall : Item
    {
        public Wall() : base(Layer)
        {
        }

        [PropertyDescription(Name = "xleft")] public int xLeft { get; set; }

        [PropertyDescription(Name = "yleft")] public int yLeft { get; set; }

        [PropertyDescription(Name = "xright")] public int xRight { get; set; }

        [PropertyDescription(Name = "yright")] public int yRight { get; set; }

        public override void Init(FieldLayer layer)
        {
            base.Init(layer);
            Coordinate[] coordinates =
            {
                new(xLeft, yLeft), new(xRight, yRight)
            };
            Layer.Environment.Insert(this, new LineString(coordinates));
            Position = Position.CreatePosition((xLeft + xRight) / 2, (yLeft + yRight) / 2);
            Console.Write("New Wall at: " + "(" + xLeft + ";" + yLeft + ")" + "(" + xRight + ";" + yRight + ")");
        }

        public override bool IsRoutable(ICharacter character)
        {
            // this means that the wall cannot be passed
            return false;
        }

        public override CollisionKind? HandleCollision(ICharacter character)
        {
            Console.WriteLine("STOP. This is a wall!");
            return CollisionKind.Block;
        }

        public override VisibilityKind? HandleExploration(ICharacter explorer)
        {
            return VisibilityKind.Opaque;
        }
    }
}