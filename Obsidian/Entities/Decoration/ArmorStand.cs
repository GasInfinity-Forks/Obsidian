﻿namespace Obsidian.Entities.Decoration;

public class ArmorStand : LivingEntity
{
    public StandProperties StandProperties { get; set; }

    public Rotation Head { get; set; }
    public Rotation Body { get; set; }
    public Rotation LeftArm { get; set; }
    public Rotation RightArm { get; set; }
    public Rotation LeftLeg { get; set; }
    public Rotation RightLeft { get; set; }

    public ArmorStand()
    {
        Type = EntityType.ArmorStand;
    }
}

public struct StandProperties
{
    public bool IsSmall;
    public bool HasArms;
    public bool NoBasePlate;
    public bool SetMarker;
}
