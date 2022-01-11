using Obsidian.ChunkData;
using Obsidian.WorldData.Generators.Overworld.Features.Trees;

namespace Obsidian.WorldData.Generators;

public class SkyblockGenerator : WorldGenerator
{
    public SkyblockGenerator() : base("skyblock")
    {

    }

    public override async Task<Chunk> GenerateChunkAsync(int x, int z, World world, Chunk? chunk = null)
    {
        if (chunk is { isGenerated: true })
            return chunk;

        if (chunk is null)
            chunk = new Chunk(x, z);

        for (int bx = 0; bx < 16; bx++)
            for (int bz = 0; bz < 16; bz++)
                if (bx % 4 == 0 && bz % 4 == 0)
                    for (int by = -64; by < 320; by += 4)
                        chunk.SetBiome(bx, by, bz, Biomes.Plains);

        if (!(x == 0 && z == 0))
        {
            chunk.isGenerated = true;
            return chunk;
        }

        BuildIsland((3, 3), chunk);
        BuildIsland((6, 3), chunk);
        BuildIsland((6, 6), chunk);

        var tree = new OakTree(world);
        await tree.TryGenerateTreeAsync(new Vector(3, 202, 3), 0);

        Vector chestPos = new(7, 203, 8);
        var chest = new Block(Material.Chest, 0);
        chunk.SetBlock(chestPos, chest);

        chunk.CalculateHeightmap();

        chunk.isGenerated = true;
        return chunk;
    }

    private static void BuildIsland((int x, int z) offset, Chunk chunk)
    {
        Block grass = new(Material.GrassBlock, 1);
        Block dirt = new(Material.Dirt);
        for (int ay = 200; ay < 203; ay++)
            for (int ax = 0 + offset.x; ax < 3 + offset.x; ax++)
                for (int az = 0 + offset.z; az < 3 + offset.z; az++)
                {
                    chunk.SetBlock(ax, ay, az, ay == 202 ? grass : dirt);
                }
    }
}
