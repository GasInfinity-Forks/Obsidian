using Obsidian.Entities.AI.Goal;

namespace Obsidian.Entities.Animal;

public class Cow : Animal
{
    public Cow()
    {
        Type = EntityType.Cow;
        Goals.Add(new RandomLookAroundGoal(this));
    }
}
