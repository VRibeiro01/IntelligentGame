using System;
using Mars.Components.Agents;
using Mars.Interfaces.Agents;

namespace JAZG.Model.Players;

/// <summary>
/// abstract class to define player properties and main behaviour.
/// Zombies and humans inherit from this class and can specify their behaviour in their concrete classes
/// </summary>
public class Player : IAgent<FieldLayer>
{
 

    public void Init(FieldLayer layer)
    {
        // implement Init process
        throw new NotImplementedException();
    }

    public void Tick() 
    {
        // define your logic here
    }

    public Guid ID { get; set; }
}
