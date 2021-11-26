namespace Obsidian.Entities.AI.Goal;

internal class WrappedGoal : BaseGoal, IEquatable<BaseGoal>
{
    private readonly BaseGoal goal;
    private readonly int priority;
    internal bool isRunning;

    public WrappedGoal(int priority, BaseGoal goal)
    {
        this.priority = priority;
        this.goal = goal;
    }

    public override void Start()
    {
        if (!isRunning)
        {
            isRunning = true;
            goal.Start();
        }
    }

    public override void Stop()
    {
        if (isRunning)
        {
            isRunning = false;
            goal.Stop();
        }
    }

    public bool CanBeReplacedBy(WrappedGoal newGoal) => IsInterruptable && newGoal.priority < priority;

    bool IEquatable<BaseGoal>.Equals(BaseGoal? other)
    {
        if (other is WrappedGoal o && GetHashCode() == o.GetHashCode()) { return true; }
        return false;
    }
}
