using System;
using Mars.Components.Agents;
using Mars.Interfaces.Agents;

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
        }
    }
}