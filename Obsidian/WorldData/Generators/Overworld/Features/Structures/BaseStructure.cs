using Obsidian.API;
using System.Collections.Generic;

namespace Obsidian.WorldData.Generators.Overworld.Features.Structures
{
    public abstract class BaseStructure
    {
        protected readonly World world;

        protected BaseStructure(World world)
        {
            this.world = world;
        }

        public virtual bool TryGenerateStructure(Vector origin)
        {
            return false;
        }
    }
}
