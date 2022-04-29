using JAZG.Model.Players;
using Mars.Components.Environments.Cartesian;

namespace JAZG.Model.Objects
{
    public class Food : Item
    {
        private int EnergyPoints = 2;
        public Food(FieldLayer layer) : base(layer)
        {
        }

        public override bool IsRoutable(ICharacter character)
        {
            return true;
        }

        public override CollisionKind? HandleCollision(ICharacter character)
        {
            if (character is Human)
            {
                var human = (Human) character;
                human.Energy += EnergyPoints;
                //TODO remove food from environment
                return CollisionKind.Remove;
            }

            return CollisionKind.Pass;
        }

        public override VisibilityKind? HandleExploration(ICharacter explorer)
        {
            return VisibilityKind.Opaque;
        }
    }
}