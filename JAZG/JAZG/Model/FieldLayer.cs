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
                Environment.BoundingBox =
                    new BoundingBox(new Position(0, 0), new Position(Width - 1, Height - 1));

                //TODO represent wall as number in BattleGround cvs file and automate wall creation by reading from cvs file (see lasertag game)
                //TODO same with weapons and food
                Coordinate[] wall1 = {new(0, Height - 1), new(Width - 1, Height - 1)};
                Coordinate[] wall2 = {new(0, 0), new(Width - 1, 0)};
                Coordinate[] wall3 = {new(Width - 1, Height - 1), new(0, Height - 1)};
                Coordinate[] wall4 = {new(Width - 1, Height - 1), new(Width - 1, 0)};
                Environment.Insert(new Wall(), new LineString(wall1));
                Environment.Insert(new Wall(), new LineString(wall2));
                Environment.Insert(new Wall(), new LineString(wall3));
                Environment.Insert(new Wall(), new LineString(wall4));
                //Environment.Insert(new Weapon(),new Point(3,3));
                for (int i = 0; i < 100; i++)
                {
                    Point p = FindRandomPoint();
                  //  Console.WriteLine("the point is " + p);
                    Environment.Insert(new Gun(), p);
                }
            }

            // the agent manager can create agents and initializes them as defined in the sim config
            var agentManager = layerInitData.Container.Resolve<IAgentManager>();

            //Create and register agents
            var human_agents = agentManager.Spawn<Human, FieldLayer>().ToList();
            var zombie_agents = agentManager.Spawn<Zombie, FieldLayer>().ToList();
            Console.WriteLine("We created " + human_agents.Count + " human agents.");
            Console.WriteLine("We created " + zombie_agents.Count + " zombie agents.");

            return true;
        }
        
        
        // Helper method to find random position within the bounds of the layer
        public Position FindRandomPosition()
        {
            var random = RandomHelper.Random;
            return Position.CreatePosition(random.Next(Width - 1), random.Next(Height - 1));
        }

        public Point FindRandomPoint()
        {
            var random = RandomHelper.Random;
            int x = (int) random.Next(Width);
            int y = (int) random.Next(Height);
            return new Point(x, y);
        }
    }
}