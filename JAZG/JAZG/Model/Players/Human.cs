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
using ServiceStack.Text;

namespace JAZG.Model.Players
{
    public class Human : Player
    {
        private Geometry _boundaryBoxGeometry;

        // TODO: remove
        private int _lastAction;
        private int _maxSeeingDistance;
        public List<Weapon> weapons = new();
        public int BrainNr;

        // TODO: change to enum
        public int HasWeapon { get; set; }
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
            if (Layer.learningMode > 0)
            {
                var mindIndex = RandomHelper.Random.Next(Layer.amountOfMinds);
                Layer.QHumanLearningList[mindIndex].QMovement(this);
            }
            else
            {
                NonQMovement();
            }

            //TODO Search for food and weapons
            // TODO Where to go? Where to hide? When to rest? When to kill? 
            //TODO reduce speed when energy is reduced
        }

        public Zombie FindClosestZombie()
        {
            // Sichtfeld des Menschen einschränken
            return (Zombie) FindZombies().OrderBy(GetDistanceFromPlayer).FirstOrDefault();
        }

        public List<Player> FindZombies()
        {
            return Layer.Environment.Characters
                .Where(c => c is Zombie && GetDistanceFromPlayer(c) <= _maxSeeingDistance).ToList();
        }

        public Weapon FindClosestWeapon()
        {
            return (Weapon) Layer.Environment
                .ExploreObstacles(_boundaryBoxGeometry,
                    item => item is Weapon && GetDistanceFromItem(item) <= _maxSeeingDistance)
                .OrderBy(GetDistanceFromItem).FirstOrDefault();
        }

        public void RunFromZombies(Player closestZombie)
        {
            // TODO: do not run into walls
            var directionFromClosest = (GetDirectionToPlayer(closestZombie) + 180) % 360;
            var directionFromEnemies = directionFromClosest;
            var closestDistance = GetDistanceFromPlayer(closestZombie);
            var zombies = FindZombies();
            foreach (var zombie in zombies)
            {
                if (zombie == closestZombie) continue;
                var directionFromEnemy = (GetDirectionToPlayer(zombie) + 180) % 360;
                var directionToClosest = directionFromEnemy - directionFromClosest;
                directionFromEnemies =
                    (directionFromEnemies + closestDistance / GetDirectionToPlayer(zombie) * directionToClosest) % 360;
            }

            if (double.IsNaN(directionFromEnemies))
                directionFromEnemies = RandomHelper.Random.Next(360);


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
            var weaponIndex = weapons.FindIndex(e => e is M16);
            if (weaponIndex < 0)
            {
                weapons[weaponIndex].Use(zombie);
            }
            else
            {
                weapons[0].Use(zombie);
            }
        }

        public override void Kill()
        {
            base.Kill();
            Layer.HumansKilled++;
            Console.WriteLine("They got me! Leave me behind... arghhh!");
        }


        public void NonQMovement()
        {
            IsShooting = false;

            var nextZombie = FindClosestZombie();
            var nextWeapon = FindClosestWeapon();

            if (nextZombie != null)
            {
                var zombieDistance = GetDistanceFromPlayer(nextZombie);
                double weaponDistance = 999;

                if (zombieDistance <= 10)
                {
                    RunFromZombies(nextZombie);
                    Console.WriteLine("Running");
                    if (_lastAction != 2)
                    {
                        Console.WriteLine("The zombies are coming, run!!!");
                        _lastAction = 2;
                    }
                }
                else if (weapons.Count < 1 && nextWeapon != null)
                {
                    CollectItem(nextWeapon);
                    if (_lastAction != 4)
                    {
                        Console.WriteLine("I saw a weapon.");
                        _lastAction = 4;
                    }
                }
                else if (weapons.Count > 0)
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
                    if (weapons.Count < 1 && nextWeapon != null)
                    {
                        CollectItem(nextWeapon);
                        if (_lastAction != 4)
                        {
                            Console.WriteLine("I saw a weapon.");
                            _lastAction = 4;
                        }
                    }
                    else
                    {
                        RandomMove();
                        if (_lastAction != 1)
                        {
                            //Console.WriteLine("I walk.");
                            _lastAction = 1;
                        }
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