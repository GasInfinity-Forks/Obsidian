using Obsidian.API._Enums;
using System.Collections.Generic;


namespace Obsidian.API.AI.Behavior
{
    /// <summary>
    /// Base class of AI Behavior
    /// </summary>
    public partial class BaseBehavior
    {
        protected readonly IWorld world;

        /// <summary>
        /// Ctor.
        /// </summary>
        public BaseBehavior(IWorld world)
        {
            this.world = world;
        }

        public virtual BehaviorResult GetBehavior(IEntity entity)
        {
            return new BehaviorResult
            {
                Action = BehaviorAction.Idle,
                ActionTimeout = 1,
                Path = new()
            };
        }
        

        public struct BehaviorResult
        {
            public BehaviorAction Action { get; set; }

            public int ActionTimeout { get; set; }

            public List<Vector> Path { get; set; }
        }
    }
}
