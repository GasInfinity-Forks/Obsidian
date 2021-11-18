using Obsidian.Net.Packets.Play.Clientbound;
using Obsidian.API;

namespace Obsidian.Entities;
public class PathfinderMob : Mob
{

    int ActivityTimer = 20 * 5; // 5 sec delay before starting activities.

    bool hasTarget = false;

    /// <summary>
    /// Speed in meters/second
    /// </summary>
    public int speed = 2;

    /// <summary>
    /// Destination
    /// </summary>
    public VectorF target;

    public PathfinderMob()
    {
        this.target = Position;
    }

    public override async Task TickAsync()
    {
        await base.TickAsync();

        if (hasTarget)
        {
            await MoveTo();
            if (VectorF.Distance(Position, this.target) < 1.0f)
            {
                hasTarget = false;
            }
        }

        if (ActivityTimer != 0)
        {
            ActivityTimer--;
        } 
        else
        {
            var nearby = World.GetEntitiesNear(Position);
            foreach (var target in nearby)
            {
                if (target.Type == API.EntityType.Player)
                {
                    this.target = target.Position;
                    hasTarget = true;
                }
            }
            ActivityTimer = 20 * 5; // Don't do anything for 5 seconds after this.
        }
    }

    public async Task MoveTo()
    {
        double stepInterval = this.speed / 20.0;
        var theta = GetTheta(this.target);
        var yaw = Angle.NormalizeToByte((float)((theta * 180 / Math.PI) - 90.0));
        float newX = (float)(stepInterval * Math.Cos(theta));
        float newZ = (float)(stepInterval * Math.Sin(theta));

        var deltaPos = new VectorF(newX, 0, newZ);
        await base.UpdateAsync(deltaPos + Position, yaw, this.Pitch, true);
    }

    public void StopMovement()
    {

    }

    public void LookAt(VectorF target)
    {
        var theta = GetTheta(target);
        Yaw = Angle.NormalizeToByte((float)((theta * 180 / Math.PI) - 90.0));

        World.Server.BroadcastPacket(new EntityHeadLook
        {
            EntityId = EntityId,
            HeadYaw = Yaw
        });
    }

    private double GetTheta(VectorF target)
    {
        float xDiff = target.X - Position.X;
        float zDiff = target.Z - Position.Z;
        return Math.Atan2(zDiff, xDiff);
    }
}
