﻿using System;
using System.Linq;
using Mars.Common;
using Mars.Numerics;

namespace JAZG.Model.Players
{
    public class Zombie : Player
    {
        public override void Init(FieldLayer layer)
        {
            base.Init(layer);
            Energy = 15;
        }
        
        private int _lastAction;

        public override void Tick()
        {
            // TODO Implement simple movements
            // TODO implement action upon meeting zombie using collisionHashEnvironment  functionalities
            base.Tick();
            var nearestHuman = Layer.Environment.Characters.Where(h => h.GetType() == typeof(Human))
                .OrderBy(hD => Distance.Chebyshev(Position.PositionArray, hD.Position.PositionArray)).FirstOrDefault();
            if (nearestHuman != null)
            {
                var humanDistance = (int) Distance.Chebyshev(
                    Position.PositionArray, nearestHuman.Position.PositionArray);
                if (humanDistance <= 2)
                {
                    EatHuman(nearestHuman);
                    Console.WriteLine("Chomp, chomp!");
                }
                else if (humanDistance <= 10)
                {
                    MoveTowardsHuman(nearestHuman);
                    if (_lastAction != 2)
                    {
                        Console.WriteLine("Braaaaaains!");
                        _lastAction = 2;
                    }

                }
                else
                {
                    RandomMove();
                    if (_lastAction != 1)
                    {
                        Console.WriteLine("Brains?");
                        _lastAction = 1;
                    }
                }
            }
            else
            {
                RandomMove();
                if (_lastAction != 1)
                {
                    Console.WriteLine("Brains?");
                    _lastAction = 1;
                }
            }
        }

        private void EatHuman(Player human)
        {
            Energy += 4;
            human.Kill();
            // TO DO Kill
        }
        
        private void MoveTowardsHuman(Player human)
        {
            var directionToEnemy =
                PositionHelper.CalculateBearingCartesian
                    (Position.X, Position.Y, human.Position.X, human.Position.Y);
            Layer.Environment.Move(this, directionToEnemy, 2);
        }
    }
}