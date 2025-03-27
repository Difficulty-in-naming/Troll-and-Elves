using UnityEngine;

namespace ScratchCardAsset.Tools
{
    public static class TextureExtensions
    {
        public static bool IsCrunched(this Texture2D texture)
        {
            if (!texture) 
                return false;
        
            switch (texture.format)
            {
                case TextureFormat.DXT1Crunched:
                case TextureFormat.DXT5Crunched:
                case TextureFormat.ETC_RGB4Crunched:
                case TextureFormat.ETC2_RGBA8Crunched:
                    return true;
                default:
                    return false;
            }
        }
    }
}