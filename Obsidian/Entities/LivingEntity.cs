using Obsidian.Entities.AI;
using Obsidian.Net;

namespace Obsidian.Entities;

public class LivingEntity : Entity, ILiving
{
    public LivingBitMask LivingBitMask { get; set; }

    public uint ActiveEffectColor { get; private set; }

    public bool AmbientPotionEffect { get; set; }

    public int AbsorbedArrows { get; set; }

    public int AbsorbtionAmount { get; set; }

    public Vector BedBlockPosition { get; set; }
    public bool Alive => this.Health > 0f;

    private readonly Brain brain = new();

    internal float xRot = 0.0f;
    internal float yHeadRot = 0.0f;
    internal float yBodyRot = 0.0f;

    private int lerpSteps = 0;
    private double lerpX, lerpY, lerpZ = 0;
    private int noJumpDelay = 0;

    public void aiStep()
    {
        if (noJumpDelay > 0) --noJumpDelay;

        if (lerpSteps > 0)
        {
            var lerpedX = Position.X + (lerpX - Position.X) / (double)lerpSteps;
            var lerpedY = Position.Y + (lerpY - Position.Y) / (double)lerpSteps;
            var lerpedZ = Position.Z + (lerpZ - Position.Z) / (double)lerpSteps;
        }

        ServerAiStep();
    }

    internal virtual void ServerAiStep()
    {

    }

    public override async Task WriteAsync(MinecraftStream stream)
    {
        await base.WriteAsync(stream);

        await stream.WriteEntityMetdata(7, EntityMetadataType.Byte, (byte)this.LivingBitMask);

        await stream.WriteEntityMetdata(8, EntityMetadataType.Float, this.Health);

        await stream.WriteEntityMetdata(9, EntityMetadataType.VarInt, (int)this.ActiveEffectColor);

        await stream.WriteEntityMetdata(10, EntityMetadataType.Boolean, this.AmbientPotionEffect);

        await stream.WriteEntityMetdata(11, EntityMetadataType.VarInt, this.AbsorbedArrows);

        await stream.WriteEntityMetdata(12, EntityMetadataType.VarInt, this.AbsorbtionAmount);

        await stream.WriteEntityMetdata(13, EntityMetadataType.OptPosition, this.BedBlockPosition, this.BedBlockPosition != API.Vector.Zero);
    }

    public override void Write(MinecraftStream stream)
    {
        base.Write(stream);

        stream.WriteEntityMetadataType(7, EntityMetadataType.Byte);
        stream.WriteByte((byte)LivingBitMask);

        stream.WriteEntityMetadataType(8, EntityMetadataType.Float);
        stream.WriteFloat(Health);

        stream.WriteEntityMetadataType(9, EntityMetadataType.VarInt);
        stream.WriteVarInt((int)ActiveEffectColor);

        stream.WriteEntityMetadataType(10, EntityMetadataType.Boolean);
        stream.WriteBoolean(AmbientPotionEffect);

        stream.WriteEntityMetadataType(11, EntityMetadataType.VarInt);
        stream.WriteVarInt(AbsorbedArrows);

        stream.WriteEntityMetadataType(12, EntityMetadataType.VarInt);
        stream.WriteVarInt(AbsorbtionAmount);
        
        stream.WriteEntityMetadataType(13, EntityMetadataType.OptPosition);
        stream.WriteBoolean(BedBlockPosition != default);
        if (BedBlockPosition != default)
            stream.WritePositionF(BedBlockPosition);
    }
}
