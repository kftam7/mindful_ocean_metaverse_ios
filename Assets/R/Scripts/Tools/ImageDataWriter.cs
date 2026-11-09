using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class ImageDataWriter
{
    private const string FolderName = "Image";
    private static Dictionary<int, Dictionary<int, Texture2D>> cachedTextureDic = new();

    public static string GetSaveFilePath(string fullFileName)
    {
        string path = Path.Combine(Application.persistentDataPath, FolderName);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return Path.Combine(path, fullFileName);
    }

    public static string SaveImageByte(byte[] data, string fullFileName)
    {
        string filePath = GetSaveFilePath(fullFileName) + ".jpg";
        File.WriteAllBytes(filePath, data);
        return filePath;
    }

    public static void SaveImgToGallery(string filePath)
    {
        ImageToAlbum.Instance.SaveImage(filePath);
    }

    public static void SaveImage(Texture2D texture, string fullFileName, bool generateSmall = true)
    {
        void SaveImageData(Texture2D texture2D, string fullFileName)
        {
            string ext = Path.GetExtension(fullFileName).ToLower();
            byte[] bytes = ext == "jpg" ? texture2D.EncodeToJPG() : texture2D.EncodeToPNG();
            File.WriteAllBytes(GetSaveFilePath(fullFileName), bytes);
        }

        SaveImageData(texture, fullFileName);
        TextureCache.Instance.RemoveCache(fullFileName);
        if (!generateSmall) return;
        Texture2D smallTexture;
        if (texture.width > 1024)
        {
            smallTexture = Scaled(texture, 1024, 1024);
            Scale(smallTexture, 512, 512);
        }
        else
        {
            smallTexture = Scaled(texture, 512, 512);
        }

        string smallFileName = ImageFileNameUtils.GetSmallImageFullFileName(fullFileName);
        SaveImageData(smallTexture, smallFileName);
        TextureCache.Instance.RemoveCache(smallFileName);
    }

    public static byte[] GetImageRawData(string fullFileName)
    {
        string path = GetSaveFilePath(fullFileName);

        if (File.Exists(path))
        {
            return File.ReadAllBytes(path);
        }

        Debug.Log($"!!![ImageDataWriter] GetImageRawData: Cannot find {fullFileName} in {path}: ");
        return Array.Empty<byte>();
    }

    public static string GetImagePath(string fullFileName, bool small)
    {
        return GetSaveFilePath(small
            ? ImageFileNameUtils.GetSmallImageFullFileName(fullFileName)
            : fullFileName);
    }

    public static Texture2D LoadImage(string fullFileName, bool small)
    {
        string path = GetSaveFilePath(fullFileName);
        string realPath = GetImagePath(fullFileName, small);

        if (File.Exists(realPath))
        {
            Texture2D texture = new Texture2D(1, 1);
            if (texture.LoadImage(File.ReadAllBytes(realPath)))
            {
                return texture;
            }
            else
            {
                Debug.Log("!!![ImageDataWriter] LoadImage: Cannot convert to Texture.");
                Debug.Log(realPath);
                return null;
            }
        }

        if (realPath != path && File.Exists(path))
        {
            Texture2D texture = new Texture2D(1, 1);
            if (texture.LoadImage(File.ReadAllBytes(path)))
            {
                SaveImage(texture, fullFileName);
                if (texture.LoadImage(File.ReadAllBytes(realPath)))
                {
                    return texture;
                }
                else
                {
                    Debug.Log("!!![ImageDataWriter] LoadImage: Cannot convert to Texture.");
                    Debug.Log(realPath);
                    return null;
                }
            }
            else
            {
                Debug.Log("!!![ImageDataWriter] LoadImage: Cannot convert to Texture.");
                Debug.Log(path);
                return null;
            }
        }

        Debug.Log($"!!![ImageDataWriter] LoadImage: Cannot find {realPath}: ");
        return null;
    }

    public static bool CheckImageExists(string fullFileName, bool small)
    {
        return File.Exists(GetImagePath(fullFileName, small));
    }

    public static Sprite LoadSprite(string fullFileName)
    {
        Texture2D image = LoadImage(fullFileName, false);
        return image ? Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(0.5f, 0.5f)) : null;
    }

    public static void RemoveImage(string fullFileName)
    {
        string path = GetSaveFilePath(fullFileName);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("[ImageDataWriter] RemoveImage: " + path);
        }
        else
        {
            Debug.Log("[ImageDataWriter] RemoveImage: File doesn't exist: " + path);
        }
    }

    /// <summary>
    /// Returns a scaled copy of given texture.
    /// </summary>
    /// <param name="srcTexture">Source texture to scale</param>
    /// <param name="width">Destination texture width</param>
    /// <param name="height">Destination texture height</param>
    /// <param name="mode">Filtering mode</param>
    public static Texture2D Scaled(Texture2D srcTexture, int width, int height, FilterMode mode = FilterMode.Trilinear)
    {
        Rect texR = new(0, 0, width, height);
        GpuScale(srcTexture, width, height, mode);

        //Get rendered data back to a new texture
        Texture2D result = GetCachedTexture(width, height);
        result.Reinitialize(width, height);
        result.ReadPixels(texR, 0, 0, true);
        return result;
    }

    /// <summary>
    /// Scales the texture data of the given texture.
    /// </summary>
    /// <param name="texture">Texure to scale</param>
    /// <param name="width">New width</param>
    /// <param name="height">New height</param>
    /// <param name="mode">Filtering mode</param>
    public static void Scale(Texture2D texture, int width, int height, FilterMode mode = FilterMode.Trilinear)
    {
        Rect texR = new(0, 0, width, height);
        GpuScale(texture, width, height, mode);

        // Update new texture
        texture.Reinitialize(width, height);
        texture.ReadPixels(texR, 0, 0, true);
        texture.Apply(true); //Remove this if you hate us applying textures for you :)
    }

    // Internal unility that renders the source texture into the RTT - the scaling method itself.
    private static void GpuScale(Texture2D src, int width, int height, FilterMode fmode)
    {
        //We need the source texture in VRAM because we render with it
        src.filterMode = fmode;
        src.Apply(true);

        //Using RTT for best quality and performance. Thanks, Unity 5
        RenderTexture rtt = RenderTexture.GetTemporary(width, height, 32);

        //Set the RTT in order to render to it
        Graphics.SetRenderTarget(rtt);

        //Setup 2D matrix in range 0..1, so nobody needs to care about sized
        GL.LoadPixelMatrix(0, 1, 1, 0);

        //Then clear & draw the texture to fill the entire RTT.
        GL.Clear(true, true, new Color(0, 0, 0, 0));
        Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
        RenderTexture.ReleaseTemporary(rtt);
    }

    private static Texture2D GetCachedTexture(int width, int height)
    {
        if (!cachedTextureDic.TryGetValue(width, out var subDic))
        {
            subDic = new Dictionary<int, Texture2D>();
            cachedTextureDic.Add(width, subDic);
        }

        if (!subDic.TryGetValue(height, out var texture))
        {
            texture = new Texture2D(width, height, TextureFormat.ARGB32, true);
            subDic.Add(height, texture);
        }

        return texture;
    }
}