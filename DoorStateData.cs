using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SecurityDoorHologramOverhaul
{
    public sealed class DoorStateData
    {
        public DoorStateType Target { get; set; } = DoorStateType.None;
        public string Texture { get; set; } = string.Empty;
        public float Emission { get; set; } = 0.305f;
        public Color ColorA { get; set; } = Color.white;
        public Color ColorB { get; set; } = Color.white;
        public Color ColorC { get; set; } = Color.white;
        public float ColorADistance { get; set; } = 0.0f;
        public float ColorBDistance { get; set; } = 0.0f;
        public float ColorCDistance { get; set; } = 0.0f;
        public ColorFadeData FadeColorA { get; set; } = new ();
        public ColorFadeData FadeColorB { get; set; } = new ();
        public ColorFadeData FadeColorC { get; set; } = new ();
        public BlinkData Blink { get; set; } = new();
        public StrobeData EmissionStrobe { get; set; } = new ();

        public Vector4 VectorA => new (ColorA.r, ColorA.g, ColorA.b, ColorADistance);
        public Vector4 VectorB => new (ColorB.r, ColorB.g, ColorB.b, ColorBDistance);
        public Vector4 VectorC => new (ColorC.r, ColorC.g, ColorC.b, ColorCDistance);
    }

    public sealed class BlinkData
    {
        public bool InEnabled { get; set; } = false;
        public bool OutEnabled { get; set; } = false;

        internal void TryBlinkOutOrDisable(GameObject obj)
        {
            if (OutEnabled)
            {
                CoroutineManager.BlinkOut(obj, 0.0f);
            }
            else
            {
                obj.SetActive(false);
            }
        }

        internal void TryBlinkInOrEnable(GameObject obj)
        {
            if (InEnabled)
            {
                CoroutineManager.BlinkIn(obj, 0.0f);
            }
            else
            {
                obj.SetActive(true);
            }
        }
    }

    public sealed class StrobeData
    {
        public bool Enabled { get; set; } = false;
        public float Duration { get; set; } = 1.0f;
        public float MinMulti { get; set; } = 0.5f;
        public float MaxMulti { get; set; } = 1.0f;
    }

    public sealed class ColorFadeData
    {
        public bool Enabled { get; set; } = false;
        public float Duration { get; set; } = 1.0f;
        public Color[] Colors { get; set; } = Array.Empty<Color>();

        internal Color _previousColor = Color.white;
        internal int _currentIndex = 0;
        internal float _timer = 0.0f;

        internal void NextIndex()
        {
            _currentIndex++;

            if (_currentIndex >= Colors.Length)
            {
                _currentIndex = 0;
            }
        }
    }

    public enum DoorStateType
    {
        None,
        Opening,
        Unlocked,
        Unlocked_MotionTrigger,
        Locked_Keycard,
        Locked_Generator,
        Locked_Scan,
        Locked_ScanProgress,
        Locked_Alarm,
        Locked_AlarmProgress,
        Locked_ErrorAlarm,
        Locked_ErrorAlarmProgress,
        Lockdowned,
    }
}
