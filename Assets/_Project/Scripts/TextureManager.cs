using UnityEngine;
using System.Collections.Generic;[CreateAssetMenu(fileName = "TextureManager", menuName = "Game/Texture Manager")]
public class TextureManager : ScriptableObject
{
    [System.Serializable]
    public class BlockTextures
    {
        public int colorId;
        public Texture paralel1Block;    // Cube01BlueParalelTextureMap
        public Texture up1Block;         // Cube01BlueUpTextureMap
        public Texture paralel2Block;    // Cube02BlueParalelTextureMap
        public Texture up2Block;         // Cube02BlueUpTextureMap
    }

    public List<BlockTextures> colorTextures = new List<BlockTextures>();

    public Texture GetTexture(int colorId, int length, bool isParallel)
    {
        BlockTextures textures = colorTextures.Find(t => t.colorId == colorId);
        if (textures == null) return null;

        if (length == 1)
            return isParallel ? textures.paralel1Block : textures.up1Block;
        else
            return isParallel ? textures.paralel2Block : textures.up2Block;
    }
}