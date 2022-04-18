using System;
using System.IO;
using JAZG.Model;
using JAZG.Model.Objects;
using JAZG.Model.Players;
using Mars.Components.Starter;
using Mars.Interfaces.Model;
using NetTopologySuite.Index.HPRtree;

namespace JAZG
{
    /// <summary>
    ///     Class that sets up model, configures the scenerario and starts the simulation
    /// </summary>
    internal static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello world from Main!");
            var description = new ModelDescription();

            description.AddLayer<FieldLayer>();
            
            // Add agents to model
            description.AddAgent<Human, FieldLayer>();
            description.AddAgent<Zombie, FieldLayer>();
            
           
            
            // Get model configuration
            var file = File.ReadAllText(@"C:\Users\vivia\mars\jazg\JAZG\JAZG\config.json");
            var config = SimulationConfig.Deserialize(file);
            
            // Start Simulation
            var task = SimulationStarter.Start(description, config);
            var loopResults = task.Run();

            Console.WriteLine("Successful!!");
        }
    }
}