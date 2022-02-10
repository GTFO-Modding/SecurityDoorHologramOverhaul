using BepInEx;
using GameData;
using LevelGeneration;
using SecurityDoorHologramOverhaul.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SecurityDoorHologramOverhaul
{
    public static class DoorHologramManager
    {
        public static void ReadConfig()
        {
            var userConfig = Path.Combine(Paths.ConfigPath, "SecurityDoorHologram.json");
            if (File.Exists(userConfig))
            {
                _userConfig = JSON.Deserialize<DoorHologramUserConfig>(File.ReadAllText(userConfig));
            }

            if (MTFOUtil.IsLoaded && MTFOUtil.HasCustomContent)
            {
                var rundownConfig = Path.Combine(MTFOUtil.CustomPath, "SecurityDoorHologram.json");
                if (File.Exists(rundownConfig))
                {
                    _rundownConfig = JSON.Deserialize<DoorHologramRundownConfig>(File.ReadAllText(rundownConfig));
                }
            }
        }

        public static void SecurityDoorSpawned(LG_SecurityDoor door)
        {
            var updater = door.gameObject.AddComponent<DoorHologramUpdater>();
            updater.Setup(door);

            if (updater.IsSetup)
            {
                updater.SetDefaultState(_userConfig.DefaultState);

                foreach (var setting in _userConfig.DefaultSettings)
                    updater.SetStateData(setting.Target, setting);

                if (_rundownConfig != null)
                {
                    updater.SetDefaultState(_rundownConfig.DefaultState);

                    foreach (var setting in _rundownConfig.DefaultSettings)
                        updater.SetStateData(setting.Target, setting);

                    foreach (var levelOverride in _rundownConfig.LevelOverrides)
                        levelOverride.TryAddSettings(door, updater);

                    foreach (var puzzleOverride in _rundownConfig.ChainedPuzzleOverrides)
                        puzzleOverride.TryAddSettings(door, updater);
                }
            }
        }

        private static DoorHologramUserConfig _userConfig = new ();
        private static DoorHologramRundownConfig _rundownConfig = null;
    }

    public sealed class DoorHologramUserConfig
    {
        public DoorStateType DefaultState { get; set; } = DoorStateType.Locked_Alarm;
        public DoorStateData[] DefaultSettings { get; set; } = Array.Empty<DoorStateData>();
    }

    public sealed class DoorHologramRundownConfig
    {
        public DoorStateType DefaultState { get; set; } = DoorStateType.Locked_Alarm;
        public DoorStateData[] DefaultSettings { get; set; } = Array.Empty<DoorStateData>();
        public LevelOverride[] LevelOverrides { get; set; } = Array.Empty<LevelOverride>();
        public ChainPuzzleOverride[] ChainedPuzzleOverrides { get; set; } = Array.Empty<ChainPuzzleOverride>();
    }

    public sealed class ChainPuzzleOverride : OverrideConfig
    {
        public override bool IsTarget(LG_SecurityDoor door)
        {
            return PersistentIDs.Contains(door.LinkedToZoneData.ChainedPuzzleToEnter);
        }
    }

    public sealed class LevelOverride : OverrideConfig
    {
        public bool UsingLocalIndex { get; set; } = false;
        public eLocalZoneIndex[] LocalIndexes { get; set; } = Array.Empty<eLocalZoneIndex>();
        
        public override bool IsTarget(LG_SecurityDoor door)
        {
            var linkZone = door.Gate.m_linksTo.m_zone;
            if (UsingLocalIndex && !LocalIndexes.Contains(linkZone.LocalIndex))
            {
                return false;
            }

            var activeExpedition = RundownManager.ActiveExpedition;
            var layoutID = 0u;
            if (linkZone.IsMainDimension)
            {
                switch (linkZone.Layer.m_type)
                {
                    case LG_LayerType.MainLayer:
                        layoutID = activeExpedition.LevelLayoutData;
                        break;

                    case LG_LayerType.SecondaryLayer:
                        layoutID = activeExpedition.SecondaryLayout;
                        break;

                    case LG_LayerType.ThirdLayer:
                        layoutID = activeExpedition.ThirdLayout;
                        break;
                }
            }
            else
            {
                layoutID = linkZone.Dimension.DimensionData.LevelLayoutData;
            }
            
            
            return PersistentIDs.Contains(layoutID);
        }
    }

    public abstract class OverrideConfig
    {
        public uint[] PersistentIDs { get; set; } = Array.Empty<uint>();
        public DoorStateData[] Settings { get; set; } = Array.Empty<DoorStateData>();

        public void TryAddSettings(LG_SecurityDoor door, DoorHologramUpdater updater)
        {
            if (IsTarget(door))
            {
                foreach (var setting in Settings)
                {
                    updater.SetStateData(setting.Target, setting);
                }
            }
        }

        public virtual bool IsTarget(LG_SecurityDoor door)
        {
            return true;
        }
    }
}
