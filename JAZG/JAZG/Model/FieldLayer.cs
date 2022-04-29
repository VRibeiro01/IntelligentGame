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

        public override bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle,
            UnregisterAgent unregisterAgentHandle)
        {
            base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle); // base class requires init, too

            var inputs = layerInitData.LayerInitConfig.Inputs;

            if (inputs != null && inputs.Any())
            {
                Environment = new CollisionEnvironment<Player, Item>();
                
                // TODO: improve this
                Environment.BoundingBox =
                    new BoundingBox(new Position(0, 0), new Position(100, 100));
                Coordinate[] wall1 = {new(0, 0), new(0, 100)};
                Coordinate[] wall2 = {new(0, 0), new(100, 0)};
                Coordinate[] wall3 = {new(100, 100), new(0, 100)};
                Coordinate[] wall4 = {new(100, 100), new(100, 0)};
                Environment.Insert(new Wall(), new LineString(wall1));
                Environment.Insert(new Wall(), new LineString(wall2));
                Environment.Insert(new Wall(), new LineString(wall3));
                Environment.Insert(new Wall(), new LineString(wall4));
            }

            // the agent manager can create agents and initializes them as defined in the sim config
            var agentManager = layerInitData.Container.Resolve<IAgentManager>();

            //Create and register agents
            var human_agents = agentManager.Spawn<Human, FieldLayer>().ToList();
            var zombie_agents = agentManager.Spawn<Zombie, FieldLayer>().ToList();
            Console.WriteLine("We created " + human_agents.Count + " human agents.");
            Console.WriteLine("We created " + zombie_agents.Count + " zombie agents.");

            //TODO create walls and place them on the field
            // TODO create food class and place them on field + functionality when human finds food
            // TODO create Weapon Gun 
            return true;
        }

        // Helper method to find random position within the bounds of the layer
        public Position FindRandomPosition()
        {
            var random = RandomHelper.Random;
            return Position.CreatePosition(random.Next(Width - 1), random.Next(Height - 1));
        }
    }
}