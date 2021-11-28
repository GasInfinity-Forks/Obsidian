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

    protected bool jumping;
    public float xxa;
    public float yya;
    public float zza;
    public float yRotA;

    internal float xRot = 0.0f;
    internal float yRot = 0.0f;
    internal float yHeadRot = 0.0f;
    internal float yBodyRot = 0.0f;

    private int lerpSteps = 0;
    private double lerpX, lerpY, lerpZ = 0;
    protected double lerpYRot;
    protected double lerpXRot;
    protected double lyHeadRot;
    protected int lerpHeadSteps;

    private int noJumpDelay = 0;

    public void Tick()
    {
        // base.Tick();
        AiStep();

    }

    public void AiStep()
    {
        if (noJumpDelay > 0) --noJumpDelay;

        if (lerpSteps > 0)
        {
            var lerpedX = Position.X + (lerpX - Position.X) / (double)lerpSteps;
            var lerpedY = Position.Y + (lerpY - Position.Y) / (double)lerpSteps;
            var lerpedZ = Position.Z + (lerpZ - Position.Z) / (double)lerpSteps;
            double wrapped = NumericsHelper.WrapDegrees(lerpYRot - (double)yRot);
            yRot = (float)((double)yRot + wrapped / (double)lerpSteps);
            xRot = (float)((double)xRot + (lerpXRot - (double)xRot) / (double)lerpSteps);
            --lerpSteps;
            Position = new VectorF((float)lerpedX, (float)lerpedY, (float)lerpedZ);
            Rotation = new Rotation()
            {
                X = xRot,
                Y = yRot
            };
        }
        else
        {
            DeltaMovement *= 0.98f;
        }

        if (lerpHeadSteps > 0)
        {
            yHeadRot = (float)((double)yHeadRot + NumericsHelper.WrapDegrees(lyHeadRot - (double)yHeadRot) / (double)lerpHeadSteps);
            --lerpHeadSteps;
        }

        float deltaMovX = (float)(Math.Abs(DeltaMovement.X) < 0.003D ? 0.0D : DeltaMovement.X);
        float deltaMovY = (float)(Math.Abs(DeltaMovement.Y) < 0.003D ? 0.0D : DeltaMovement.Y);
        float deltaMovZ = (float)(Math.Abs(DeltaMovement.Z) < 0.003D ? 0.0D : DeltaMovement.Z);
        DeltaMovement = new VectorF(deltaMovX, deltaMovY, deltaMovZ);

        // AI
        if (Health <= 0.0)
        {
            jumping = false;
            xxa = 0.0F;
            zza = 0.0F;
            yRotA = 0.0F;
        }
        else
        {
            ServerAiStep();
        }

        // Jumping
        /*        if (this.jumping)
                {
                    if (this.waterHeight > 0.0D && (!this.onGround || this.waterHeight > 0.4D))
                    {
                        this.jumpInLiquid(FluidTags.WATER);
                    }
                    else if (this.isInLava())
                    {
                        this.jumpInLiquid(FluidTags.LAVA);
                    }
                    else if ((this.onGround || this.waterHeight > 0.0D && this.waterHeight <= 0.4D) && this.noJumpDelay == 0)
                    {
                        this.jumpFromGround();
                        this.noJumpDelay = 10;
                    }
                }
                else
                {
                    this.noJumpDelay = 0;
                }*/

        // Traveling
        this.xxa *= 0.98F;
        this.zza *= 0.98F;
        this.yRotA *= 0.9F;
        //Travel(new Vec3((double)xxa, (double)yya, (double)zza));

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
