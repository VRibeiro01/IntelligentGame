﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JAZG.Model;
using JAZG.Model.Players;
using Mars.Components.Starter;
using Mars.Interfaces.Model;

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
            var file = File.ReadAllText("config.json");
            var config = SimulationConfig.Deserialize(file);

            // Start Simulation
            var task = SimulationStarter.Start(description, config);
            var loopResults = task.Run();

            //var keys = loopResults.Model.ExecutionAgentTypeGroups.Keys.ToList();
            //var count = loopResults.Model.ExecutionAgentTypeGroups[keys[0]].Count;
            Console.WriteLine("The sun rises and the night of the living dead is over...\n" + 
                              (loopResults.Model.ExecutionAgentTypeGroups[new AgentType(typeof(Human))].Count <= 0
                                  ? "All humans were killed. All hope is gone."
                                  : "A small group of people survived. They will rebuild civilization."));
        }
    }
}