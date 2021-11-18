using System;

namespace Obsidian.API.AI.Goal
{
    public class WanderGoal : BaseGoal
    {
        private readonly int wanderRange;

        public WanderGoal(IWorld world, int wanderRange) : base(world)
        {
            this.wanderRange = wanderRange;
        }

        public override GoalResult GetGoal(IEntity entity) 
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

            return new GoalResult
            {
                ActionTimeout = 5,
                Path = path.GetPath(entity.Position, target)
            };
        }
    }
}
