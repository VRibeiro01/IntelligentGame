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

        // TODO: remove
        private int _lastAction;

        public List<Weapon> weapons = new();
        
        public override void Init(FieldLayer layer)
        {
            base.Init(layer);
            Energy = 30;
          //  weapons.Add(new Gun());
           // foreach (Gun weapon in this.weapons)
          //  {
         //       weapon.SetAmmo(10);
          //  }
        }

        public override void Tick()
        {
            base.Tick();
            
            //TODO Search for food and weapons
            //TODO Use weapons to kill zombie
            // TODO Where to go? Where to hide? When to rest? When to kill? 
            //TODO reduce speed when energy is reduced


            var nextZombie = findClosestZombie();

            if (nextZombie != null)
            {
                var zombieDistance = getDistanceFromPlayer(nextZombie);

                if (zombieDistance <= 10)
                {
                    RunFromZombie(nextZombie);
                    if (_lastAction != 2)
                    {
                        Console.WriteLine("The zombies are coming, run!!!");
                        _lastAction = 2;
                    }
                }
                else if (weapons.Count > 0 && zombieDistance <= 30)
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

        private Zombie findClosestZombie()
        {
            return (Zombie)Layer.Environment.Characters.Where(c => c.GetType() == typeof(Zombie))
                .OrderBy(zombie => Distance.Chebyshev(Position.PositionArray, zombie.Position.PositionArray))
                .FirstOrDefault();   
        }
        
        
        private void RunFromZombie(Player zombie)
        {
            var directionToEnemy = PositionHelper.CalculateBearingCartesian(
                Position.X, Position.Y, zombie.Position.X, zombie.Position.Y);
            if (double.IsNaN(directionToEnemy)) directionToEnemy = RandomHelper.Random.Next(360);
            var directionOpposite = (directionToEnemy + 180) % 360;
            Layer.Environment.Move(this, directionOpposite, 2);
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