using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
        public static int Actions = 2;

        //implements method to choose next action according to Roulette Wheel 
        public static RouletteWheelExploration ExplorationPolicy = new RouletteWheelExploration();

        // possible distances from zombie
        public static int States = 4;


        // QLearning class contains table with states and a zero-initialized double array where length=actions
        public QLearning QLearning { get; set; }


        public QHumanLearning()
        {
            QLearning = new QLearning(States, Actions, ExplorationPolicy);
        }


        // Wenn Zombies gesehen werde und Waffe vorhanden ist: Entscheidung ob laufen oder schießen anhand des QLearning-Algorithmus
        public void QMovement(Human human)
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
                    //TODO states erweitern
                    // Distanz zum nächsten Zombie, Anzahl Zombies,
                    // wo befinden sich Zombies ("entweder umzingelt oder nicht" oder "zonen mit zombies")
                    var state = GetState(closestZombie, human);
                    Console.WriteLine("State: " + state);

                    // action anhand der Q-Werte für Aktionen im aktuellen Zustand
                    // Wahrscheinlichkeit nach Roulette Wheel Policy
                    // TODO Find out why program doesnt get past this line when using deserialized qtable!!!

                    lock (QLearning)
                    {
                        var action = QLearning.GetAction(state);
                        Act(action, closestZombie, human);
                        var nextState = GetState(closestZombie, human);
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

        public int GetState(Player closestZombie, Human human)
        {
            var distanceFromZombie =
                Distance.Chebyshev(human.Position.PositionArray, closestZombie.Position.PositionArray);
            int d1 = 0, d2 = 0, d3 = 0;
            switch (distanceFromZombie)
            {
                case <= 5:
                    d1 = 1;
                    break;
                case <= 10:
                    d2 = 1;
                    break;
                case <= 15:
                    d3 = 1;
                    break;
            }

            var closeZombies = human.FindZombies();
            var closeZombiesCount = human.FindZombies().Count;
            int zn1 = 0, zn2 = 0, zn3 = 0, zn4 = 0;
            //TODO: welche Anzahlen von Zombies
            switch (closeZombiesCount)
            {
                case <= 5:
                    zn1 = 1;
                    break;
                case <= 10:
                    zn2 = 1;
                    break;
                case <= 20:
                    zn3 = 1;
                    break;
                case > 20:
                    zn4 = 1;
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
            if (zombiesNorthEast.Count > 2) zl1 = 1;
            if (zombiesSouthEast.Count > 2) zl2 = 1;
            if (zombiesSouthWest.Count > 2) zl3 = 1;
            if (zombiesNorthWest.Count > 2) zl4 = 1;

            return d1 | (d2 << 1) | (d3 << 2) | (zn1 << 3) | (zn2 << 4) | (zn3 << 5) | (zn4 << 6) | (zl1 << 7) |
                   (zl2 << 8) | (zl3 << 9) | (zl4 << 10);
        }

        // Übersetzung des Action-Index in Aktion
        public void Act(int actionIndex, Zombie closestZombie, Human human)
        {
            Console.WriteLine("action index: " + actionIndex);
            switch (actionIndex)
            {
                case > 0:
                    human.UseWeapon(closestZombie);
                    break;

                default:
                    human.RunFromZombies(closestZombie);
                    Console.WriteLine("Running");
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