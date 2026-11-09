using System.IO;
using UnityEngine;

/// <summary>
/// Template image: tpl_outline_{modelName}_{suffixes}.png
/// User image: usr_{modelName}_{suffixes}_{id}.jpg
/// Aquarium background: hatchery_aquarium.jpg / metaverse_aquarium.jpg (defined in GameSettings.cs)
/// </summary>
public static class ImageFileNameUtils
{
    /// <summary>
    /// http://127.0.0.1:2021/download?file=tpl_outline_ArcherFish_Cartoon.png"
    /// or
    /// http://127.0.0.1:2021/download/tpl_outline_ArcherFish_Cartoon.png"
    /// </summary>
    public static string GetFullFileNameFromUrl(string url)
    {
        string fileName = Path.GetFileName(url);
        if (fileName.Contains("="))
        {
            fileName = fileName.Split("=")[1];
        }

        return fileName;
    }

    public static string GetSmallImageFullFileName(string fullFileName)
    {
        string fileExt = Path.GetExtension(fullFileName);
        string fileName = GetFileName(fullFileName);
        return $"{fileName}_small{fileExt}";
    }

    public static bool IsValidArtworkFileName(string fullFileName)
    {
        return fullFileName.StartsWith("usr_");
    }

    public static string GetFileName(string fullFileName)
    {
        return Path.GetFileNameWithoutExtension(fullFileName);
    }

    public static string GetFileId(string fullFileName)
    {
        var elements = GetFileName(fullFileName).Split("_");
        if (elements.Length < 3) // hatchery_aquarium.jpg
        {
            return elements[0];
        }

        return elements[3];
    }

    public static string GetModelName(string fullFileName)
    {
        var elements = GetFileName(fullFileName).Split("_");
        if (elements.Length < 3) // hatchery_aquarium.jpg
        {
            return null;
        }

        return elements[1];
    }
}