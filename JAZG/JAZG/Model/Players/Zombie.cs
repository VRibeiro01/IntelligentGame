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

        private static int Level { get; set; } = 1;

        public override void Init(FieldLayer layer)
        {
            base.Init(layer);
            Energy = 15;
        }

        public override void Tick()
        {
            if (Energy <= 0) Kill();
            var nearestHuman = Layer.Environment.Characters
                .Where(h => h.GetType() == typeof(Human) || h.GetType() == typeof(CustomHuman))
                .OrderBy(hD => Distance.Chebyshev(Position.PositionArray, hD.Position.PositionArray)).FirstOrDefault();

            if (nearestHuman != null)
            {
                var humanDistance = GetDistanceFromPlayer(nearestHuman);

                if (humanDistance <= 2)
                {
                    EatHuman(nearestHuman);
                    //Console.WriteLine("Chomp, chomp!");
                }
                else if (humanDistance <= Level * 20)
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
            human.Kill();
            Energy += 4;
        }

        protected internal override void Kill()
        {
            base.Kill();
            Layer.ZombiesKilled++;
            Console.WriteLine("Zombie down!");
            if (AllZombiesDead())
            {
                Console.WriteLine("you got Level " + Level);
                Level += 1;
                Spawn();
            }
        }

        private void MoveTowardsHuman(Player human)
        {
            var distanceToHuman = GetDistanceFromPlayer(human);
            var directionToEnemy =
                PositionHelper.CalculateBearingCartesian
                    (Position.X, Position.Y, human.Position.X, human.Position.Y);
            Layer.Environment.Move(this, directionToEnemy, distanceToHuman > 2 ? Speed : distanceToHuman);
        }

        private bool AllZombiesDead()
        {
            var erg = Layer.Environment.Characters.Where(h => h.GetType() == typeof(Zombie)).ToList().IsEmpty();
            return erg;
        }

        private void Spawn()
        {
            var newZombies = Layer.AgentManager.Spawn<Zombie, FieldLayer>().ToList();
            foreach (var z in newZombies)
            {
                z.Energy *= 2 * Level;
                z.Speed = Level;
            }

            Console.WriteLine("We created " + newZombies.Count + " zombie agents." + "for level " + Level +
                              " with Energy " + newZombies.First().Energy);
            Layer.ZombiesSpawned += newZombies.Count;
        }

        public static void HumanToZombie(FieldLayer layer, Player human)
        {
            var zombie = layer.AgentManager.Spawn<Zombie, FieldLayer>(null, z => { }).Take(1).First();
            zombie.Position = human.Position;
            layer.ZombiesSpawned++;
            Console.WriteLine("Human To Zombie at Position :" + zombie.Position);
        }
    }
}