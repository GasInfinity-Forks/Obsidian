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

    internal float yHeadRot = 0.0f;
    internal float yHeadRotO = 0.0f;
    internal float yBodyRot = 0.0f;
    internal float yBodyRotO = 0.0f;

    private int lerpSteps = 0;
    private double lerpX, lerpY, lerpZ = 0;
    protected double lerpYRot;
    protected double lerpXRot;
    protected double lyHeadRot;
    protected int lerpHeadSteps;

    protected float oRun;
    protected float run;

    private int noJumpDelay = 0;

    public override async Task TickAsync()
    {
        await base.TickAsync();
        AiStep();

        double xDiff = this.Position.X - this.xo;
        double zDiff = this.Position.Z - this.zo;
        float c2 = (float)(xDiff * xDiff + zDiff * zDiff);
        float bodyRot = this.yBodyRot;
        float var16 = 0.0F;
        this.oRun = this.run;
        float runOffset = 0.0F;
        if (c2 > 0.0025000002F)
        {
            runOffset = 1.0F;
            var16 = (float)Math.Sqrt((double)c2) * 3.0F;
            float angleC = (float)Math.Atan2(zDiff, xDiff) * 57.295776F - 90.0F;
            float var10 = Math.Abs(NumericsHelper.WrapDegrees(this.yRot) - angleC);
            if (95.0F < var10 && var10 < 265.0F)
            {
                bodyRot = angleC - 180.0F;
            }
            else
            {
                bodyRot = angleC;
            }
        }

        if (!this.OnGround)
        {
            runOffset = 0.0F;
        }

        this.run += (runOffset - this.run) * 0.3F;

        var16 = this.TickHeadTurn(bodyRot, var16);

        RangeChecks();


    }

    protected float TickHeadTurn(float var1, float var2)
    {
        float var3 = NumericsHelper.WrapDegrees(var1 - this.yBodyRot);
        this.yBodyRot += var3 * 0.3F;
        float var4 = NumericsHelper.WrapDegrees(this.yRot - this.yBodyRot);
        bool var5 = var4 < -90.0F || var4 >= 90.0F;
        if (var4 < -75.0F)
        {
            var4 = -75.0F;
        }

        if (var4 >= 75.0F)
        {
            var4 = 75.0F;
        }

        this.yBodyRot = this.yRot - var4;
        if (var4 * var4 > 2500.0F)
        {
            this.yBodyRot += var4 * 0.2F;
        }

        if (var5)
        {
            var2 *= -1.0F;
        }

        return var2;
    }

    private void RangeChecks()
    {
        // Range checks
        while (this.yRot - this.yRotO < -180.0F)
        {
            this.yRotO -= 360.0F;
        }

        while (this.yRot - this.yRotO >= 180.0F)
        {
            this.yRotO += 360.0F;
        }

        while (this.yBodyRot - this.yBodyRotO < -180.0F)
        {
            this.yBodyRotO -= 360.0F;
        }

        while (this.yBodyRot - this.yBodyRotO >= 180.0F)
        {
            this.yBodyRotO += 360.0F;
        }

        while (this.xRot - this.xRotO < -180.0F)
        {
            this.xRotO -= 360.0F;
        }

        while (this.xRot - this.xRotO >= 180.0F)
        {
            this.xRotO += 360.0F;
        }

        while (this.yHeadRot - this.yHeadRotO < -180.0F)
        {
            this.yHeadRotO -= 360.0F;
        }

        while (this.yHeadRot - this.yHeadRotO >= 180.0F)
        {
            this.yHeadRotO += 360.0F;
        }
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
        /* if (this.jumping)
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

    public void Travel(VectorF var1)
    {
        double var2;
        float var8;
        if (true)
        {
            var2 = 0.08D;
            bool var4 = DeltaMovement.Y <= 0.0D;
            if (var4)
            { // && this.hasEffect(MobEffects.SLOW_FALLING)) {
                var2 = 0.01D;
                this.FallDistance = 0.0F;
            }

            double var5;
            float var7;
            double var12;
            if (!this.IsInWater || this is Player) // && ((Player)this).abilities.flying)
            {
                if (this.IsInLava && this is not Player) // || !((Player)this).abilities.flying))
                {
                    var5 = this.Position.Y;
                    this.moveRelative(0.02F, var1);
                    this.move(MoverType.SELF, DeltaMovement);
                    DeltaMovement *= 0.5f;
/*                    if (!this.isNoGravity())
                    {
                        DeltaMovement *= new VectorF(0, (float)(-var2 / 4.0f), 0);
                    }*/

                    VectorF var25 = DeltaMovement;
                    if (IsFree(var25.Z, var25.Y + 0.6000000238418579D - this.Position.Y + var5, var25.Z)) // && this.horizontalCollision)
                    {
                        DeltaMovement = new VectorF(var25.X, 0.30000001192092896f, var25.Z);
                    }

                }
                else if (this.FlyingWithElytra)
                {
                    VectorF var21 = DeltaMovement;
                    if (var21.Y > -0.5D)
                    {
                        this.FallDistance = 1.0F;
                    }

                    VectorF var6 = this.GetLookDirection();
                    var7 = this.xRot * 0.017453292F;
                    double var26 = Math.Sqrt(var6.X * var6.X + var6.Z * var6.Z);
                    double var29 = Math.Sqrt(getHorizontalDistanceSqr(var21));
                    var12 = var6.length();
                    float var14 = Mth.cos(var7);
                    var14 = (float)((double)var14 * (double)var14 * Math.min(1.0D, var12 / 0.4D));
                    var21 = this.getDeltaMovement().add(0.0D, var2 * (-1.0D + (double)var14 * 0.75D), 0.0D);
                    double var15;
                    if (var21.y < 0.0D && var26 > 0.0D)
                    {
                        var15 = var21.y * -0.1D * (double)var14;
                        var21 = var21.add(var6.x * var15 / var26, var15, var6.z * var15 / var26);
                    }

                    if (var7 < 0.0F && var26 > 0.0D)
                    {
                        var15 = var29 * (double)(-Mth.sin(var7)) * 0.04D;
                        var21 = var21.add(-var6.x * var15 / var26, var15 * 3.2D, -var6.z * var15 / var26);
                    }

                    if (var26 > 0.0D)
                    {
                        var21 = var21.add((var6.x / var26 * var29 - var21.x) * 0.1D, 0.0D, (var6.z / var26 * var29 - var21.z) * 0.1D);
                    }

                    this.setDeltaMovement(var21.multiply(0.9900000095367432D, 0.9800000190734863D, 0.9900000095367432D));
                    this.move(MoverType.SELF, this.getDeltaMovement());
                    if (this.horizontalCollision && !this.level.isClientSide)
                    {
                        var15 = Math.sqrt(getHorizontalDistanceSqr(this.getDeltaMovement()));
                        double var17 = var29 - var15;
                        float var19 = (float)(var17 * 10.0D - 3.0D);
                        if (var19 > 0.0F)
                        {
                            this.playSound(this.getFallDamageSound((int)var19), 1.0F, 1.0F);
                            this.hurt(DamageSource.FLY_INTO_WALL, var19);
                        }
                    }

                    if (this.onGround && !this.level.isClientSide)
                    {
                        this.setSharedFlag(7, false);
                    }
                }
                else
                {
                    BlockPos var24 = new BlockPos(this.x, this.getBoundingBox().minY - 1.0D, this.z);
                    float var22 = this.level.getBlockState(var24).getBlock().getFriction();
                    var7 = this.onGround ? var22 * 0.91F : 0.91F;
                    this.moveRelative(this.getFrictionInfluencedSpeed(var22), var1);
                    this.setDeltaMovement(this.handleOnClimbable(this.getDeltaMovement()));
                    this.move(MoverType.SELF, this.getDeltaMovement());
                    Vec3 var27 = this.getDeltaMovement();
                    if ((this.horizontalCollision || this.jumping) && this.onLadder())
                    {
                        var27 = new Vec3(var27.x, 0.2D, var27.z);
                    }

                    double var28 = var27.y;
                    if (this.hasEffect(MobEffects.LEVITATION))
                    {
                        var28 += (0.05D * (double)(this.getEffect(MobEffects.LEVITATION).getAmplifier() + 1) - var27.y) * 0.2D;
                        this.fallDistance = 0.0F;
                    }
                    else if (this.level.isClientSide && !this.level.hasChunkAt(var24))
                    {
                        if (this.y > 0.0D)
                        {
                            var28 = -0.1D;
                        }
                        else
                        {
                            var28 = 0.0D;
                        }
                    }
                    else if (!this.isNoGravity())
                    {
                        var28 -= var2;
                    }

                    this.setDeltaMovement(var27.x * (double)var7, var28 * 0.9800000190734863D, var27.z * (double)var7);
                }
            }
            else
            {
                var5 = this.y;
                var7 = this.isSprinting() ? 0.9F : this.getWaterSlowDown();
                var8 = 0.02F;
                float var9 = (float)EnchantmentHelper.getDepthStrider(this);
                if (var9 > 3.0F)
                {
                    var9 = 3.0F;
                }

                if (!this.onGround)
                {
                    var9 *= 0.5F;
                }

                if (var9 > 0.0F)
                {
                    var7 += (0.54600006F - var7) * var9 / 3.0F;
                    var8 += (this.getSpeed() - var8) * var9 / 3.0F;
                }

                if (this.hasEffect(MobEffects.DOLPHINS_GRACE))
                {
                    var7 = 0.96F;
                }

                this.moveRelative(var8, var1);
                this.move(MoverType.SELF, this.getDeltaMovement());
                Vec3 var10 = this.getDeltaMovement();
                if (this.horizontalCollision && this.onLadder())
                {
                    var10 = new Vec3(var10.x, 0.2D, var10.z);
                }

                this.setDeltaMovement(var10.multiply((double)var7, 0.800000011920929D, (double)var7));
                Vec3 var11;
                if (!this.isNoGravity() && !this.isSprinting())
                {
                    var11 = this.getDeltaMovement();
                    if (var4 && Math.abs(var11.y - 0.005D) >= 0.003D && Math.abs(var11.y - var2 / 16.0D) < 0.003D)
                    {
                        var12 = -0.003D;
                    }
                    else
                    {
                        var12 = var11.y - var2 / 16.0D;
                    }

                    this.setDeltaMovement(var11.x, var12, var11.z);
                }

                var11 = this.getDeltaMovement();
                if (this.horizontalCollision && this.isFree(var11.x, var11.y + 0.6000000238418579D - this.y + var5, var11.z))
                {
                    this.setDeltaMovement(var11.x, 0.30000001192092896D, var11.z);
                }
            }
        }

        this.animationSpeedOld = this.animationSpeed;
        var2 = this.x - this.xo;
        double var20 = this.z - this.zo;
        double var23 = this instanceof FlyingAnimal ? this.y - this.yo : 0.0D;
        var8 = Mth.sqrt(var2 * var2 + var23 * var23 + var20 * var20) * 4.0F;
        if (var8 > 1.0F)
        {
            var8 = 1.0F;
        }

        this.animationSpeed += (var8 - this.animationSpeed) * 0.4F;
        this.animationPosition += this.animationSpeed;
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
