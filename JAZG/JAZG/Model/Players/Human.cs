using System;
using System.Collections.Generic;
using System.Linq;
using JAZG.Model.Objects;
using Mars.Common;
using Mars.Common.Core.Random;
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
        public bool WallCollision;
        public Wall BlockingWall;
        public int mindIndex;
       

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
            mindIndex = RandomHelper.Random.Next(Layer.amountOfMinds);
        }

        public override void Tick()
        {
            if (Layer.learningMode > 0)
            {
                Layer.QHumanLearningList[mindIndex].QMovement(this);
            }
            else
            {
                NonQMovement();
            }
        }

        public Zombie FindClosestZombie()
        {
            // Sichtfeld des Menschen einschränken
            return (Zombie)FindZombies().OrderBy(GetDistanceFromPlayer).FirstOrDefault();
        }

        public List<Player> FindZombies()
        {
            return Layer.Environment.Characters
                .Where(c => c is Zombie && GetDistanceFromPlayer(c) <= _maxSeeingDistance).ToList();
        }

        public Weapon FindClosestWeapon()
        {
            return (Weapon)Layer.Environment
                .ExploreObstacles(_boundaryBoxGeometry,
                    item => item is Weapon && GetDistanceFromItem(item) <= _maxSeeingDistance)
                .OrderBy(GetDistanceFromItem).FirstOrDefault();
        }

        public void RunFromZombies(Player closestZombie)
        {
            // TODO: do not run into walls
            var directionFromClosest = Modulo(GetDirectionToPlayer(closestZombie) + 180, 360);
            var directionFromEnemies = directionFromClosest;
            var closestDistance = GetDistanceFromPlayer(closestZombie);
            var zombies = FindZombies();
            foreach (var zombie in zombies)
            {
                if (zombie == closestZombie) continue;
                var directionFromEnemy = Modulo(GetDirectionToPlayer(zombie) + 180, 360);
                var directionToClosest = directionFromEnemy - directionFromClosest;
                directionFromEnemies =
                    Modulo(directionFromEnemies + closestDistance / GetDirectionToPlayer(zombie) * directionToClosest,
                        360);
            }

            if (double.IsNaN(directionFromEnemies))
                directionFromEnemies = RandomHelper.Random.Next(360);

            Layer.Environment.Move(this, directionFromEnemies, 2);

            if (WallCollision)
            {
                WallCollision = false;

                var wallBearing = BlockingWall.bearing;
                var otherBearing = (wallBearing + 180) % 360;
                var finalBearing =
                    BearingDif(directionFromEnemies, wallBearing) < BearingDif(directionFromClosest, otherBearing)
                        ? wallBearing
                        : otherBearing;

                Layer.Environment.Move(this, finalBearing, 2);
            }
        }

        public void CollectItem(Item item)
        {
            var distanceToItem = GetDistanceFromItem(item);
            var directionToItem = GetDirectionToItem(item);
            if (double.IsNaN(directionToItem)) directionToItem = RandomHelper.Random.Next(360);
            Layer.Environment.Move(this, directionToItem, distanceToItem < 2 ? distanceToItem : 2);
        }

        public bool UseWeapon(Zombie zombie)
        {
            if (weapons.Count == 0)
            {
                Console.WriteLine("I don't have a weapon");
                return false;
            }

            var weaponIndex = weapons.FindIndex(e => e is M16);
            if (hasM16())
            {
                return weapons[weaponIndex].Use(zombie);
            }

            return weapons[0].Use(zombie);
        }

        public bool hasM16()
        {
            var m16 = weapons.Where(e => e is M16).ToList();
            if (m16.Count > 0)
            {
                return true;
            }

            return false;
        }

        protected internal override void Kill()
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
                    IsShooting = UseWeapon(nextZombie);
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

        private static double Modulo(double a, double b)
        {
            return (a % b + b) % b;
        }

        private static double BearingDif(double a, double b)
        {
            var dif = Math.Abs(a - b) % 360;
            if (dif > 180) dif = 360 - dif;
            return dif;
        }
    }
}