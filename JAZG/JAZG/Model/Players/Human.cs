using System;
using Mars.Components.Agents;
using Mars.Components.Environments.Cartesian;
using Mars.Interfaces.Agents;
using Mars.Interfaces;

namespace JAZG.Model.Players
{
    public class Human : Player
    {
        public override void Init(FieldLayer layer)
        {
            base.Init(layer);
            Energy = 30;
        }
        public override void Tick()
        {
            base.Tick();
            Console.WriteLine("Hello world From Human!");
            
            // TODO Implement simple movements
            // TODO implement action upon meting zombie using collisionHashEnvironment  functionalities
        }
    }
}