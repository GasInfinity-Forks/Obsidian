using Obsidian.Concurrency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obsidian.Entities.AI.Goal
{
    public class BaseGoal
    {
        protected ConcurrentHashSet<Flag> Flags { get; set; } = new();

        public bool CanUse { get; set; } = true;

        public virtual void Start() { }

        public virtual void Stop() { }

        public virtual void Tick() { }

        public enum Flag
        {
            MOVE,
            LOOK,
            JUMP,
            TARGET
        }
    }
}
