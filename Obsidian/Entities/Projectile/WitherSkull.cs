namespace Obsidian.Entities.Projectile;

public class WitherSkull : Entity
{
    public bool Invulnerable { get; private set; }

    public WitherSkull()
    {
        Type = EntityType.WitherSkull;
    }
}
