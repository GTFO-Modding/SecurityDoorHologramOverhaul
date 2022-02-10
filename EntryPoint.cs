using BepInEx;
using BepInEx.IL2CPP;
using GTFO.API;
using HarmonyLib;
using SecurityDoorHologramOverhaul.Utils;
using System;
using System.IO;
using System.Linq;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace SecurityDoorHologramOverhaul
{
    [BepInPlugin("SecurityDoorHologramOverhaul", "SecurityDoorHologramOverhaul", "1.0.0")]
    [BepInProcess("GTFO.exe")]
    [BepInDependency(MTFOUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(MTFOPartialDataUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class EntryPoint : BasePlugin
    {
        public override void Load()
        {
            ClassInjector.RegisterTypeInIl2Cpp<DoorHologramUpdater>();

            _harmonyInstance = new Harmony("SecurityDoorHologramOverhaul");
            _harmonyInstance.PatchAll();

            DoorHologramManager.ReadConfig();
            AssetAPI.OnStartupAssetsLoaded += AssetLoaded;
        }

        private void AssetLoaded()
        {
            var texturesPath = Path.Combine(Paths.ConfigPath, "Assets", "Textures", "SecDoorHolograms");
            var textures = GetTextureFiles(texturesPath);

            foreach (var textureFile in textures)
            {
                TryAddTextureFile(textureFile, isRundownFile: false);  
            }

            if (MTFOUtil.IsLoaded && MTFOUtil.HasCustomContent)
            {
                var customTexturesPath = Path.Combine(MTFOUtil.CustomPath, "Textures", "SecDoorHolograms");
                var customTextures = GetTextureFiles(customTexturesPath);
                foreach (var textureFile in customTextures)
                {
                    TryAddTextureFile(textureFile, isRundownFile: true);
                }
            }
        }

        private string[] GetTextureFiles(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var textures = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                .Where(x =>
                    x.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase) ||
                    x.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) ||
                    x.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

            return textures;
        }

        private void TryAddTextureFile(string path, bool isRundownFile = false)
        {
            if (!File.Exists(path))
                return;

            var bytes = File.ReadAllBytes(path);
            var newTexture = new Texture2D(1, 1);
            if (!ImageConversion.LoadImage(newTexture, bytes))
            {
                GameObject.Destroy(newTexture);
                return;
            }

            var newName = Path.GetFileNameWithoutExtension(path);
            if (isRundownFile)
            {
                newName = $"Custom/{newName}";
            }
            newTexture.name = newName;
            
            newTexture.hideFlags = HideFlags.HideAndDontSave;
            Textures.Add(newName, newTexture);
        }

        private Harmony _harmonyInstance;
    }
}
