﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using JAZG.Model.Learning;
using JAZG.Model.Objects;
using Mars.Common;
using Mars.Common.Collections;
using Mars.Common.Core.Random;
using Mars.Components.Environments.Cartesian;
using Mars.Numerics;
using NetTopologySuite.Geometries;
using GeometryFactory = Mars.Components.Environments.Cartesian.GeometryFactory;

namespace JAZG.Model.Players
{
    public class Human : Player
    {
        // TODO: remove
        private int _lastAction;
        private Geometry _boundaryBoxGeometry;
        private QHumanLearning _qHumanLearning;
        private int maxSeeingDistance;
       

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
            maxSeeingDistance = 40;
            _qHumanLearning = new QHumanLearning(maxSeeingDistance);

        }

        public override void Tick()
        {
            base.Tick();
            //QMovement();
            NonQMovement();

            //TODO Search for food and weapons
            //TODO Use weapons to kill zombie
            // TODO Where to go? Where to hide? When to rest? When to kill? 
            //TODO reduce speed when energy is reduced
        }

        private Zombie FindClosestZombie()
        {
            // Sichtfeld des Menschen einschränken
            return (Zombie)Layer.Environment.Characters.Where(c => c.GetType() == typeof(Zombie) && Distance.Chebyshev(Position.PositionArray, c.Position.PositionArray) <=maxSeeingDistance)
                .OrderBy(zombie => Distance.Chebyshev(Position.PositionArray, zombie.Position.PositionArray))
                .FirstOrDefault();
        }
        //TODO find out why no zombies are found
        public List<Player> ExploreZombies()
        {
            ConeExplorationView explorationView = new ConeExplorationView();
            explorationView.Bearing = 90.0;
            explorationView.Range = 99.0;
            explorationView.Source = new double[] {Position.X, Position.Y};
            Polygon cone = GeometryFactory.CreateCone(explorationView);
            explorationView.Angle = 360.0;

            List<Player> res =  Layer.Environment.ExploreCharacters(this, cone,player => player.GetType()==typeof(Zombie)).ToList();
            Console.WriteLine("Zombies in sight... ");
           res.ForEach(Console.WriteLine);
            return res;
        }

        private Weapon FindClosestWeapon()
        {
            return (Weapon)Layer.Environment
                .ExploreObstacles(_boundaryBoxGeometry, item => item is Weapon).OrderBy(item =>
                    Distance.Chebyshev(Position.PositionArray, item.Position.PositionArray)).FirstOrDefault();
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
            Layer.Environment.
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

   // Wenn Zombies gesehen werde und Waffe vorhanden ist: Entscheidung ob laufen oder schießen anhand des QLearning-Algorithmus
        public void QMovement()
        {
            var closestZombie = FindClosestZombie();
            var nextWeapon = FindClosestWeapon();
            
                
            if (closestZombie != null)
            {
                if (weapons.Count > 1)
                {
                    int state = (int)GetDistanceFromPlayer(closestZombie);
                    Console.WriteLine("State: " + state); 
                
                    // action anhand der Q-Werte für Aktionen im aktuellen Zustand
                    // Wahrscheinlichkeit nach Roulette Wheel Policy
                    int action = QHumanLearning.QLearning.GetAction(state);
                    Act(action,closestZombie);
                
                    int nextState = (int)GetDistanceFromPlayer(closestZombie);
                    QHumanLearning.QLearning.UpdateState(state,action,Reward(closestZombie,state),nextState);
                }
                else 
                {
                    RunFromZombie(closestZombie);
                }
            }
            else if(weapons.Count < 1 && GetDistanceFromItem(nextWeapon) < 20)
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
            Console.WriteLine("action index: " + actionIndex );
            switch(actionIndex)
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
        //TODO Wie soll die Bewertung sein?
        public double Reward(Zombie closestZombie, int oldDistance)
        {
            if (closestZombie.Dead)
            {
                return 1.0;
            }

            if (GetDistanceFromPlayer(closestZombie) - oldDistance > 10)
            {
                return 0.5;
            }

            return 0.0;
        }
    }
}