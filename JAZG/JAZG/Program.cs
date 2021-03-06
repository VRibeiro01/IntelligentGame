using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JAZG.Model;
using JAZG.Model.Objects;
using JAZG.Model.Players;
using Mars.Components.Starter;
using Mars.Interfaces.Model;


namespace JAZG
{
   
    //   Class that sets up model, configures the scenario and starts the simulation
    public static class Program
    {
        public static void Main(string[] args)
        {
            var basePath = @"..\..\..\Resources";
            
            
            
           // ------ Start visualization: Comment this section out if you don't want the visualization to start---------
            
            
            /*ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "..\\..\\..\\..\\Visualization\\main.py";
            bool exists = File.Exists(start.FileName);
            start.Arguments = "";
            start.UseShellExecute = true;
            Process.Start(start);*/
            
            
            
            
            //----------------------------- Set up model description ---------------------------------------------------
            var description = new ModelDescription();

            description.AddLayer<FieldLayer>();

            // Add agents to model
            description.AddAgent<Human, FieldLayer>();
            description.AddAgent<Zombie, FieldLayer>();
            description.AddAgent<Wall, FieldLayer>();
            description.AddAgent<Gun, FieldLayer>();
            description.AddAgent<Food, FieldLayer>();
            description.AddAgent<DeadPlayer, FieldLayer>();
            description.AddAgent<M16, FieldLayer>();
            description.AddAgent<CustomHuman, FieldLayer>();
            //TODO Add you custom agent to the model description here!
            //----------------------------------------------------------------------------------------------------------
            
            
            
            // ----------------------------- Start Simulation ----------------------------------------------------------
            var file = File.ReadAllText("config.json");
            var config = SimulationConfig.Deserialize(file);
            int learningIterations = 90;
            for (int iterationIndex=76; iterationIndex <= learningIterations; iterationIndex++)
            {
                var task = SimulationStarter.Start(description, config);
                var loopResults = task.Run(); 
            // ---------------------------------------------------------------------------------------------------------



                //----------------------------- Serialize QTables-----------------------------------------------------------
                FieldLayer layer = (FieldLayer) loopResults.Model.Layers.Values.First();

                if (layer.learningMode > 0 && layer.learningMode< 3)
                {
                    for (int i = 0; i < layer.amountOfMinds; i++)
                    {
                        layer.QHumanLearningList[i].Serialize(
                            Path.Combine(basePath, "HumanLearning" + i + ".txt"));
                    }
                }
                if (layer.learningMode >=3 && layer.updateTable)
                {
                    for (int i = 0; i < layer.amountOfMinds; i++)
                    {
                        layer.QHumanLearningList[i].Serialize(
                            Path.Combine(basePath, layer.learningIterations + "NewHumanLearning" + i + ".txt"));
                    }
                }
                //----------------------------------------------------------------------------------------------------------
                

                //----------------------------- Save game statistics in file------------------------------------------------
                if (layer.SaveStats)
                {
                    var statsText =iterationIndex + ";" + 
                                   ((double) (layer.HumansSpawned - layer.HumansKilled) / layer.HumansSpawned )*100+ ";" +
                                   ( (double) layer.ZombiesKilled / layer.ZombiesSpawned)*100 + "\n";
                    
                    File.AppendAllText(Path.Combine(basePath, "stats.txt"), statsText);
                    Console.WriteLine("Statistics saved!");

                }
                //----------------------------------------------------------------------------------------------------------



                Console.WriteLine("The sun rises and the night of the living dead is over...\n" +
                                  (loopResults.Model.ExecutionAgentTypeGroups[new AgentType(typeof(Human))].Count + 
                                      loopResults.Model.ExecutionAgentTypeGroups[new AgentType(typeof(CustomHuman))].Count <= 0 
                                      ? "All humans were killed. All hope is gone."
                                      : "A small group of people survived. They will rebuild civilization."));

                Console.WriteLine("Humans: " +
                                  loopResults.Model.ExecutionAgentTypeGroups[new AgentType(typeof(Human))].Count);
                Console.WriteLine("DeadZombies: " +
                                  loopResults.Model.ExecutionAgentTypeGroups[new AgentType(typeof(DeadPlayer))].Count);
                Console.WriteLine("CustomHumans: " +
                                  loopResults.Model.ExecutionAgentTypeGroups[new AgentType(typeof(CustomHuman))].Count);
                Console.WriteLine("Zombies: " +
                                  loopResults.Model.ExecutionAgentTypeGroups[new AgentType(typeof(Zombie))].Count);
                Console.WriteLine("CustomHumans: " +
                                  loopResults.Model.ExecutionAgentTypeGroups[new AgentType(typeof(CustomHuman))].Count);
            }

        }
    }
}