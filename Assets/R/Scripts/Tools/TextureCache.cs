using System.Collections.Generic;
using UnityEngine;

public class TextureCache : Network.Singleton<TextureCache>
{
    private Dictionary<string, Texture2D> cacheDic = new(); //filename, texture

    public void Init()
    {
        DontDestroyOnLoad(gameObject);
        cacheDic = new Dictionary<string, Texture2D>();
    }

    public void ClearCache()
    {
        cacheDic = new Dictionary<string, Texture2D>();
        Resources.UnloadUnusedAssets();
    }

    public void RemoveCache(string fullFileName)
    {
        if (cacheDic.ContainsKey(fullFileName))
        {
            DestroyImmediate(cacheDic[fullFileName]);
            cacheDic.Remove(fullFileName);
        }
    }

    public void Cache(string fullFileName, Texture2D texture)
    {
        cacheDic[fullFileName] = texture;
    }

    public Texture2D Load(string fullFileName, bool small)
    {
        try
        {
            string realFullFileName = small
                ? ImageFileNameUtils.GetSmallImageFullFileName(fullFileName)
                : fullFileName;

            if (cacheDic.TryGetValue(realFullFileName, out var texture))
            {
                return texture;
            }
            else
            {
                texture = ImageDataWriter.LoadImage(fullFileName, small);
                if (texture.width < 1024) // only cache small texture to save memory
                {
                    Cache(realFullFileName, texture);
                }

                return texture;
            }
        }
        catch
        {
            Debug.Log($"!!![{GetType().Name}] Error while find file: {fullFileName}");
            return new Texture2D(512, 512);
        }
    }
}