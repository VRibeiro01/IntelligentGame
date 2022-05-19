using System;
using System.Collections.Generic;
using System.Linq;
using JAZG.Model.Learning;
using JAZG.Model.Objects;
using Mars.Common;
using Mars.Common.Core.Random;
using Mars.Numerics;
using NetTopologySuite.Geometries;

namespace JAZG.Model.Players
{
    public class Human : Player
    {
        // TODO: remove
        private int _lastAction;
        private Geometry _boundaryBoxGeometry;
        private QHumanLearning _qHumanLearning;
        private int _maxSeeingDistance;

        // TODO: change to enum
        public bool HasWeapon { get; set; }
        public bool IsShooting { get; set; }
        public List<Weapon> weapons = new();

        public override void Init(FieldLayer layer)
        {
            base.Init(layer);
            Coordinate[] coordinates =
            {
                layer.Extent.LowerLeft.ToCoordinate(), layer.Extent.LowerRight.ToCoordinate(),
                layer.Extent.UpperRight.ToCoordinate(), layer.Extent.UpperLeft.ToCoordinate(),
                layer.Extent.LowerLeft.ToCoordinate()
            };
            _boundaryBoxGeometry = new Polygon(new LinearRing(coordinates));
            Energy = 30;
            _maxSeeingDistance = 20;
            _qHumanLearning = new QHumanLearning(_maxSeeingDistance);
        }

        public override void Tick()
        {
            base.Tick();
            ExploreZombies();
            RandomMove();
            // QMovement();
            //NonQMovement();

            //TODO Search for food and weapons
            // TODO Where to go? Where to hide? When to rest? When to kill? 
            //TODO reduce speed when energy is reduced
        }

        private Zombie FindClosestZombie()
        {
            // Sichtfeld des Menschen einschränken
            return (Zombie) Layer.Environment.Characters.Where(c =>
                    c.GetType() == typeof(Zombie) &&
                    Distance.Chebyshev(Position.PositionArray, c.Position.PositionArray) <= _maxSeeingDistance)
                .OrderBy(zombie => Distance.Chebyshev(Position.PositionArray, zombie.Position.PositionArray))
                .FirstOrDefault();
        }

        private List<Player> FindZombies()
        {
            return Layer.Environment.Characters.Where(c =>
                    c.GetType() == typeof(Zombie) &&
                    Distance.Chebyshev(Position.PositionArray, c.Position.PositionArray) <= _maxSeeingDistance)
                .OrderBy(zombie => Distance.Chebyshev(Position.PositionArray, zombie.Position.PositionArray)).ToList();
        }

        public List<Player> ExploreZombies()
        {
            var conePosition = Position.Copy();
            var conePosition2 = Position.Copy();

            var seeingAngleToRad = 60.0 * (Math.PI/180.0);
            var seeingAngleToRad2 = 300.0 * (Math.PI/180.0);
            
            conePosition.X = conePosition.X + (_maxSeeingDistance * Math.Cos(seeingAngleToRad));
            conePosition.Y = conePosition.Y + (_maxSeeingDistance * Math.Sin(seeingAngleToRad));
          
            conePosition2.X = conePosition2.X + (_maxSeeingDistance * Math.Cos(seeingAngleToRad2));
            conePosition2.Y = conePosition2.Y + (_maxSeeingDistance * Math.Sin(seeingAngleToRad2));
            
            Coordinate[] coordinates =
            {
                Position.ToCoordinate(), conePosition.ToCoordinate(),
                conePosition2.ToCoordinate(), Position.ToCoordinate()
            };
            var cone = new Polygon(new LinearRing(coordinates));
            List<Player> res = Layer.Environment
                .ExploreCharacters(this, cone, player => player.GetType() == typeof(Zombie)).ToList();
            
            Console.WriteLine("Zombies in sight... ");
            foreach (var zomb in res)
            {
                Console.WriteLine("Distance from zombie: " + Distance.Chebyshev(Position.PositionArray,zomb.Position.PositionArray));
            }
            return res;
        }

        private Weapon FindClosestWeapon()
        {
            return (Weapon) Layer.Environment
                .ExploreObstacles(_boundaryBoxGeometry, item => item is Weapon).OrderBy(item =>
                    Distance.Chebyshev(Position.PositionArray, item.Position.PositionArray)).FirstOrDefault();
        }

        private void RunFromZombie(Player zombie)
        {
            // TODO: run from all zombies that are near
            var directionToEnemy = GetDirectionToPlayer(zombie);
            if (double.IsNaN(directionToEnemy)) directionToEnemy = RandomHelper.Random.Next(360);
            var directionOpposite = (directionToEnemy + 180) % 360;
            Layer.Environment.Move(this, directionOpposite, 2);
        }

        private void RunFromZombies(Player closestZombie)
        {
            var directionFromClosest = (GetDirectionToPlayer(closestZombie) + 180) % 360;
            var directionFromEnemies = directionFromClosest;
            var closestDistance = GetDistanceFromPlayer(closestZombie);
            var zombies = FindZombies();
            foreach (var zombie in zombies)
            {
                if (zombie == closestZombie) continue;
                var directionFromEnemy = (GetDirectionToPlayer(zombie) + 180) % 360;
                var directionToClosest = Math.Abs(directionFromClosest - directionFromEnemy);
                directionFromEnemies += Math.Abs(directionFromEnemy -
                                                 closestDistance / GetDistanceFromPlayer(zombie) * directionToClosest);
            }

            directionFromEnemies /= zombies.Count;
            Layer.Environment.Move(this, directionFromEnemies, 2);
        }

        private void CollectItem(Item item)
        {
            var distanceToItem = GetDistanceFromItem(item);
            var directionToItem = GetDirectionToItem(item);
            if (double.IsNaN(directionToItem)) directionToItem = RandomHelper.Random.Next(360);
            Layer.Environment.Move(this, directionToItem, distanceToItem < 2 ? distanceToItem : 2);
        }

        private void UseWeapon(Zombie zombie)
        {
            weapons[0].Use(zombie);
        }

        public override void Kill()
        {
            base.Kill();
            Console.WriteLine("They got me! Leave me behind... arghhh!");
        }


        public void NonQMovement()
        {
            var nextZombie = FindClosestZombie();
            var nextWeapon = FindClosestWeapon();

            if (nextZombie != null)
            {
                double zombieDistance = GetDistanceFromPlayer(nextZombie);
                double weaponDistance = 999;
                if (nextWeapon != null) weaponDistance = GetDistanceFromItem(nextWeapon);

                if (zombieDistance <= 10)
                {
                    //RunFromZombie(nextZombie);
                    RunFromZombies(nextZombie);
                    if (_lastAction != 2)
                    {
                        Console.WriteLine("The zombies are coming, run!!!");
                        _lastAction = 2;
                    }
                }
                else if (weapons.Count < 1 && weaponDistance <= 20)
                {
                    CollectItem(nextWeapon);
                    if (_lastAction != 4)
                    {
                        Console.WriteLine("I saw a weapon.");
                        _lastAction = 4;
                    }
                }
                else if (weapons.Count > 0 && zombieDistance <= 20)
                {
                    UseWeapon(nextZombie);
                    IsShooting = true;
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

        // Wenn Zombies gesehen werde und Waffe vorhanden ist: Entscheidung ob laufen oder schießen anhand des QLearning-Algorithmus
        public void QMovement()
        {
            var closestZombie = FindClosestZombie();
            var nextWeapon = FindClosestWeapon();


            if (closestZombie != null)
            {
                if (weapons.Count > 1)
                {
                    // TODO: wie wird state ermittelt
                    // Distanz zum nächsten Zombie, Anzahl Zombies,
                    // wo befinden sich Zombies ("entweder umzingelt oder nicht" oder "zonen mit zombies")
                    int state = (int) GetDistanceFromPlayer(closestZombie);
                    Console.WriteLine("State: " + state);

                    // action anhand der Q-Werte für Aktionen im aktuellen Zustand
                    // Wahrscheinlichkeit nach Roulette Wheel Policy
                    int action = QHumanLearning.QLearning.GetAction(state);
                    Act(action, closestZombie);

                    int nextState = (int) GetDistanceFromPlayer(closestZombie);
                    QHumanLearning.QLearning.UpdateState(state, action, Reward(closestZombie, state), nextState);
                }
                else
                {
                    RunFromZombie(closestZombie);
                }
            }
            else if (weapons.Count < 1 && GetDistanceFromItem(nextWeapon) < 20)
            {
                CollectItem(nextWeapon);
            }
            else
            {
                RandomMove();
            }
        }

        // Übersetzung des Action-Index in Aktion
        public void Act(int actionIndex, Zombie closestZombie)
        {
            Console.WriteLine("action index: " + actionIndex);
            switch (actionIndex)
            {
                case > 0:
                    UseWeapon(closestZombie);
                    break;

                default:
                    RunFromZombie(closestZombie);
                    break;
            }
        }

        // Berechnet die Bewertungsfunktion
        // TODO Wie soll die Bewertung sein?
        // GUT: Zombie tot, weniger Zombies in Sichtfeld, neue Distanz zum nächsten Zombie kleiner
        // Schlecht: Zombie lebt noch, mehr Zombies in Sichtfeld, Distanz zum nächsten Zombie kleiner
        public double Reward(Zombie closestZombie, int oldDistance)
        {
            if (closestZombie.Dead)
            {
                return 1.0;
            }

            Player newClosestZombie = FindClosestZombie();
            if (GetDistanceFromPlayer(newClosestZombie) > oldDistance)
            {
                return 0.5;
            }

            return 0.0;
        }
    }
}