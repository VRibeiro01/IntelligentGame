using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
        // amount of possible actions: run or shoot
        public static int Actions = 3;

        //implements method to choose next action according to Roulette Wheel 
        public static RouletteWheelExploration ExplorationPolicy = new RouletteWheelExploration();

        // possible distances from zombie
        public static int States = 1536;


        // QLearning class contains table with states and a zero-initialized double array where length=actions
        public QLearning QLearning { get; set; }


        public QHumanLearning()
        {
            QLearning = new QLearning(States, Actions, ExplorationPolicy);
        }


        // Wenn Zombies gesehen werde und Waffe vorhanden ist: Entscheidung ob laufen oder schießen anhand des QLearning-Algorithmus
        /*public void QMovement(Human human)
        {
            var zombiesNearMe = human.FindZombies();
            var closestZombie = (Zombie) zombiesNearMe.OrderBy(zombie =>
                    Distance.Chebyshev(human.Position.PositionArray, zombie.Position.PositionArray))
                .FirstOrDefault();
            var nextWeapon = human.FindClosestWeapon();

            if (closestZombie != null)
            {
                if (human.weapons.Count > 1)
                {
                    var state = GetState(closestZombie, zombiesNearMe, human);
                    Console.WriteLine("State: " + state);

                    // action anhand der Q-Werte für Aktionen im aktuellen Zustand
                    // Wahrscheinlichkeit nach Roulette Wheel Policy
                    // TODO Find out why program doesnt get past this line when using deserialized qtable!!!

                    lock (QLearning)
                    {
                        var action = QLearning.GetAction(state);
                        Act(action, closestZombie, nextWeapon, human);
                        var nextState = GetState(closestZombie, zombiesNearMe, human);
                        QLearning.UpdateState(state, action, Reward(closestZombie, state, zombiesNearMe.Count, human),
                            nextState);
                    }
                }
                else
                {
                    human.RunFromZombies(closestZombie);
                }
            }
            else if (human.weapons.Count < 1 && human.GetDistanceFromItem(nextWeapon) < 20)
            {
                human.CollectItem(nextWeapon);
            }
            else
            {
                human.RandomMove();
            }
        }*/

        public void QMovement(Human human)
        {
            var zombiesNearMe = human.FindZombies();
            var closestZombie = (Zombie) zombiesNearMe.OrderBy(zombie =>
                    Distance.Chebyshev(human.Position.PositionArray, zombie.Position.PositionArray))
                .FirstOrDefault();
            var nextWeapon = human.FindClosestWeapon();

            var state = GetState(closestZombie, zombiesNearMe, nextWeapon, human);
            Console.WriteLine("State: " + state);

            // action anhand der Q-Werte für Aktionen im aktuellen Zustand
            // Wahrscheinlichkeit nach Roulette Wheel Policy
            // TODO Find out why program doesnt get past this line when using deserialized qtable!!!

            lock (QLearning)
            {
                var action = QLearning.GetAction(state);
                Act(action, closestZombie, nextWeapon, human);
                var nextState = GetState(closestZombie, zombiesNearMe, nextWeapon, human);
                QLearning.UpdateState(state, action, Reward(closestZombie, state, zombiesNearMe.Count, human),
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

            int zd1 = 0, zd2 = 0, zd3 = 0;
            switch (distanceFromZombie)
            {
                case -1:
                    break;
                case <= 5:
                    zd1 = 1;
                    break;
                case <= 15:
                    zd2 = 1;
                    break;
                case > 15:
                    zd3 = 1;
                    break;
            }

            var closeZombiesCount = closeZombies.Count;
            int zn1 = 0, zn2 = 0, zn3 = 0;
            //TODO: welche Anzahlen von Zombies
            switch (closeZombiesCount)
            {
                case <= 5:
                    zn1 = 1;
                    break;
                case <= 15:
                    zn2 = 1;
                    break;
                case > 15:
                    zn3 = 1;
                    break;
            }

            var zombiesNorthEast = closeZombies.FindAll(zombie => human.GetDirectionToPlayer(zombie) < 90);
            var zombiesSouthEast = closeZombies.FindAll(zombie =>
                human.GetDirectionToPlayer(zombie) >= 90 && human.GetDirectionToPlayer(zombie) < 180);
            var zombiesSouthWest = closeZombies.FindAll(zombie =>
                human.GetDirectionToPlayer(zombie) >= 180 && human.GetDirectionToPlayer(zombie) < 270);
            var zombiesNorthWest = closeZombies.FindAll(zombie => human.GetDirectionToPlayer(zombie) >= 270);
            int zl1 = 0, zl2 = 0, zl3 = 0, zl4 = 0;
            //TODO: wieviele Zombies pro Zone?
            if (zombiesNorthEast.Count > 4) zl1 = 1;
            if (zombiesSouthEast.Count > 4) zl2 = 1;
            if (zombiesSouthWest.Count > 4) zl3 = 1;
            if (zombiesNorthWest.Count > 4) zl4 = 1;

            double weaponDistance;
            if (nextWeapon == null) weaponDistance = -1;
            else weaponDistance = human.GetDistanceFromItem(nextWeapon);

            int wd1 = 0, wd2 = 0, wd3 = 0;
            switch (weaponDistance)
            {
                case -1:
                    break;
                case <= 5:
                    wd1 = 1;
                    break;
                case <= 15:
                    wd2 = 1;
                    break;
                case > 15:
                    wd3 = 1;
                    break;
            }

            var w = 0;
            if (human.HasWeapon > 0) w = 1;

            // number of states: 4 * 3 * 16 * 4 * 2 = 1536
            return zd1 | (zd2 << 1) | (zd3 << 2) | (zn1 << 3) | (zn2 << 4) | (zn3 << 5) | (zl1 << 6) | (zl2 << 7) |
                   (zl3 << 8) | (zl4 << 9) | (wd1 << 10) | (wd2 << 11) | (wd3 << 12) | (w << 13);
        }

        // Übersetzung des Action-Index in Aktion
        public void Act(int actionIndex, Zombie closestZombie, Weapon nextWeapon, Human human)
        {
            Console.WriteLine("action index: " + actionIndex);
            switch (actionIndex)
            {
                case 0:
                    if (closestZombie == null) break;
                    human.RunFromZombies(closestZombie);
                    Console.WriteLine("Running");
                    break;
                case 1:
                    if (closestZombie == null || human.weapons.Count == 0) break;
                    human.UseWeapon(closestZombie);
                    break;
                case 2:
                    if (nextWeapon == null) break;
                    human.CollectItem(nextWeapon);
                    break;
            }
        }

        // Berechnet die Bewertungsfunktion
        // GUT: Zombie tot, weniger Zombies in Sichtfeld, neue Distanz zum nächsten Zombie kleiner
        // Schlecht: Zombie lebt noch, mehr Zombies in Sichtfeld, Distanz zum nächsten Zombie kleiner
        public double Reward(Player closestZombie, int oldDistance, int oldClosestZombiesCount, Human human)
        {
            var reward = 0.0;
            if (closestZombie.Dead)
                reward++;
            else
                reward -= 0.25;

            Player newClosestZombie = human.FindClosestZombie();
            if (newClosestZombie != null && human.GetDistanceFromPlayer(newClosestZombie) > oldDistance)
                reward += 0.5;
            else if (human.GetDistanceFromPlayer(newClosestZombie) < oldDistance) reward -= 0.5;


            if (human.FindZombies().Count < oldClosestZombiesCount)
                reward += 0.5;
            else
                reward -= 0.5;

            if (human.Dead) reward--;

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
    }
}