using Mars.Components.Services.Explorations;
using Mars.Components.Services.Learning;
using Mars.Interfaces.Environments;
using Mars.Numerics;

namespace JAZG.Model.Learning
{
    public class QHumanLearning
    {
        // possible distances from zombie
        public int states;

        // amount of possible actions: run or shoot
        public int actions;

        //implements method to choose next action according to Roulette Wheel 
        public IExplorationPolicy ExplorationPolicy;

        // QLearning class contains table with states and a zero-initialized double array where length=actions
        public static QLearning QLearning { get; set; }

        public QHumanLearning(int maxSeeingDistance)
        {
            states = 3;
            actions = 2;
            ExplorationPolicy = new RouletteWheelExploration();
            QLearning = new QLearning(states, actions, ExplorationPolicy);

        }


    }
}