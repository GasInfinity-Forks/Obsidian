using Obsidian.Concurrency;

namespace Obsidian.Entities.AI.Goal;

public class RandomLookAroundGoal : BaseGoal
{
    protected readonly PathfinderMob mob;

    protected readonly float speed;

    protected readonly int interval;

    public RandomLookAroundGoal(PathfinderMob mob, float speed, int interval = 120)
    {
        this.mob = mob;
        this.speed = speed;
        this.interval = interval;
        Flags = new ConcurrentHashSet<Flag> { BaseGoal.Flag.MOVE };
    }
}
