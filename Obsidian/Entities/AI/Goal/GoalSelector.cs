namespace Obsidian.Entities.AI.Goal;

public class GoalSelector
{
    private readonly Mob mob;

    internal BaseGoal activeGoal;

    public GoalSelector(Mob mob)
    {
        this.mob = mob;
        this.activeGoal = new BaseGoal();
    }

    public void Tick()
    {

    }
}
