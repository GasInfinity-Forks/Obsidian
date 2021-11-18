using System.Collections.Generic;


namespace Obsidian.API.AI.Goal
{
    /// <summary>
    /// Base class of AI Goals
    /// </summary>
    public partial class BaseGoal
    {
        protected readonly IWorld world;

        /// <summary>
        /// Ctor.
        /// </summary>
        public BaseGoal(IWorld world)
        {
            this.world = world;
        }

        public virtual GoalResult GetGoal(IEntity entity)
        {
            return new GoalResult
            {
                ActionTimeout = 1,
                Path = new()
            };
        }
        

        public struct GoalResult
        {

            public int ActionTimeout { get; set; }

            public List<Vector> Path { get; set; }
        }
    }
}
