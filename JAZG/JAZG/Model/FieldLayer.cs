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

                //TODO represent wall as number in BattleGround cvs file and automate wall creation by reading from cvs file (see lasertag game)
                //TODO same with weapons and food
                Coordinate[] wall1 = {new(0 + outerWallOffset, Height - outerWallOffset), new(Width - outerWallOffset, Height - outerWallOffset)};
                Coordinate[] wall2 = {new(0 + outerWallOffset, 0 + outerWallOffset), new(Width - outerWallOffset, 0 + outerWallOffset)};
                Coordinate[] wall3 = {new(0 + outerWallOffset, 0 + outerWallOffset), new(0 + outerWallOffset, Height - outerWallOffset)};
                Coordinate[] wall4 = {new(Width - outerWallOffset, Height - outerWallOffset), new(Width - outerWallOffset, 0 + outerWallOffset)};
                Environment.Insert(new Wall(), new LineString(wall1));
                Environment.Insert(new Wall(), new LineString(wall2));
                Environment.Insert(new Wall(), new LineString(wall3));
                Environment.Insert(new Wall(), new LineString(wall4));
            }

            // the agent manager can create agents and initializes them as defined in the sim config
            var agentManager = layerInitData.Container.Resolve<IAgentManager>();

            //Create and register agents
            agentManager.Spawn<Wall, FieldLayer>();
            var gunAgents = agentManager.Spawn<Gun, FieldLayer>().ToList();
            var humanAgents = agentManager.Spawn<Human, FieldLayer>().ToList();
            var zombieAgents = agentManager.Spawn<Zombie, FieldLayer>().ToList();
            Console.WriteLine("We created " + humanAgents.Count + " human agents.");
            Console.WriteLine("We created " + zombieAgents.Count + " zombie agents.");

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