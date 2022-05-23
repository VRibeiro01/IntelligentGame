using System;
using System.Linq;
using JAZG.Model.Learning;
using JAZG.Model.Objects;
using JAZG.Model.Players;
using Mars.Common.Core.Random;
using Mars.Components.Environments.Cartesian;
using Mars.Components.Layers;
using Mars.Core.Data;
using Mars.Interfaces.Data;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Layers;
using NetTopologySuite.Geometries;
using Position = Mars.Interfaces.Environments.Position;

namespace JAZG.Model
{
    /// <summary>
    ///     Field, where all agents and entities live
    ///     Type Rasterlayer represents a n x m matrix. Allows processing of grid data
    /// </summary>
    public class FieldLayer : RasterLayer
    {
        private readonly int outerWallOffset = 10;
        public CollisionEnvironment<Player, Item> Environment { get; set; }
        public IAgentManager AgentManager { get; private set; }

        public QHumanLearning QHumanLearning;

        // TODO get from config file
        public int learningMode = 0;


        public override bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle,
            UnregisterAgent unregisterAgentHandle)
        {
            base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle); // base class requires init, too

            var inputs = layerInitData.LayerInitConfig.Inputs;

            if (inputs != null && inputs.Any())
            {
                Environment = new CollisionEnvironment<Player, Item>();
                Environment.BoundingBox =
                    new BoundingBox(new Position(0, 0), new Position(100, 95));
            }


            // the agent manager can create agents and initializes them as defined in the sim config
            AgentManager = layerInitData.Container.Resolve<IAgentManager>();

            // initialize QLearning of humans
            initQHumanLearning();

            //Create and register agents
            var wallAgents = AgentManager.Spawn<Wall, FieldLayer>().ToList();
            var gunAgents = AgentManager.Spawn<Gun, FieldLayer>().ToList();
            var humanAgents = AgentManager.Spawn<Human, FieldLayer>().ToList();
            var zombieAgents = AgentManager.Spawn<Zombie, FieldLayer>().ToList();
            Console.WriteLine("We created " + humanAgents.Count + " human agents.");
            Console.WriteLine("We created " + zombieAgents.Count + " zombie agents.");


            return true;
        }

        public void initQHumanLearning()
        {
            QHumanLearning = new QHumanLearning();
            if (learningMode == 0)
            {
                return;
            }
            if (learningMode == 1)
            {
                Console.WriteLine("New QTable created");
                return;
            }

            QHumanLearning.QLearning = QHumanLearning.Deserialize("JAZG/Resources/HumanLearning.txt");
        }

        // Helper method to find random position within the bounds of the layer
        public Position FindRandomPosition()
        {
            var random = RandomHelper.Random;
            return Position.CreatePosition(random.Next(0 + outerWallOffset, Width - outerWallOffset),
                random.Next(0 + outerWallOffset, Height - outerWallOffset));
        }

        public Point FindRandomPoint()
        {
            var random = RandomHelper.Random;
            var x = random.Next(0 + outerWallOffset * 3, Width - outerWallOffset * 3);
            var y = random.Next(0 + outerWallOffset * 3, Height - outerWallOffset * 3);
            return new Point(x, y);
        }
    }
}