using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityDoorHologramOverhaul.Events
{
    public static class LevelEvents
    {
        public static event Action OnLevelBuildDone;

        static LevelEvents()
        {
            LG_Factory.add_OnFactoryBuildDone(new Action(()=> { OnLevelBuildDone?.Invoke(); }));
        }
    }
}
