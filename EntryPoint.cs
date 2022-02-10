﻿using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace SecurityDoorHologramOverhaul
{
    [BepInPlugin("SecurityDoorHologramOverhaul", "SecurityDoorHologramOverhaul", "1.0.0")]
    public class EntryPoint : BasePlugin
    {
        public override void Load()
        {
            ClassInjector.RegisterTypeInIl2Cpp<DoorHologramUpdater>();

            _harmonyInstance = new Harmony("SecurityDoorHologramOverhaul");
            _harmonyInstance.PatchAll();

            DoorHologramManager.ReadConfig();
            AssetShards.AssetShardManager.add_OnStartupAssetsLoaded((Action)AssetLoaded);
        }

        private void AssetLoaded()
        {
            var texturesPath = Path.Combine(Paths.ConfigPath, "Assets", "Textures", "SecDoorHolograms");

            if (!Directory.Exists(texturesPath))
                Directory.CreateDirectory(texturesPath);

            var textures = Directory.GetFiles(texturesPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(x =>
                    x.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase) ||
                    x.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) ||
                    x.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

            foreach (var textureFile in textures)
            {
                if (!File.Exists(textureFile))
                    continue;

                var bytes = File.ReadAllBytes(textureFile);
                var newTexture = new Texture2D(1, 1);
                if (!ImageConversion.LoadImage(newTexture, bytes))
                {
                    GameObject.Destroy(newTexture);
                    continue;
                }

                newTexture.name = Path.GetFileNameWithoutExtension(textureFile);
                newTexture.hideFlags = HideFlags.HideAndDontSave;
                Textures.Add(newTexture.name, newTexture);
            }
        }

        private Harmony _harmonyInstance;
    }
}