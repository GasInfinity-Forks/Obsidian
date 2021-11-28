namespace Obsidian.Entities.AI.Control;

public class LookControl
{
    private readonly Mob mob;
    private float yMaxRotSpeed, xMaxRotAngle;
    private bool hasWanted;
    private double wantedX, wantedY, wantedZ;

    public LookControl(Mob mob)
    {
        this.mob = mob;
    }

    public void SetLookAt(VectorF vec) => SetLookAt(vec.X, vec.Y, vec.Z);

    public void SetLookAt(Entity entity, float yMaxRotSpeed, float xMaxRotAngle) => SetLookAt(entity.Position.X, GetWantedY(entity), entity.Position.Z, yMaxRotSpeed, xMaxRotAngle);

    public void SetLookAt(double x, double y, double z) => SetLookAt(x, y, z, mob.HeadRotSpeed, mob.MaxHeadXRot);

    public void SetLookAt(double x, double y, double z, float yMaxRotSpeed, float xMaxRotAngle)
    {
        this.wantedX = x;
        this.wantedY = y;
        this.wantedZ = z;
        this.yMaxRotSpeed = yMaxRotSpeed;
        this.xMaxRotAngle = xMaxRotAngle;
        this.hasWanted = true;
    }

    public void Tick()
    {
        mob.xRot = 0.0f;

        if (hasWanted)
        {
            hasWanted = false;
            mob.yHeadRot = RotateTowards(mob.yHeadRot, GetYRotDegrees(), yMaxRotSpeed);
            mob.xRot = RotateTowards(mob.xRot, GetXRotDegrees(), xMaxRotAngle);
        }
        else
        {
            mob.yHeadRot = RotateTowards(mob.yHeadRot, mob.yBodyRot, 10.0f);
        }

        if (mob.Navigator.IsNavigating)
        {
            mob.yHeadRot = RotateTowardsIfNecessary(mob.yHeadRot, mob.yBodyRot, mob.MaxHeadYRot);
        }
    }

    private static double GetWantedY(Entity entity)
    {
        // return entity is LivingEntity ? entity.Position.Y + entity.GetEyeHeight() : (entity.BoundingBox.minY + entity.BoundingBox.maxY) / 2.0D;
        return 1.0;
    }

    private float GetXRotDegrees()
    {
        var xDiff = wantedX - mob.Position.X;
        var yDiff = wantedY - (mob.Position.Y + mob.GetEyeHeight());
        var zDiff = wantedZ - mob.Position.Z;
        var c = (double)Math.Sqrt(xDiff * xDiff + zDiff * zDiff);
        return (float)-(Math.Atan2(yDiff, c) * 57.2957763671875D);
    }

    private float GetYRotDegrees()
    {
        var xDiff = wantedX - mob.Position.X;
        var zDiff = wantedZ - mob.Position.Z;
        return (float)(Math.Atan2(zDiff, xDiff) * 57.2957763671875D) - 90.0f;
    }

    private float RotateTowards(float yHeadRot, float xRot, float maxRotAngle)
    {
        float degreesDiff = NumericsHelper.DegreesDiff(yHeadRot, xRot);
        float degreesClamped = Math.Clamp(degreesDiff, -maxRotAngle, maxRotAngle);
        return yHeadRot + degreesClamped;
    }

    private float RotateTowardsIfNecessary(float yHeadRot, float yBodyRot, float maxRotAngle)
    {
        float degreesDiff = NumericsHelper.DegreesDiff(yHeadRot, yBodyRot);
        float degreesClamped = Math.Clamp(degreesDiff, -maxRotAngle, maxRotAngle);
        return yHeadRot - degreesClamped;
    }

}
