using LevelGeneration;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace SecurityDoorHologramOverhaul
{
    public sealed partial class DoorHologramUpdater : MonoBehaviour
    {
        [HideFromIl2Cpp]
        private void LevelBuildDone()
        {
            OnStateChange();
        }

        [HideFromIl2Cpp]
        internal void OnStateChange()
        {
            OnStateChange(_door.m_sync.GetCurrentSyncState(), false);
        }

        [HideFromIl2Cpp]
        internal void OnStateChange(pDoorState state, bool isDropin)
        {
            var newState = GetState(state);
            UpdateState(newState);
        }
    }
}
