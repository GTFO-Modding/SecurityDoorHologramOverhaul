using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SecurityDoorHologramOverhaul
{
    public static class Textures
    {
        public static void Add(string name, Texture2D texture)
        {
            _textures.Add(name.ToLowerInvariant(), texture);
        }

        public static bool TryGet(string name, out Texture2D texture)
        {
            return _textures.TryGetValue(name.ToLowerInvariant(), out texture);
        }

        private static readonly Dictionary<string, Texture2D> _textures = new();
    }
}
