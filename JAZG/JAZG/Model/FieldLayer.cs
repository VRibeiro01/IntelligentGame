using System;
using System.Linq;
using JAZG.Model.Objects;
using JAZG.Model.Players;
using Mars.Components.Environments.Cartesian;
using Mars.Components.Layers;
using Mars.Core.Data;
using Mars.Interfaces.Data;
using Mars.Interfaces.Layers;

namespace JAZG.Model
{
    
/// <summary>
///  Field, where all agents and entities live
///  Type Rasterlayer represents a n x m matrix. Allows processing of grid data
/// </summary>
public class FieldLayer : RasterLayer
{
    
    // TODO add human and zombies to the field
 
    public override bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle,
        UnregisterAgent unregisterAgentHandle)
    {
        base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle); // base class requires init, too
            
        // the agent manager can create agents and initializes them as defined in the sim config
        var agentManager = layerInitData.Container.Resolve<IAgentManager>();
        var environment = new CollisionEnvironment<Player, Item>();
        
        //Create and register agents
        var human_agents = agentManager.Spawn<Human, FieldLayer >().ToList();
        var zombie_agents = agentManager.Spawn<Zombie, FieldLayer >().ToList();
        Console.WriteLine($"We created {human_agents.Count} human agents.");
        Console.WriteLine($"We created {zombie_agents.Count} zombie agents.");
            
        return true;
    }
}

}
