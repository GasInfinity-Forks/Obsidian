using Obsidian.API;

namespace Obsidian.Entities.Projectile
{
    public class Fireworks : Entity
    {
        public ItemStack Item { get; private set; }
        public int Rotation { get; private set; }
    }
}
