namespace JAZG.Model.Players
{
    public class Zombie : Player
    {
        public override void Init(FieldLayer layer)
        {
            base.Init(layer);
            Energy = 15;
        }

        public void Tick()
        {
            base.Tick();

            // TODO Implement simple movements
            // TODO implement action upon meeting zombie using collisionHashEnvironment  functionalities
        }
    }
}