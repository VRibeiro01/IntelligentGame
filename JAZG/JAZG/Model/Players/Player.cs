using Mars.Components.Agents;

namespace JAZG.Model.Players;

/// <summary>
/// abstract class to define player properties and main behaviour.
/// Zombies and humans inherit from this class and can specify their behaviour in their concrete classes
/// </summary>
public abstract class Player : IAgent<PlayerLayer>
{
    public void Init(MyLayer layer) 
    {
        // finalize the init process
    }

    public void Tick() 
    {
        // define your logic here
    }

    public Guid ID { get; set; }
}
}