using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using JAZG.Model.Players;
using Mars.Components.Services.Explorations;
using Mars.Components.Services.Learning;
using Mars.Numerics;

namespace JAZG.Model.Learning
{
    [Serializable]
    public class QHumanLearning
    {
        // amount of possible actions: run or shoot
        public int Actions;

        //implements method to choose next action according to Roulette Wheel 
        public RouletteWheelExploration ExplorationPolicy;
        
        // possible distances from zombie
        public int States;

       
        // QLearning class contains table with states and a zero-initialized double array where length=actions
        public static QLearning QLearning { get; set; }
        

        public QHumanLearning()
        {
            States = 4;
            Actions = 2;
            ExplorationPolicy = new RouletteWheelExploration();
            QLearning = new QLearning(States, Actions, ExplorationPolicy);
        }
        

        // Wenn Zombies gesehen werde und Waffe vorhanden ist: Entscheidung ob laufen oder schießen anhand des QLearning-Algorithmus
        public void QMovement(Human human)
        {
            var zombiesNearMe = human.ExploreZombies();
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
                    var action = QLearning.GetAction(state);
                    Act(action, closestZombie, human);

                    var nextState = GetState(closestZombie, human);
                    QLearning.UpdateState(state, action, Reward(closestZombie, state, zombiesNearMe.Count, human), nextState);
                }
                else
                {
                    human.RunFromZombie(closestZombie);
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

        public int GetState(Player closestZombie, Human human)
        {
            var distanceFromZombie =
                Distance.Chebyshev(human.Position.PositionArray, closestZombie.Position.PositionArray);
            if (distanceFromZombie <= 5) return 0;

            if (distanceFromZombie <= 10) return 1;

            if (distanceFromZombie <= 15) return 2;

            return 3;
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
                    human.RunFromZombie(closestZombie);
                    break;
            }
        }

        // Berechnet die Bewertungsfunktion
        // TODO Wie soll die Bewertung sein?
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


            if (human.ExploreZombies().Count < oldClosestZombiesCount)
                reward += 0.5;
            else
                reward -= 0.5;

            if (human.Dead) reward--;

            return reward;
        }

        public void Serialize(String filePath)
        {

        }

        public static QHumanLearning Deserialize(String filePath)
        {
            return null;
        }

    }
}
