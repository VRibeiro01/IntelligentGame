using Mars.Components.Layers;
namespace jazg.Model;

/// <summary>
///  Field, where all agents and entities live
///  Type Rasterlayer represents a n x m matrix. Allows processing of grid data
/// </summary>
public class FieldLayer : RasterLayer
{
    public override bool InitLayer(TInitData layerInitData, RegisterAgent registerAgentHandle, UnregisterAgent unregisterAgentHandle)
    {
        base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle);
        // Do your initialization logic here.
        return true;
    } 
}