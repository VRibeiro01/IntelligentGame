﻿using System;
using Mars.Components.Environments.Cartesian;

namespace JAZG.Model.Objects
{
    public class Wall : Item
    {
        public override bool IsRoutable(ICharacter character)
        {
            // this means that the wall cannot be passed (??? right ??)
            return false;
        }

        public override CollisionKind? HandleCollision(ICharacter character)
        {
            Console.WriteLine("Collision with a wall has ocurred at position " + character.Position);
            return CollisionKind.Block;
            
        }

        public override VisibilityKind? HandleExploration(ICharacter explorer)
        {
            return VisibilityKind.Opaque;
        }

        
    }
}