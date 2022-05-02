using System;
using System.Linq;
using JAZG.Model.Objects;
using Mars.Common;
using Mars.Common.Core.Random;
using Mars.Common.IO.Mapped.Collections;
using Mars.Components.Environments.Cartesian;
using Mars.Interfaces.Environments;
using Mars.Numerics;

namespace JAZG.Model.Players
{
    public class Human : Player
    {
        private bool _dead;

        // TODO: remove
        private int _lastAction;


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
            
            //TODO Search for food and weapons
            //TODO Use weapons to kill zombie
            // TODO Where to go? Where to hide? When to rest? When to kill? 
            //TODO reduce speed when energy is reduced
            

            var nextZombie = (Zombie)Layer.Environment.Characters.Where(c => c.GetType() == typeof(Zombie))
                .OrderBy(zombie => Distance.Chebyshev(Position.PositionArray, zombie.Position.PositionArray))
                .FirstOrDefault();

            if (nextZombie != null)
            {
                var zombieDistance = (int) Distance.Chebyshev(
                    Position.PositionArray, nextZombie.Position.PositionArray);

                if (zombieDistance <= 10)
                {
                    RunFromZombie(nextZombie);
                    if (_lastAction != 2)
                    {
                        Console.WriteLine("The zombies are coming, run!!!");
                        _lastAction = 2;
                    }
                }
                else if (zombieDistance <= 30 && weapons.Count > 0)
                {
                    UseWeapon(nextZombie);
                    if (_lastAction != 3)
                    {
                        Console.WriteLine("Ah, zombies!!!");
                        _lastAction = 3;
                    }
                    Console.WriteLine("Die zombie!!!");
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

            
        }

        private void RunFromZombie(Player zombie)
        {
            var directionToEnemy = PositionHelper.CalculateBearingCartesian(
                Position.X, Position.Y, zombie.Position.X, zombie.Position.Y);
            if (double.IsNaN(directionToEnemy)) directionToEnemy = RandomHelper.Random.Next(360);
            var directionOpposite = (directionToEnemy + 180) % 360;

            //Console.WriteLine("Moving from " + Position + " in direction of " + directionOpposite);

            
            Layer.Environment.Move(this, directionOpposite, Speed);
        }

        private void UseWeapon(Zombie zombie)
        {
            weapons[0].Use(zombie);
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