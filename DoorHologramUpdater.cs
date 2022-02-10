using LevelGeneration;
using SecurityDoorHologramOverhaul.Events;
using System;
using System.Collections.Generic;
using System.Text;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace SecurityDoorHologramOverhaul
{
    public sealed partial class DoorHologramUpdater : MonoBehaviour
    {
        [HideFromIl2Cpp]
        public bool IsSetup { get; private set; } = false;
        [HideFromIl2Cpp]
        public bool HasStateSetting { get; private set; } = false;
        [HideFromIl2Cpp]
        public GameObject CurrentObject => _holoObject;
        [HideFromIl2Cpp]
        public DoorStateData CurrentData => _holoData;
        [HideFromIl2Cpp]
        public Material CurrentMaterial => _holoMat;

        [HideFromIl2Cpp]
        internal void Setup(LG_SecurityDoor secDoor)
        {
            if (IsSetup)
            {
                return;
            }

            if (!_sCached)
            {
                _sEmissive = Shader.PropertyToID("_EmissiveColor");
                _sLayerA = Shader.PropertyToID("_LayerA");
                _sLayerB = Shader.PropertyToID("_LayerB");
                _sLayerC = Shader.PropertyToID("_LayerC");
                _sCached = true;
            }

            var stateLength = Enum.GetValues(typeof(DoorStateType)).Length;
            _holoObjectList = new GameObject[stateLength];
            _holoDataList = new DoorStateData[stateLength];
            _holoMatList = new Material[stateLength];

            _door = secDoor;
            switch (_door.m_securityDoorType)
            {
                case eSecurityDoorType.Apex:
                case eSecurityDoorType.Bulkhead:
                    Logger.Error($"There is no door hologram on {_door.m_securityDoorType} door! DESTROYING HOLOGRAM SUPPORT! : {secDoor.name}");
                    Destroy(this);
                    return;
            }

            if (_door == null)
            {
                Logger.Error($"Setup was not called?");
                Destroy(this);
                return;
            }

            Transform hologramChild = null;

            var renderers = gameObject.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var renderer in renderers)
            {
                if (renderer.material.shader.name.Contains("Display_Hologram", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (hologramChild == null)
                    {
                        hologramChild = renderer.gameObject.transform.parent;
                    }

                    renderer.enabled = false;
                    renderer.forceRenderingOff = true;
                }
            }

            if (hologramChild == null)
            {
                Logger.Error($"Hologram screen was null?");
                Destroy(this);
                return;
            }
            var baseTransform = hologramChild;
            _hologramPrefab = Instantiate(hologramChild.gameObject, baseTransform.parent);
            _hologramPrefab.transform.position = baseTransform.position;
            _hologramPrefab.transform.rotation = baseTransform.rotation;
            _hologramPrefab.transform.localScale = baseTransform.localScale;

            LevelEvents.OnLevelBuildDone += LevelBuildDone;
            _door.m_sync.add_OnDoorStateChange((Action<pDoorState, bool>)OnStateChange);
            _door.m_locks.add_OnChainedPuzzleSolved(new Action(()=> { OnStateChange(); }));
            IsSetup = true;
        }

        [HideFromIl2Cpp]
        public void ForceState(DoorStateType type)
        {
            UpdateState(newState: type);
        }

        [HideFromIl2Cpp]
        private bool CreateHologram(DoorStateData data, out GameObject obj, out Material mat)
        {
            if (!IsSetup)
            {
                obj = null;
                mat = null;
                return false;
            }    

            try
            {
                var baseTransform = _hologramPrefab.transform;
                var newObj = Instantiate(_hologramPrefab, baseTransform.parent);
                newObj.transform.position = baseTransform.position;
                newObj.transform.rotation = baseTransform.rotation;
                newObj.transform.localScale = baseTransform.localScale;

                var renderer = newObj.GetComponentInChildren<MeshRenderer>(true);
                var newMat = new Material(renderer.material);
                renderer.material = newMat;
                renderer.enabled = true;
                renderer.forceRenderingOff = false;

                if (Textures.TryGet(data.Texture, out var texture))
                {
                    newMat.SetTexture("_EmissiveMap", texture);
                    newMat.SetVector(_sEmissive, new Vector4(data.Emission, data.Emission, data.Emission, 1.0f));
                    newMat.SetVector(_sLayerA, data.VectorA);
                    newMat.SetVector(_sLayerB, data.VectorB);
                    newMat.SetVector(_sLayerC, data.VectorC);
                }
                else
                {
                    Destroy(newMat);
                    Destroy(newObj);
                    throw new ArgumentException("data.Texture");
                }

                newObj.SetActive(false);

                obj = newObj;
                mat = renderer.material;
                return true;
            }
            catch(Exception e)
            {
                Logger.Error(e);
                obj = null;
                mat = null;
                return false;
            }
        }

        [HideFromIl2Cpp]
        internal void SetStateData(DoorStateType type, DoorStateData data)
        {
            if (!IsSetup)
            {
                return;
            }

            if (!Enum.IsDefined(typeof(DoorStateType), type))
            {
                Logger.Error("DoorState was not valid!");
                return;
            }

            var index = (int)type;
            if (_holoDataList[index] == null)
            {
                CreateHologram(data, out var obj, out var mat);
                _holoObjectList[index] = obj;
                _holoMatList[index] = mat;
            }

            _holoDataList[index] = data;
        }

        [HideFromIl2Cpp]
        internal void SetDefaultState(DoorStateType type)
        {
            if (!IsSetup)
            {
                return;
            }

            if (!Enum.IsDefined(typeof(DoorStateType), type))
            {
                Logger.Error("DoorState was not valid!");
                return;
            }

            var index = (int)type;
            _defaultStateOnMissing = index;
        }

        [HideFromIl2Cpp]
        private void UpdateState(DoorStateType newState)
        {
            var index = (int)newState;
            if (_holoDataList[index] == null)
            {
                if (_defaultStateOnMissing >= 0)
                {
                    newState = (DoorStateType)_defaultStateOnMissing;
                    index = _defaultStateOnMissing;
                }
                else
                {
                    return;
                }
            }

            if (_currentState != newState)
            {
                _currentState = newState;
                _holoData?.Blink.TryBlinkOutOrDisable(_holoObject);

                SetCurrentIndex(index);

                if (_holoData == null)
                {
                    HasStateSetting = false;
                    return;
                }

                _holoData.Blink.TryBlinkInOrEnable(_holoObject);
                HasStateSetting = true;
            }
        }

        [HideFromIl2Cpp]
        private void SetCurrentIndex(int index)
        {
            _holoObject = _holoObjectList[index];
            _holoData = _holoDataList[index];
            _holoMat = _holoMatList[index];
        }

        [HideFromIl2Cpp]
        private DoorStateType GetState(pDoorState state)
        {
            switch (state.status)
            {
                case eDoorStatus.None: //???
                    return DoorStateType.None;


                case eDoorStatus.Open:
                case eDoorStatus.Opening:
                    return DoorStateType.Opening;


                case eDoorStatus.Closed:
                case eDoorStatus.Unlocked:
                    if (_door.ActiveEnemyWaveData?.HasActiveEnemyWave ?? false)
                    {
                        return DoorStateType.Unlocked_MotionTrigger;
                    }
                    return DoorStateType.Unlocked;


                case eDoorStatus.Closed_LockedWithKeyItem:
                    return DoorStateType.Locked_Keycard;


                case eDoorStatus.Closed_LockedWithPowerGenerator:
                    return DoorStateType.Locked_Generator;


                case eDoorStatus.Closed_LockedWithNoKey:
                    return DoorStateType.Lockdowned;


                case eDoorStatus.ChainedPuzzleActivated:
                    if (_door.m_locks.ChainedPuzzleToSolve?.Data?.TriggerAlarmOnActivate ?? false)
                    {
                        if (_door.m_locks.ChainedPuzzleToSolve.Data.DisableSurvivalWaveOnComplete)
                        {
                            return DoorStateType.Locked_AlarmProgress;
                        }
                        return DoorStateType.Locked_ErrorAlarmProgress;
                    }
                    return DoorStateType.Locked_ScanProgress;


                case eDoorStatus.Closed_LockedWithChainedPuzzle:
                    return DoorStateType.Locked_Scan;


                case eDoorStatus.Closed_LockedWithChainedPuzzle_Alarm:
                    if (_door.m_locks.ChainedPuzzleToSolve?.Data?.DisableSurvivalWaveOnComplete ?? false)
                    {
                        return DoorStateType.Locked_Alarm;
                    }
                    return DoorStateType.Locked_ErrorAlarm;
            }

            return DoorStateType.None;
        }

        internal void OnDestroy()
        {
            LevelEvents.OnLevelBuildDone -= LevelBuildDone;

            _door = null;
            _hologramPrefab = null;

            foreach (var mat in _holoMatList)
            {
                Destroy(mat);
            }

            _holoMat = null;
            _holoData = null;
            _holoObject = null;
            _holoMatList = null;
            _holoDataList = null;
            _holoObjectList = null;
        }

        private static bool _sCached = false;
        private static int _sEmissive = 0;
        private static int _sLayerA = 0;
        private static int _sLayerB = 0;
        private static int _sLayerC = 0;

        private DoorStateType _currentState = DoorStateType.None;
        private int _defaultStateOnMissing = -1;
        private LG_SecurityDoor _door;
        private DoorStateData _holoData;
        private Material _holoMat;
        private GameObject _holoObject;

        private DoorStateData[] _holoDataList = Array.Empty<DoorStateData>();
        private Material[] _holoMatList = Array.Empty<Material>();
        private GameObject[] _holoObjectList = Array.Empty<GameObject>();

        private GameObject _hologramPrefab;
    }
}
