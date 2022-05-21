using System;
using System.Collections.Generic;
using System.Linq;
using JAZG.Model.Learning;
using JAZG.Model.Objects;
using Mars.Common;
using Mars.Common.Core.Random;
using Mars.Components.Services.Learning;
using Mars.Numerics;
using NetTopologySuite.Geometries;

namespace JAZG.Model.Players
{
    public class Human : Player
    {
        private Geometry _boundaryBoxGeometry;

        // TODO: remove
        private int _lastAction;
        private int _maxSeeingDistance;
        public List<Weapon> weapons = new();

        // TODO: change to enum
        public bool HasWeapon { get; set; }
        public bool IsShooting { get; set; }
        

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
        }

        public override void Tick()
        {
            base.Tick();
            //qLearning.QMovement();
            NonQMovement();

            //TODO Search for food and weapons
            // TODO Where to go? Where to hide? When to rest? When to kill? 
            //TODO reduce speed when energy is reduced
        }

        public Zombie FindClosestZombie()
        {
            // Sichtfeld des Menschen einschränken
            return (Zombie) ExploreZombies()
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

            // human can see a cone shape that's 120 degrees across
            var seeingAngleToRad = 60.0 * (Math.PI / 180.0);
            var seeingAngleToRad2 = 300.0 * (Math.PI / 180.0);

            conePosition.X = conePosition.X + _maxSeeingDistance * Math.Cos(seeingAngleToRad);
            conePosition.Y = conePosition.Y + _maxSeeingDistance * Math.Sin(seeingAngleToRad);

            conePosition2.X = conePosition2.X + _maxSeeingDistance * Math.Cos(seeingAngleToRad2);
            conePosition2.Y = conePosition2.Y + _maxSeeingDistance * Math.Sin(seeingAngleToRad2);

            Coordinate[] coordinates =
            {
                Position.ToCoordinate(), conePosition.ToCoordinate(),
                conePosition2.ToCoordinate(), Position.ToCoordinate()
            };
            var cone = new Polygon(new LinearRing(coordinates));
            var res = Layer.Environment
                .ExploreCharacters(this, cone, player => player.GetType() == typeof(Zombie)).ToList();

            return res;
        }

        public Weapon FindClosestWeapon()
        {
            return (Weapon) Layer.Environment
                .ExploreObstacles(_boundaryBoxGeometry, item => item is Weapon).OrderBy(item =>
                    Distance.Chebyshev(Position.PositionArray, item.Position.PositionArray)).FirstOrDefault();
        }

        public void RunFromZombie(Player zombie)
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

        public void CollectItem(Item item)
        {
            var distanceToItem = GetDistanceFromItem(item);
            var directionToItem = GetDirectionToItem(item);
            if (double.IsNaN(directionToItem)) directionToItem = RandomHelper.Random.Next(360);
            Layer.Environment.Move(this, directionToItem, distanceToItem < 2 ? distanceToItem : 2);
        }

        public void UseWeapon(Zombie zombie)
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
                var zombieDistance = GetDistanceFromPlayer(nextZombie);
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
    }
}