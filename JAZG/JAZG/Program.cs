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
    /// <summary>
    ///     Class that sets up model, configures the scenario and starts the simulation
    /// </summary>
    internal static class Program
    {
        public static void Main(string[] args)
        {
            // TODO Für das Training: for-Schleife um Spiel mehrmals zu starten
            Console.WriteLine("Hello world from Main!");
            
            
            //TODO change basePath
            var basePath = @"..\..\..\Resources";
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "..\\..\\..\\..\\Visualization\\main.py";
            bool exists = File.Exists(start.FileName);
            start.Arguments = "";
            //start.WorkingDirectory = "..\\..\\..\\..\\Visualization";
            start.UseShellExecute = true;
            Process.Start(start); //Comment this for no visualization
            
            var description = new ModelDescription();

            description.AddLayer<FieldLayer>();

            // Add agents to model
            description.AddAgent<Human, FieldLayer>();
            description.AddAgent<Zombie, FieldLayer>();

            // Add entities to model
            description.AddAgent<Wall, FieldLayer>();
            description.AddAgent<Gun, FieldLayer>();
            description.AddAgent<Food, FieldLayer>();
            description.AddAgent<DeadPlayer, FieldLayer>();
            description.AddAgent<M16, FieldLayer>();
            // Get model configuration
            var file = File.ReadAllText("config.json");
            var config = SimulationConfig.Deserialize(file);

            // Start Simulation
            var task = SimulationStarter.Start(description, config);
            var loopResults = task.Run();

            // Serialize QTable 
            FieldLayer layer = (FieldLayer) loopResults.Model.Layers.Values.First();
            
            if (layer.learningMode > 0)
            {
                for (int i = 0; i < layer.amountOfMinds; i++)
                {
                    layer.QHumanLearningList[i].Serialize(
                        Path.Combine(basePath, "HumanLearning" + i + ".txt"));
                }
            }

            // TODO Add training loop to program and current learning interation to statistics
            // Save game statistics in file
            if (layer.SaveStats)
            {
                var statsText = "\n" + (double)(layer.HumansSpawned - layer.HumansKilled) / layer.HumansSpawned +";"+
                                (double) layer.ZombiesKilled / layer.ZombiesSpawned ;
                File.AppendAllText(Path.Combine(basePath, "stats.txt"), statsText);
                Console.WriteLine("Statistics saved!");
                
            }
            Console.WriteLine("The sun rises and the night of the living dead is over...\n" +
                              (loopResults.Model.ExecutionAgentTypeGroups[new AgentType(typeof(Human))].Count <= 0
                                  ? "All humans were killed. All hope is gone."
                                  : "A small group of people survived. They will rebuild civilization."));

            Console.WriteLine("Humans: " +
                              loopResults.Model.ExecutionAgentTypeGroups[new AgentType(typeof(Human))].Count);
            Console.WriteLine("Zombies: " +
                              loopResults.Model.ExecutionAgentTypeGroups[new AgentType(typeof(Zombie))].Count);
        }
    }
}