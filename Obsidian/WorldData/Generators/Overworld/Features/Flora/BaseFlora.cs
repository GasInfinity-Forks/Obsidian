using Obsidian.API;
using System;
using System.Collections.Generic;

namespace Obsidian.WorldData.Generators.Overworld.Features.Flora
{
    public abstract class BaseFlora
    {
        protected readonly World world;

        protected Material FloraMat { get; set; }

        protected int height = 1;

        protected List<Material> growsIn = new()
        {
            Material.Air
        };

        protected List<Material> growsOn = new()
        {
            Material.GrassBlock,
            Material.Dirt,
            Material.Podzol
        };

        protected BaseFlora(World world, Material mat = Material.RedTulip)
        {
            this.world = world;
            this.FloraMat = mat;
        }

        /// <summary>
        /// Place a grouping of plants in a circular patch.
        /// </summary>
        /// <param name="origin">Center of the grouping.</param>
        /// <param name="seed">World Seed.</param>
        /// <param name="radius">Radius of circular patch.</param>
        /// <param name="density">less dense: 1 < density < 10 :more dense.</param>
        public virtual void GenerateFlora(Vector origin, int seed, int radius, int density)
        {
            density = Math.Max(1, 10 - density);
            var seedRand = new Random(seed + origin.GetHashCode());

            for (int rx = -radius; rx <= radius + 1; rx++)
            {
                for (int rz = -radius; rz <= radius + 1; rz++)
                {
                    if ((rx * rx) + (rz * rz) <= (radius * radius))
                    {
                        int x = origin.X + rx;
                        int z = origin.Z + rz;
                        int y = world.GetWorldSurfaceHeight(x, z) ?? -1;
                        if (y == -1) { continue; }
                        bool isFlora = seedRand.Next(10) % density == 0;
                        var placeVec = new Vector(x, y + 1, z);
                        if (isFlora)
                        {
                            TryPlaceFlora(placeVec);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Place a single plant.
        /// </summary>
        /// <param name="placeVector">The position above the surface block.</param>
        /// <returns>Whether plant was planted.</returns>
        public virtual bool TryPlaceFlora(Vector placeVector)
        {
            if (GrowHeight(placeVector) >= height && ValidSurface(placeVector))
            {
                world.SetBlockUntracked(placeVector, new Block(FloraMat));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if the surface is compatible.
        /// </summary>
        /// <param name="loc">The position above the surface block.</param>
        /// <returns>Whether surface is compatible.</returns>
        protected virtual bool ValidSurface(Vector loc) => world.GetBlock(loc + Vector.Down) is Block b && growsOn.Contains(b.Material);

        /// <summary>
        /// Check free space above grow location.
        /// </summary>
        /// <param name="loc">Location to sample.</param>
        /// <returns>Count of vertical free space above plant.</returns>
        protected virtual int GrowHeight(Vector loc)
        {
            int freeSpace = 0;
            for (int y = 0; y < height; y++)
            {
                if (world.GetBlock(loc + (0, y, 0)) is Block above && growsIn.Contains(above.Material))
                {
                    freeSpace++;
                }
            }
            return freeSpace;
        }
    }
}
