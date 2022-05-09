using System;
using System.Linq;
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
        public CollisionEnvironment<Player, Item> Environment { get; set; }

        private int outerWallOffset = 10;

        public override bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle,
            UnregisterAgent unregisterAgentHandle)
        {
            base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle); // base class requires init, too

            var inputs = layerInitData.LayerInitConfig.Inputs;

            if (inputs != null && inputs.Any())
            {
                Environment = new CollisionEnvironment<Player, Item>();
                Environment.BoundingBox =
                    new BoundingBox(new Position(0 + outerWallOffset, 0 + outerWallOffset), new Position(Width - outerWallOffset, Height - outerWallOffset));
                
            }

            // the agent manager can create agents and initializes them as defined in the sim config
            var agentManager = layerInitData.Container.Resolve<IAgentManager>();

            //Create and register agents
            var gunAgents = agentManager.Spawn<Gun, FieldLayer>().ToList();
            var humanAgents = agentManager.Spawn<Human, FieldLayer>().ToList();
            var zombieAgents = agentManager.Spawn<Zombie, FieldLayer>().ToList();
            var wallAgents = agentManager.Spawn<Wall, FieldLayer>().ToList();
            
            // Debug outputs
            Console.WriteLine("We created " + humanAgents.Count + " human agents.");
            Console.WriteLine("We created " + zombieAgents.Count + " zombie agents.");
            Console.WriteLine("We created " + wallAgents.Count + " wall agents." + "\n");

            return true;
        }
        
        
        // Helper method to find random position within the bounds of the layer
        public Position FindRandomPosition()
        {
            var random = RandomHelper.Random;
            return Position.CreatePosition(random.Next(0 + outerWallOffset, Width - outerWallOffset), random.Next(0 + outerWallOffset, Height - outerWallOffset));
        }

        public Point FindRandomPoint()
        {
            var random = RandomHelper.Random;
            int x = random.Next(0 + outerWallOffset*3, Width - outerWallOffset*3);
            int y = random.Next(0 + outerWallOffset*3, Height - outerWallOffset*3);
            return new Point(x, y);
        }
    }
}