using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class SpriteReference : MonoBehaviour
{
    [SerializeField] protected List<Sprite> spriteList = new List<Sprite>();

    protected Dictionary<string, int> spriteDictionary;

    public List<Sprite> SpriteList
    {
        get { return spriteList; }
    }

    private Dictionary<string, int> SpriteDictionary
    {
        get
        {
            if (spriteDictionary == null) GenerateSpriteDictionary();
            return spriteDictionary;
        }
    }

    public Sprite GetSpriteByName(string name)
    {
        if (SpriteDictionary.ContainsKey(name))
        {
            return spriteList[spriteDictionary[name]];
        }

        Debug.LogError("Sprite " + name + " not found in SpriteReference " + this.name);
        return null;
    }

    public void GenerateSpriteDictionary()
    {
        if (spriteList != null)
        {
            spriteDictionary = new Dictionary<string, int>();
            for (int i = 0; i < spriteList.Count; i++)
            {
                if (spriteList[i] != null)
                {
                    spriteDictionary[spriteList[i].name] = i;
                }
                else
                {
                    Debug.Log("SpriteDictionary have null reference");
                }
            }
        }
    }

    public void AddSprite(Sprite s)
    {
        spriteList.Add(s);
    }

    public void SortList()
    {
        spriteList.Sort((x, y) =>
        {
            return string.Compare(x.name, y.name);
        });
    }

    //For Editor
    public void ClearEmptyArrayEntry(bool sort = false)
    {
        if (spriteList != null)
        {
            List<Sprite> tem = new List<Sprite>();

            for (int i = 0; i < spriteList.Count; i++)
            {
                if (spriteList[i] != null && !tem.Contains(spriteList[i]))
                {
                    tem.Add(spriteList[i]);
                }
            }

            if (sort)
            {
                AlphanumComparatorFast comparer = new AlphanumComparatorFast();

                tem.Sort((Sprite a, Sprite b) =>
                {
                    return comparer.Compare(a.name, b.name);
                });
            }

            spriteList = tem;
        }
    }

    public string GetSpriteList()
    {
        if (spriteList == null || spriteList.Count <= 0) return "";
        List<string> nameList = new List<string>();
        foreach (Sprite s in spriteList)
        {
            if (s != null)
            {
                nameList.Add(s.name);
            }
        }

        return JsonConvert.SerializeObject(nameList);
    }
}