namespace Obsidian.Entities.Ambient;

public class Bat : Ambient
{
    public bool IsHanging { get; }

    public Bat() : base()
    {
        Type = EntityType.Bat;
    }
}
