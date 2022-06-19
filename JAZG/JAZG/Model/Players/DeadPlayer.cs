using System;
using System.Linq;
using Mars.Components.Environments.Cartesian;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Environments;

namespace JAZG.Model.Players
{
    public class DeadPlayer : IAgent<FieldLayer>, ICharacter
    {
        private FieldLayer Layer { get; set; }

        public Guid ID { get; set; }

        public void Init(FieldLayer layer)
        {
            Layer = layer;
        }

        public void Tick()
        {
            //do nothing
        }

        public Position Position { get; set; }

        public double Extent { get; set; }

        public CollisionKind? HandleCollision(ICharacter other)
        {
            return CollisionKind.Pass;
        }

        public static void Spawn(FieldLayer layer, Player oldPlayer)
        {
            layer.AgentManager.Spawn<DeadPlayer, FieldLayer>(null,
                player => { player.Position = oldPlayer.Position; });
        }
    }
}