using Mars.Components.Layers;
namespace jazg.Model.Players;

public class PlayerLayer : ILayer
{
    public override bool InitLayer(TInitData layerInitData, RegisterAgent registerAgentHandle, UnregisterAgent unregisterAgentHandle)
    {
        base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle);
        // Do your initialization logic here.
        return true;
    } 
}