using NetTopologySuite.Geometries;

namespace JAZG.Model.Players
{
    public class Human : Player
    {
        public override void Init(FieldLayer layer)
        {
            base.Init(layer);
            Energy = 30;
        }

        

        public void Tick()
        {
            base.Tick();


            // TODO Implement simple movements
            // TODO implement action upon meting zombie using collisionHashEnvironment  functionalities
        }
    }
}