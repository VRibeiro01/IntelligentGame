using System;
using System.Linq;
using JAZG.Model.Objects;
using Mars.Common;
using Mars.Common.Core.Random;
using Mars.Common.IO.Mapped.Collections;
using Mars.Numerics;

using Mars.Components.Environments.Cartesian;

namespace JAZG.Model.Players
{
    public class Human : Player
    {
        private bool _dead;
        // TODO: remove
        private int _lastAction = 0;

        public List<Weapon> weapons = new();

        public override void Init(FieldLayer layer)
        {
            base.Init(layer);
            Energy = 30;
        }
        
        public override void Tick()
        {
            base.Tick();
        
            // Bewegt sich randomly
            // Wenn er Zombie sieht --> Weg vom Zombie
            
            var nextZombie = Layer.Environment.Characters.Where(c => c.GetType() == typeof(Zombie))
                .OrderBy(zombie => Distance.Chebyshev(Position.PositionArray, zombie.Position.PositionArray))
                .FirstOrDefault();

            if (nextZombie != null)
            {
                var zombieDistance = (int) Distance.Chebyshev(
                    Position.PositionArray, nextZombie.Position.PositionArray);

                if (zombieDistance <= 30)
                {
                    RunFromZombie(nextZombie);
                    if (_lastAction != 2)
                    {
                        Console.WriteLine("Ah, Zombies!!!");
                        _lastAction = 2;
                    }
                }
                else
                {
                    RandomMove();
                    if (_lastAction != 1)
                    {
                        Console.WriteLine("I walk.");
                        _lastAction = 1;
                    }
                }
            }
            // TODO: remove duplicate
            else
            {
                RandomMove();
                if (_lastAction != 1)
                {
                    Console.WriteLine("I walk.");
                    _lastAction = 1;
                }
            }

            // TODO implement action upon meting zombie using collisionHashEnvironment  functionalities
        }

        private void RunFromZombie(Player zombie)
        {
            var directionToEnemy = PositionHelper.CalculateBearingCartesian(
                Position.X, Position.Y, zombie.Position.X, zombie.Position.Y);
            if (double.IsNaN(directionToEnemy)) directionToEnemy = RandomHelper.Random.Next(360);
            var directionOpposite = (directionToEnemy + 180) % 360;

            //Console.WriteLine("Moving from " + Position + " in direction of " + directionOpposite);

            // TODO: implement speed
            Layer.Environment.Move(this, directionOpposite, 2);
        }

        public override void Kill()
        {
            if (_dead) return;
            _dead = true;
            base.Kill();
            Console.WriteLine("They got me! Leave me behind... arghhh!");
        }
        public override CollisionKind? HandleCollision(ICharacter other)
        {
            // Dummy Implementierung
            return CollisionKind.Pass;
        }
    }
}