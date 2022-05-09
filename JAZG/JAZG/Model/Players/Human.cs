using System;
using System.Linq;
using JAZG.Model.Objects;
using Mars.Common;
using Mars.Common.Core.Random;
using Mars.Common.IO.Mapped.Collections;
using Mars.Numerics;
using NetTopologySuite.Geometries;

namespace JAZG.Model.Players
{
    public class Human : Player
    {
        // TODO: remove
        private int _lastAction;
        private Geometry _boundaryBoxGeometry;

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
        }

        public override void Tick()
        {
            base.Tick();

            //TODO Search for food and weapons
            //TODO Use weapons to kill zombie
            // TODO Where to go? Where to hide? When to rest? When to kill? 
            //TODO reduce speed when energy is reduced

            var nextZombie = FindClosestZombie();
            var nextWeapon = FindClosestWeapon();

            if (nextZombie != null)
            {
                double zombieDistance = GetDistanceFromPlayer(nextZombie);
                double weaponDistance = 999;
                if (nextWeapon != null) weaponDistance = GetDistanceFromItem(nextWeapon);

                if (zombieDistance <= 10)
                {
                    RunFromZombie(nextZombie);
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

        private Zombie FindClosestZombie()
        {
            return (Zombie)Layer.Environment.Characters.Where(c => c.GetType() == typeof(Zombie))
                .OrderBy(zombie => Distance.Chebyshev(Position.PositionArray, zombie.Position.PositionArray))
                .FirstOrDefault();
        }

        private Weapon FindClosestWeapon()
        {
            return (Weapon)Layer.Environment
                .ExploreObstacles(_boundaryBoxGeometry, item => item is Weapon).OrderBy(item =>
                    Distance.Chebyshev(Position.PositionArray, item.Position.PositionArray)).FirstOrDefault();
            /*var enumerable = Layer.Environment.ExploreObstacles(_boundaryBoxGeometry, item => item is Gun);
            enumerable = enumerable.OrderBy(item => Distance.Chebyshev(Position.PositionArray, item.Position.PositionArray));
            return (Weapon)enumerable.FirstOrDefault();*/
        }

        private void RunFromZombie(Player zombie)
        {
            var directionToEnemy = GetDirectionToPlayer(zombie);
            if (double.IsNaN(directionToEnemy)) directionToEnemy = RandomHelper.Random.Next(360);
            var directionOpposite = (directionToEnemy + 180) % 360;
            Layer.Environment.Move(this, directionOpposite, 2);
        }
        
        private void CollectItem(Item item)
        {
            var distanceToItem = GetDistanceFromItem(item);
            var directionToItem = GetDirectionToItem(item);
            if (double.IsNaN(directionToItem)) directionToItem = RandomHelper.Random.Next(360);
            Layer.Environment.Move(this, directionToItem, distanceToItem < 2? distanceToItem : 2);
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
    }
}