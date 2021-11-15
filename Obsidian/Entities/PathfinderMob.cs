using Obsidian.Net.Packets.Play.Clientbound;
using System.Threading.Tasks;
using System;
using Obsidian.API;

namespace Obsidian.Entities
{
    public class PathfinderMob : Mob
    {

        int ActivityTimer = 0;

        /// <summary>
        /// Speed in meters/second
        /// </summary>
        public int speed = 1;

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

            if (VectorF.Distance(Position, this.target) < 2.0f)
            {
                StopMovement();
                this.target = Position;
            }
            else
            {
                MoveTo();
            }


            if (ActivityTimer != 0)
            {
                ActivityTimer--;
            } 
            else
            {
                // Look at player
                var nearby = World.GetEntitiesNear(Position);
                foreach (var target in nearby)
                {
                    if (target.Type == API.EntityType.Player)
                    {
                        this.target = target.Position;
                        ActivityTimer = 20 * 10; // Don't do anything for 10 seconds after this.
                    }
                }
            }
        }

        public void MoveTo()
        {
            double stepInterval = this.speed / 20.0;
            var theta = GetTheta(this.target);
            Yaw = Angle.NormalizeToByte((float)((theta * 180 / Math.PI) - 90.0));
            double newX = stepInterval * Math.Cos(theta);
            double newZ = stepInterval * Math.Sin(theta);

            short deltaX = (short)((newX * 32) * 128);
            short deltaZ = (short)((newZ * 32) * 128);
            short deltaY = 0;

            LastPosition = Position;
            World.Server.BroadcastPacket(new EntityHeadLook
            {
                EntityId = EntityId,
                HeadYaw = Yaw
            });

            World.Server.BroadcastPacket(new EntityPositionAndRotation
            {
                EntityId = EntityId,
                Delta = new Vector(deltaX, deltaY, deltaZ),
                Yaw = Yaw
            });
        }

        public void StopMovement()
        {
            World.Server.BroadcastPacket(new EntityPositionAndRotation
            {
                EntityId = EntityId,
                Delta = Vector.Zero,
                Yaw = Yaw
            });
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
}
