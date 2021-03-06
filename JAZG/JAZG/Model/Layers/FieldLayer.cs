using System;
using System.Collections.Generic;
using System.IO;
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

        public List<QHumanLearning> QHumanLearningList=new();
      
        public int amountOfMinds=5;

        // if true gaming statistics will be saved on to file stats.txt
        public bool SaveStats = true;

        //-------------------- Needed for statistics---------------------------------
        public int ZombiesKilled = 0;

        public int HumansKilled = 0;

        public int ZombiesSpawned;

        public int HumansSpawned;
        
        public int CustomHumansSpawned;
        
      //-----------------------------------------------------------------------------
        
        /// <summary>
        // 0 --> The agents will move without the QLearning algorithm
        // 1 --> A new Qtable will be created 
        // 2 --> A previously trained Qtable will be obtained from a file 
        // 3 --> A new Qtable of Type 2 will be created 
        // 4  --> A previously trained Qtable of Type 2 will be obtained from a file 
        /// </summary>
        public int learningMode = 4;

        // should table changes me saved?
        public bool updateTable = true;

        
        public int learningIterations = 21501;

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
            InitQHumanLearning();

            //Create and register agents
            var wallAgents = AgentManager.Spawn<Wall, FieldLayer>().ToList();
            var gunAgents = AgentManager.Spawn<Gun, FieldLayer>().ToList();            
            var m16Agents = AgentManager.Spawn<M16, FieldLayer>().ToList();
            var humanAgents = AgentManager.Spawn<Human, FieldLayer>().ToList();
            var zombieAgents = AgentManager.Spawn<Zombie, FieldLayer>().ToList();
            var customHumanAgents = AgentManager.Spawn<CustomHuman, FieldLayer>().ToList();

            HumansSpawned = humanAgents.Count;
            ZombiesSpawned = zombieAgents.Count;
            CustomHumansSpawned = humanAgents.Count;
            
            Console.WriteLine("We created " + humanAgents.Count + " human agents.");
            Console.WriteLine("We created " + zombieAgents.Count + " zombie agents.");
            Console.WriteLine("We created " + customHumanAgents.Count + " customHumanAgents agents.");

          
            return true;
        }

        public void InitQHumanLearning()
        {
            var basePath = @"..\..\..\Resources";
            

            if (learningMode < 0)
            {
                throw new ArgumentException("learningMode must equal 0 or be larger than 0");
            }
            
            if (learningMode == 0)
            {
                return;
            }

            if (learningMode <= 2)
            {
                for (int i = 0; i < amountOfMinds; i++)
                {
                    QHumanLearningList.Add(new QHumanLearning());
                }
                Console.WriteLine("New QTables created");
            }
            
            if (learningMode == 2)
            {
                for (int i = 0; i < amountOfMinds; i++)
                {
                    QHumanLearningList[i].QLearning =
                        QHumanLearning.Deserialize(Path.Combine(basePath, "HumanLearning" + i + ".txt"));
                }

                return;
            }

            if (learningMode <= 4)
            {
                for (int i = 0; i < amountOfMinds; i++)
                {
                    QHumanLearningList.Add(new QHumanLearning(true));
                    
                }
                Console.WriteLine("New QTables Type 2 created");
            }
            if (learningMode == 4)
            {
                for (int i = 0; i < amountOfMinds; i++)
                {
                    QHumanLearningList[i].QLearning =
                        QHumanLearning.Deserialize(Path.Combine(basePath, learningIterations + "NewHumanLearning" + i + ".txt"));
                }
                
            }
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

        public int GetAllZombiesCount()
        {
           return  Environment.Characters.Where(c => c is Zombie).ToList().Count;
        }
        
        public int GetAllHumansCount()
        {
            return  Environment.Characters.Where(c => c is Human).ToList().Count;
        }
    }
}