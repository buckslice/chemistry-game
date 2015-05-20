using UnityEngine;
using UnityEditor;

public class LevelTextureProcessor : AssetPostprocessor {

    // auto imports level textures correctly
	void OnPreprocessTexture() {
        if (assetPath.Contains("level")){
            TextureImporter ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Advanced;
            ti.npotScale = TextureImporterNPOTScale.None;
            ti.isReadable = true;
            ti.mipmapEnabled = false;
            ti.filterMode = FilterMode.Point;
        }
    }
}
