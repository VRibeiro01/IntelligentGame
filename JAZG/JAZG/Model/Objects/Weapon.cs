using Mars.Components.Environments.Cartesian;

namespace JAZG.Model.Objects
{
    public class Weapon : Item
    {
        public override bool IsRoutable(ICharacter character)
        {
            return true;
        }

        public override CollisionKind? HandleCollision(ICharacter character)
        {
            return CollisionKind.Pass;
        }

        public override VisibilityKind? HandleExploration(ICharacter explorer)
        {
            return VisibilityKind.Opaque;
        }
    }
}