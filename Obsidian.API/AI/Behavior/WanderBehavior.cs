using System;
using Obsidian.API._Enums;

namespace Obsidian.API.AI.Behavior
{
    public class WanderBehavior : BaseBehavior
    {
        private readonly int wanderRange;

        public WanderBehavior(IWorld world, int wanderRange) : base(world)
        {
            this.wanderRange = wanderRange;
        }

        public override BehaviorResult GetBehavior(IEntity entity) 
        {
            Random r = new Random();
            VectorF target = entity.Position + (r.Next(-wanderRange, wanderRange), 0, r.Next(-wanderRange, wanderRange));

            AStarPath path = new AStarPath(world)
            {
                // TODO: get these properties from the mob.
                EntityCanClimbLadders = false,
                EntityCanClimbWalls = false,
                EntityCanFly = false,
                EntityCanSwim = true,
                EntityHeight = 2,
                MaxClimbHeight = 1,
                MaxFallHeight = 10,
                MaxRange = 10
            };

            return new BehaviorResult
            {
                ActionTimeout = 5,
                Action = BehaviorAction.Move,
                Path = path.GetPath(entity.Position, target)
            };
        }
    }
}
