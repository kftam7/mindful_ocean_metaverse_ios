using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureReference : MonoBehaviour
{
    [SerializeField] protected Texture[] textureList;

    public Texture[] TextureList
    {
        get { return textureList; }
    }

    protected Dictionary<string, Texture> textureDic;

    public Texture GetTextureByName(string name)
    {
        if (textureDic == null)
        {
            textureDic = new Dictionary<string, Texture>();
            foreach (Texture t in textureList)
            {
                textureDic[t.name] = t;
            }
        }

        Texture texture;
        if (textureDic.TryGetValue(name, out texture))
        {
            return texture;
        }

        return null;
    }
}