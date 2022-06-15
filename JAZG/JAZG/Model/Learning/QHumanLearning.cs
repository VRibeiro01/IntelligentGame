using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using JAZG.Model.Objects;
using JAZG.Model.Players;
using Mars.Common.IO;
using Mars.Components.Services.Explorations;
using Mars.Components.Services.Learning;
using Mars.Numerics;

namespace JAZG.Model.Learning
{
    [Serializable]
    public class QHumanLearning
    {
        public static int Actions = 4;

        //implements method to choose next action according to Roulette Wheel 
        public static RouletteWheelExploration ExplorationPolicy = new RouletteWheelExploration();


        public static int States = 1152;

        private Dictionary<(int, int, int, int, int, int, int, int), int> _statesDictionary;

        // QLearning class contains table with states and a zero-initialized double array where length=actions
        public QLearning QLearning { get; set; }


        public QHumanLearning()
        {
            MakeTable();
            QLearning = new QLearning(States, Actions, ExplorationPolicy);
        }


        public void QMovement(Human human)
        {
            var zombiesNearMe = human.FindZombies();
            var closestZombie = (Zombie) zombiesNearMe.OrderBy(zombie =>
                    Distance.Chebyshev(human.Position.PositionArray, zombie.Position.PositionArray))
                .FirstOrDefault();
            var allAliveZombies = human.Layer.GetAllZombiesCount();
            var allAliveHumans = human.Layer.GetAllHumansCount();
            var oldEnergy = closestZombie.Energy;
            var oldDistance = human.GetDistanceFromPlayer(closestZombie);
            var hadWeapon = human.HasWeapon == 4 || human.HasWeapon == 7;
            var nextWeapon = human.FindClosestWeapon();
            

            var state = GetState(closestZombie, zombiesNearMe, nextWeapon, human);
            Console.WriteLine("State: " + state);

            // action anhand der Q-Werte für Aktionen im aktuellen Zustand
            // Wahrscheinlichkeit nach Roulette Wheel Policy


            lock (QLearning)
            {
                var action = QLearning.GetAction(state);
                Act(action, closestZombie, nextWeapon, human, state);
                var nextState = GetState(closestZombie, zombiesNearMe, nextWeapon, human);
                QLearning.UpdateState(state, action, Reward(closestZombie, oldEnergy, allAliveZombies,oldDistance,zombiesNearMe.Count, allAliveHumans,hadWeapon, human),
                    nextState);
            }
        }

        public int _GetState(Player closestZombie, Human human)
        {
            var distanceFromZombie =
                Distance.Chebyshev(human.Position.PositionArray, closestZombie.Position.PositionArray);
            if (distanceFromZombie <= 5) return 0;

            if (distanceFromZombie <= 10) return 1;

            if (distanceFromZombie <= 15) return 2;

            return 3;
        }

        public int GetState(Player closestZombie, List<Player> closeZombies, Weapon nextWeapon, Human human)
        {
            double distanceFromZombie;
            if (closestZombie == null) distanceFromZombie = -1;
            else
                distanceFromZombie =
                    Distance.Chebyshev(human.Position.PositionArray, closestZombie.Position.PositionArray);

            int zd = 0;
            switch (distanceFromZombie)
            {
                case -1:
                    zd = 1;
                    break;
                case <= 5:
                    zd = 2;
                    break;
                case <= 15:
                    zd = 3;
                    break;
                case > 15:
                    zd = 4;
                    break;
            }

            var closeZombiesCount = closeZombies.Count;
            int zn = 0;

            switch (closeZombiesCount)
            {
                case <= 5:
                    zn = 1;
                    break;
                case <= 10:
                    zn = 2;
                    break;
                case > 10:
                    zn = 3;
                    break;
            }

            var zombiesNorthEast = closeZombies.FindAll(zombie => human.GetDirectionToPlayer(zombie) < 90);
            var zombiesSouthEast = closeZombies.FindAll(zombie =>
                human.GetDirectionToPlayer(zombie) >= 90 && human.GetDirectionToPlayer(zombie) < 180);
            var zombiesSouthWest = closeZombies.FindAll(zombie =>
                human.GetDirectionToPlayer(zombie) >= 180 && human.GetDirectionToPlayer(zombie) < 270);
            var zombiesNorthWest = closeZombies.FindAll(zombie => human.GetDirectionToPlayer(zombie) >= 270);

            int zl1 = 1, zl2 = 1, zl3 = 1, zl4 = 1;
            if (zombiesNorthEast.Count > 2) zl1 = 2;
            if (zombiesSouthEast.Count > 2) zl2 = 2;
            if (zombiesSouthWest.Count > 2) zl3 = 2;
            if (zombiesNorthWest.Count > 2) zl4 = 2;

            double weaponDistance;
            if (nextWeapon == null) weaponDistance = -1;
            else weaponDistance = human.GetDistanceFromItem(nextWeapon);

            int wd = 0;
            switch (weaponDistance)
            {
                case -1:
                    wd = 1;
                    break;
                case <= 8:
                    wd = 2;
                    break;
                case > 8:
                    wd = 3;
                    break;
            }

            var w = 1;
            if (human.HasWeapon > 0) w = 2;
            /*var w1 = 0;
            if (human.hasM16()) w1 = 1;*/

            
            // number of states: 14 different variables that define state. Each variable can be 0 or 1: 2^14 = 16384 states
            /*return zd1 | (zd2 << 1) | (zd3 << 2) | (zn1 << 3) | (zn2 << 4) | (zn3 << 5) | (zl1 << 6) | (zl2 << 7) |
                   (zl3 << 8) | (zl4 << 9) | (wd1 << 10) | (wd2 << 11) | (w << 12) | (w1 << 13);*/
            return _statesDictionary[(zd, zn, zl1, zl2, zl3, zl4, wd, w)];
        }

        // Übersetzung des Action-Index in Aktion
        public void Act(int actionIndex, Zombie closestZombie, Weapon nextWeapon, Human human, int state)
        {
            human.IsShooting = false;
            Console.WriteLine("state: " + state + "," + "action index: " + actionIndex);

            switch (actionIndex)
            {
                case 0:
                    if (closestZombie == null) break;
                    human.RunFromZombies(closestZombie);
                    Console.WriteLine("Running");
                    break;
                case 1:
                    if (closestZombie == null || human.weapons.Count == 0) break;
                    human.IsShooting = human.UseWeapon(closestZombie);
                    break;
                case 2:
                    if (nextWeapon == null) break;
                    human.CollectItem(nextWeapon);
                    break;
                case 3:
                    // TODO: explore method
                    human.RandomMove();
                    break;
            }
        }

        // Berechnet die Bewertungsfunktion
        // GUT: Zombie tot, weniger Zombies in Sichtfeld, neue Distanz zum nächsten Zombie kleiner
        // Schlecht: Zombie lebt noch, mehr Zombies in Sichtfeld, Distanz zum nächsten Zombie kleiner
        public double Reward(Player closestZombie, int oldEnergy, int oldZombieCount, double oldDistance, int oldClosestZombiesCount,int oldTotalHumanCount, bool hadWeapon, Human human)
        {
            var reward = 0.0;
            if (closestZombie.Energy < oldEnergy)
                reward += 0.25;

            if (oldZombieCount > human.Layer.GetAllZombiesCount())
                reward++;
            else
                reward -= 0.5;
        

            Player newClosestZombie = human.FindClosestZombie();
            if (newClosestZombie != null && human.GetDistanceFromPlayer(newClosestZombie) > oldDistance)
                reward += 0.5;
            else if (human.GetDistanceFromPlayer(newClosestZombie) < oldDistance) reward -= 0.5;


            if (human.FindZombies().Count < oldClosestZombiesCount)
                reward += 0.5;
            else
                reward -= 0.5;

            if (human.Dead) reward--;

            if (human.Layer.GetAllHumansCount() < oldTotalHumanCount)
                reward -= 0.5;

            if (!hadWeapon && human.HasWeapon == 4)
                reward += 0.5;
            if (!hadWeapon && human.HasWeapon == 7)
                reward++;

            return reward;
        }

        public void Serialize(String filePath)
        {
            Console.WriteLine("Serializing... " + filePath);
            using FileStream fs = File.Create(filePath);
            var bytes = QLearning.Serialize();
            fs.Write(bytes, 0, bytes.Length);
        }

        public static QLearning Deserialize(String filePath)
        {
            Console.WriteLine("Deserializing..");
            var bytes = File.ReadAllBytes(filePath);
            var qLearning = (QLearning) ObjectSerialize.DeSerialize(bytes);
            qLearning.ExplorationPolicy = new RouletteWheelExploration();
            return qLearning;
        }
        
        public void MakeTable()
        {
            _statesDictionary = new Dictionary<(int, int, int, int, int, int, int, int), int>();
            int i = 1;
            for (int zd = 1; zd < 5; zd++)
            {
                for (int zn = 1; zn < 4; zn++)
                {
                    for (int zl1 = 1; zl1 < 3; zl1++)
                    {
                        for (int zl2 = 1; zl2 < 3; zl2++)
                        {
                            for (int zl3 = 1; zl3 < 3; zl3++)
                            {
                                for (int zl4 = 1; zl4 < 3; zl4++)
                                {
                                    for (int wd = 1; wd < 4; wd++)
                                    {
                                        for (int w = 1; w < 3; w++)
                                        {
                                            _statesDictionary.Add((zd, zn, zl1, zl2, zl3, zl4, wd, w), i);
                                            i++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}