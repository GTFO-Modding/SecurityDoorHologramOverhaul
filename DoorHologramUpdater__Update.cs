using Il2CppInterop.Runtime.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SecurityDoorHologramOverhaul
{
    public sealed partial class DoorHologramUpdater : MonoBehaviour
    {
        internal void Update()
        {
            if (!HasStateSetting)
                return;

            UpdateEmissionStrobe();
            UpdateColor(0);
            UpdateColor(1);
            UpdateColor(2);
        }

        [HideFromIl2Cpp]
        private void UpdateEmissionStrobe()
        {
            var strobe = CurrentData.EmissionStrobe;

            if (!strobe.Enabled)
                return;

            if (strobe.Duration <= 0.0f)
                return;

            var progress = Mathf.PingPong(Clock.ExpeditionProgressionTime / strobe.Duration, 1.0f);
            progress = Easing.GetEasingValue(eEasingType.EaseOutSine, progress, backwards: false);

            var newEmission = CurrentData.Emission * Mathf.Lerp(strobe.MinMulti, strobe.MaxMulti, progress);
            CurrentMaterial.SetVector(_sEmissive, new Vector4(newEmission, newEmission, newEmission, 1.0f));
        }

        [HideFromIl2Cpp]
        private void UpdateColor(int layerID)
        {
            int propertyID;
            float distance;
            ColorFadeData colorFade;

            switch (layerID)
            {
                case 0:
                    propertyID = _sLayerA;
                    distance = CurrentData.ColorADistance;
                    colorFade = CurrentData.FadeColorA;
                    break;

                case 1:
                    propertyID = _sLayerB;
                    distance = CurrentData.ColorBDistance;
                    colorFade = CurrentData.FadeColorB;
                    break;

                case 2:
                    propertyID = _sLayerC;
                    distance = CurrentData.ColorCDistance;
                    colorFade = CurrentData.FadeColorC;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(layerID));
            }

            if (!colorFade.Enabled)
                return;

            if (colorFade.Duration <= 0.0f)
                return;

            colorFade._timer += Clock.Delta;
            if (colorFade._timer < colorFade.Duration)
            {
                var progress = Mathf.InverseLerp(0.0f, colorFade.Duration, colorFade._timer);
                progress = Mathf.PingPong(progress, 0.5f);
                progress = Easing.GetEasingValue(eEasingType.EaseOutSine, progress, backwards: false);

                var newColor = Color.Lerp(colorFade._previousColor, colorFade.Colors[colorFade._currentIndex], progress);
                CurrentMaterial.SetVector(propertyID, new Vector4(newColor.r, newColor.g, newColor.b, distance));
            }
            else //Time Done
            {
                colorFade._timer = 0.0f;
                colorFade._previousColor = colorFade.Colors[colorFade._currentIndex];
                colorFade.NextIndex();
            }
        }
    }
}
