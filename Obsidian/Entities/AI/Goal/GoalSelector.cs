using Obsidian.Concurrency;

namespace Obsidian.Entities.AI.Goal;

public class GoalSelector
{
    private static readonly WrappedGoal NO_GOAL =
        new WrappedGoal(int.MaxValue, new BaseGoal())
        {
            CanUse = false,
            isRunning = false
        };

    private Dictionary<BaseGoal.Flag, WrappedGoal> lockedFlags = new();

    private HashSet<BaseGoal.Flag> disabledFlags = new();

    private List<BaseGoal> availableGoals = new();

    private int newGoalRate = 3;

    public void AddGoal(int priority, BaseGoal goal)
    {
        availableGoals.Add(new WrappedGoal(priority, goal));
    }

    public void RemoveGoal(BaseGoal goal)
    {
        availableGoals.Remove(goal);
    }

    public void Tick()
    {
        var runningGoals = GetRunningGoals();

        // Stop any goal that have become disabled.
        runningGoals.Where(g => disabledFlags.Intersect(g.Flags).Any()).ForEach(goal => goal.Stop());

        // Unlock finished Goals.
        lockedFlags.Where(lf => !lf.Value.isRunning).ToList().ForEach(lf => lockedFlags.Remove(lf.Key));


        // Update Goals
        availableGoals.Where(g => g is WrappedGoal goal && !goal.isRunning)
            .Where(g => !disabledFlags.Intersect(g.Flags).Any())
            .Where(g => g is WrappedGoal goal && goal.Flags.All(f => lockedFlags.GetValueOrDefault(f, NO_GOAL).CanBeReplacedBy(goal)))
            .Where(g => g.CanUse)
            .ForEach(g =>
            {
                WrappedGoal goal = g as WrappedGoal ?? NO_GOAL;
                goal.Flags.ForEach(f =>
                {
                    WrappedGoal notGoal = lockedFlags.GetValueOrDefault(f, NO_GOAL);
                    notGoal.Stop();
                    lockedFlags.Add(f, goal);
                });
                goal.Start();
            });

        GetRunningGoals().ForEach(g => g.Tick());
    }

    internal List<WrappedGoal> GetRunningGoals() => (availableGoals.Where(g => g is WrappedGoal goal && goal.isRunning) as List<WrappedGoal>) ?? new List<WrappedGoal>() { NO_GOAL };

    internal void DisableControlFlag(BaseGoal.Flag flag) => disabledFlags.Add(flag);

    internal void EnableControlFlag(BaseGoal.Flag flag) => disabledFlags.Remove(flag);

    internal void SetControlFlag(BaseGoal.Flag flag, bool enabled)
    {
        if (enabled)
            EnableControlFlag(flag);
        else
            DisableControlFlag(flag);
    }
}
