using Obsidian.API.AI.Goal;
using Obsidian.Concurrency;

namespace Obsidian.Entities.AI;

internal class Brain
{
    public ConcurrentHashSet<BaseGoal> Goals { get; internal set; } = new();


}
