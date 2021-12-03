using Obsidian.Concurrency;

namespace Obsidian.Entities.AI.Goal;

public class RandomLookAroundGoal : BaseGoal
{
    protected readonly Mob mob;

    protected double relX, relZ = 0.0;

    protected double lookTime = 20;

    public RandomLookAroundGoal(Mob mob)
    {
        this.mob = mob;
        Flags = new ConcurrentHashSet<Flag> { BaseGoal.Flag.MOVE, BaseGoal.Flag.LOOK };
    }

    public override void Start()
    {
        var randAngle = new Random().Next() * Math.PI * 2.0;
        this.relX = Math.Cos(randAngle);
        this.relZ = Math.Sin(randAngle);
        this.lookTime += new Random().Next(20);
    }

    public override void Tick()
    {
        --this.lookTime;
        //this.mob.LookAt(new VectorF((float)relX, 0, (float)relZ) + this.mob.Position);
    }
}
