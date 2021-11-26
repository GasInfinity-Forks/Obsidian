using Obsidian.Entities.AI.Control;
using Obsidian.Entities.AI.Goal;
using Obsidian.Net;

namespace Obsidian.Entities;

public class Mob : LivingEntity
{
    public MobBitmask MobBitMask { get; set; } = MobBitmask.None;

    protected readonly GoalSelector goalSelector;

    protected readonly LookControl lookControl;

    protected List<BaseGoal> Goals { get; set; } = new();

    public Mob() : base()
    {
        goalSelector = new();
        lookControl = new(this);
        MaxHeadXRot = 40;
        MaxHeadYRot = 75;
        HeadRotSpeed = 10;
    }

    public void Tick()
    {
        // base.Tick();
        UpdateControlFlags();

    }

    public void UpdateControlFlags()
    {
        // check in boat
        // check controlling passenger
        bool notInBoat = true;
        bool notPassenger = true;

        goalSelector.SetControlFlag(BaseGoal.Flag.MOVE, notPassenger);
        goalSelector.SetControlFlag(BaseGoal.Flag.JUMP, notPassenger && notInBoat);
        goalSelector.SetControlFlag(BaseGoal.Flag.LOOK, notPassenger);
    }

    internal override void ServerAiStep()
    {
        //checkDespawn();
        //sensing.Tick();
        //targetSelector.Tick();
        goalSelector.Tick();
        //navigation.Tick();
        CustomServerAiStep();
        //moveControl.Tick();
        lookControl.Tick();
        //jumpControl.Tick();
    }

    protected virtual void CustomServerAiStep()
    {

    }


    public override async Task WriteAsync(MinecraftStream stream)
    {
        await base.WriteAsync(stream);

        await stream.WriteEntityMetdata(14, EntityMetadataType.Byte, this.MobBitMask);
    }

    public override void Write(MinecraftStream stream)
    {
        base.Write(stream);

        stream.WriteEntityMetadataType(14, EntityMetadataType.Byte);
        stream.WriteByte((byte)MobBitMask);
    }


}

[Flags]
public enum MobBitmask
{
    None = 0x00,
    NoAi = 0x01,
    LeftHanded = 0x02,
    Agressive = 0x04
}
