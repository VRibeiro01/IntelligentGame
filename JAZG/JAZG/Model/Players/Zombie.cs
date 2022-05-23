using System;
using System.Linq;
using Mars.Common;
using Mars.Numerics;
using ServiceStack;

namespace JAZG.Model.Players
{
    public class Zombie : Player
    {
        private int _lastAction;

        private static int _level { get; set; } = 1;

        public override void Init(FieldLayer layer)
        {
            base.Init(layer);
            Energy = 15;
        }

        public override void Tick()
        {
            if (Energy <= 0) Kill();
            var nearestHuman = Layer.Environment.Characters.Where(h => h.GetType() == typeof(Human))
                .OrderBy(hD => Distance.Chebyshev(Position.PositionArray, hD.Position.PositionArray)).FirstOrDefault();

            if (nearestHuman != null)
            {
                var humanDistance = GetDistanceFromPlayer(nearestHuman);

                if (humanDistance <= 2)
                {
                    EatHuman(nearestHuman);
                    //Console.WriteLine("Chomp, chomp!");
                }
                else if (humanDistance <= _level * 10)
                {
                    MoveTowardsHuman(nearestHuman);
                    if (_lastAction != 2)
                    {
                       // Console.WriteLine("Braaaaaains!");
                        _lastAction = 2;
                    }
                }
                else
                {
                    RandomMove();
                    if (_lastAction != 1)
                    {
                        //Console.WriteLine("Brains?");
                        _lastAction = 1;
                    }
                }
            }
            else
            {
                RandomMove();
                if (_lastAction != 1)
                {
                    //Console.WriteLine("Brains?");
                    _lastAction = 1;
                }
            }
        }

        private void EatHuman(Player human)
        {
            Energy += 4;
            human.Kill();
        }

        public override void Kill()
        {
            base.Kill();
            Console.WriteLine("Zombie down!");
            if (AllZombiesDead())
            {
                Console.WriteLine("you got Level " + _level);
                _level += 1;
                Spawn();
            }
        }

        private void MoveTowardsHuman(Player human)
        {
            var distanceToHuman = GetDistanceFromPlayer(human);
            var directionToEnemy =
                PositionHelper.CalculateBearingCartesian
                    (Position.X, Position.Y, human.Position.X, human.Position.Y);
            Layer.Environment.Move(this, directionToEnemy, distanceToHuman > 2 ? Speed : 1);
        }

        private bool AllZombiesDead()
        {
            var erg = Layer.Environment.Characters.Where(h => h.GetType() == typeof(Zombie)).ToList().IsEmpty();
            return erg;
        }

        private void Spawn()
        {
            var neueZombie = Layer.AgentManager.Spawn<Zombie, FieldLayer>().ToList();
            foreach (var z in neueZombie)
            {
                z.Energy *= 2 * _level;
                z.Speed = _level;
            }
            Console.WriteLine("We created " + neueZombie.Count + " zombie agents." + "for level " + _level +
                              " with Energy " + neueZombie.First().Energy);
        }

        public static void HumanToZombie(FieldLayer layer, Player human)
        {
            var zombie = layer.AgentManager.Spawn<Zombie, FieldLayer>(null, z => { }).Take(1).First();
            zombie.Position = human.Position;
            Console.WriteLine("Human To Zombie at Position :" + zombie.Position);
        }
    }
}