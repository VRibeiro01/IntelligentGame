using System;
using System.Linq;
using Mars.Common;
using Mars.Numerics;

namespace JAZG.Model.Players
{
    public class Zombie : Player
    {
        public override void Init(FieldLayer layer)
        {
            base.Init(layer);
            Energy = 15;
        }

        public override void Tick()
        {
           
            // TODO Implement simple movements
            // TODO implement action upon meeting zombie using collisionHashEnvironment  functionalities
            base.Tick();
            var nahrestHuman = Layer.Environment.Characters.Where(h => h.GetType() == typeof(Human))
                .OrderBy(hD => Distance.Chebyshev(Position.PositionArray, hD.Position.PositionArray)).FirstOrDefault();
            if (nahrestHuman != null)
            {
                var humanDistance = (int)Distance.Chebyshev(
                    Position.PositionArray, nahrestHuman.Position.PositionArray);
                if (humanDistance <= 2)
                {
                    EatHuman(nahrestHuman);
                    Console.WriteLine("I eat human");
                }
                else if (humanDistance <= 10)
                {
                    MoveTowardsHuman(nahrestHuman);
                    Console.WriteLine("I Walk to my food");

                }
                else
                {
                    RandomMove();
                 //   Console.WriteLine("Iwalk without goal ");

                }
            }
        }
        private void EatHuman(Player human)
        {
            Energy += 4;
            // human.Kill()
            // TO DO Kill
        }
        private void MoveTowardsHuman(Player human)
        {
            var directionToEnemy =
                PositionHelper.CalculateBearingCartesian
                    (Position.X, Position.Y, human.Position.X, human.Position.Y);
            Layer.Environment.Move(this, directionToEnemy, 3);
        }
    }
}