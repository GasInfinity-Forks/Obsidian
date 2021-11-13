using Obsidian.API;
using System;

namespace Obsidian.WorldData.Generators.Overworld.Features.Structures
{
    public class LakeStructure : BaseStructure
    {

        public LakeStructure(World world) : base(world)
        {
        }

        public override bool TryGenerateStructure(Vector origin)
        {
            int radius = 7;

            // Scan for lowest terrain level
            int lowestY = int.MaxValue;
            for (int rz = 0; rz <= radius * 2; rz++)
            {
                for (int rx = 0; rx <= radius * 2; rx++)
                {
                    int x = origin.X - radius + rx;
                    int z = origin.Z - radius + rz;
                    lowestY = world.GetWorldSurfaceHeight(x, z) is int wsh && wsh < lowestY ? wsh : lowestY;
                    if (lowestY == int.MaxValue || lowestY == 0) { return false; } // Hit edge of the world. Bail out.
                }
            }

#pragma warning disable CS8629 // We've already determined we're not outside the bounds of the world.
            for (int rx = -radius; rx < radius+1; rx++)
            {
                for (int rz = -radius; rz < radius+1; rz++)
                {
                    // Inside the circle
                    if ((rx * rx) + (rz * rz) <= (radius * radius))
                    {
                        int x = origin.X + rx;
                        int z = origin.Z + rz;
                        int depthX = radius - Math.Abs(rx);
                        int depthZ = radius - Math.Abs(rz);
                        int depthY = (int)((depthX + depthZ) / (radius / 1.85));
                        for (int y = lowestY; y > lowestY - depthY; y--)
                        {
                            world.SetBlockUntracked(new Vector(x, y, z), new Block(Material.Lava));
                        }
                        world.SetBlockUntracked(new Vector(x, lowestY + 1, z), new Block(Material.Air));
                        world.SetBlockUntracked(new Vector(x, lowestY + 2, z), new Block(Material.Air));
                    }
                    // Outside the circle
                    else
                    {
                        // Check if surface
                        // Else replace with dirt
                    }
                }
            }

            return true;
#pragma warning restore CS8629
        }
    }
}
