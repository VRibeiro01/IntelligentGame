using System;
using Mars.Components.Environments.Cartesian;

namespace JAZG.Model.Objects
{
    public class Wall : Item
    {
        public Wall() : base(Layer)
        {
        }

        public override bool IsRoutable(ICharacter character)
        {
            // this means that the wall cannot be passed (??? right ??)
            return false;
        }

        public override CollisionKind? HandleCollision(ICharacter character)
        {
            return CollisionKind.Block;
        }

        public override VisibilityKind? HandleExploration(ICharacter explorer)
        {
            return VisibilityKind.Opaque;
        }
    }
}