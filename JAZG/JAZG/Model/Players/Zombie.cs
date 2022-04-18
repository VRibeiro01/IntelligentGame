using System;

namespace JAZG.Model.Players
{
    public class Zombie : Player
    {
        public override void Init(FieldLayer layer)
        {
            base.Init(layer);
            Energy = 15;
        }

        public override void Tick()
        {
            base.Tick();
            Console.WriteLine("Hello world From Zombie!");
            // TODO Implement simple movements
            // TODO implement action upon meting zomie using collisionHashEnvironment  functionalities
        }
    }
}