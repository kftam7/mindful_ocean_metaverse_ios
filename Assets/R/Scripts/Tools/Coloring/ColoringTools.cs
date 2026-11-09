using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public static class ColoringTools
{
    public const float Error = 0.05f;

    public static List<Vector2Int> FindSimilarColorContinuousRegion(Texture2D texture, Vector2Int startPoint)
    {
        return texture.Search(startPoint, new bool[texture.width, texture.height]);
    }

    /// <summary>
    /// Search the specified texture, startCoordinate and ignoreSearch.
    /// </summary>
    /// <returns>The search.</returns>
    /// <param name="texture">Texture.</param>
    /// <param name="startCoordinate">Start search coordinate.</param>
    /// <param name="ignoreSearch">Ignore search these pos.</param>
    public static List<Vector2Int> Search(this Texture2D texture, Vector2Int startCoordinate, bool[,] ignoreSearch)
    {
        Color origin = texture.GetPixel(startCoordinate);

        bool[,] markedSearched = ignoreSearch;

        List<Vector2Int> result = new List<Vector2Int>();
        List<Vector2Int> searchList = new List<Vector2Int>();

        result.Add(startCoordinate);
        searchList.Add(startCoordinate);

        Vector2Int bound = new Vector2Int(texture.width, texture.height);

        for (int i = 0; i < searchList.Count; i++)
        {
            //get target
            Vector2Int target = searchList[i];

            //find around
            List<Vector2Int> around = new List<Vector2Int>()
            {
                target + Vector2Int.up,
                target + Vector2Int.down,
                target + Vector2Int.left,
                target + Vector2Int.right
            };

            //search around
            foreach (var pos in around)
            {
                //check bounded
                if (pos.IsBounded(bound) && !markedSearched[pos.x, pos.y])
                {
                    if (origin.IsSimilarColor(texture.GetPixel(pos)))
                    {
                        result.Add(pos);
                        searchList.Add(pos);
                    }

                    markedSearched[pos.x, pos.y] = true;
                }
            }
        }

        Debug.Log("Searched pixels " + searchList.Count);
        return result;
    }

    public static bool IsBounded(this Vector2Int origin, Vector2Int bound)
    {
        return origin.x >= 0 && origin.y >= 0 && origin.x < bound.x && origin.y < bound.y;
    }

    public static Color GetPixel(this Texture2D texture, Vector2Int pos)
    {
        return texture.GetPixel(pos.x, pos.y);
    }

    public static void SetPixel(this Texture2D texture, Vector2Int pos, Color color)
    {
        texture.SetPixel(pos.x, pos.y, color);
    }

    public static bool IsSimilarColor(this Color a, Color b)
    {
        if (a == b)
        {
            return true;
        }

        Color dif = b - a;

        if (Mathf.Abs(dif.r) < Error && Mathf.Abs(dif.g) < Error && Mathf.Abs(dif.b) < Error && Mathf.Abs(dif.a) < Error)
        {
            return true;
        }

        return false;
    }


    public static Sprite GetColoredSprite(Texture2D outline)
    {
        return Sprite.Create(GetColoredTexture(outline), new Rect(0, 0, outline.width, outline.height), Vector2.one * 0.5f);
    }

    public static Texture2D GetColoredTexture(Texture2D outline, float alpha = 0.1f)
    {
        Texture2D texture = new Texture2D(outline.width, outline.height);

        Color[] colors = ColoringTools.AutoFillContinuousAlphaRegion(outline, alpha);

        texture.SetPixels(colors);

        texture.Apply();

        return texture;
    }

    public static Color[] AutoFillContinuousAlphaRegion(this Texture2D outline, float alpha)
    {
        //Start from left-bottom
        Color[] origin = outline.GetPixels();
        Vector2Int bound = new Vector2Int(outline.width, outline.height);

        return AutoFillContinuousAlphaRegion(origin, bound, alpha);
    }

    public static Color[] AutoFillContinuousAlphaRegion(Color[] origin, Vector2Int bound, float alpha)
    {
        Color[] colors = new Color[origin.Length]; //for store new color

        int regionCount = 0;

        for (int y = 0; y < bound.y; y++)
        {
            for (int x = 0; x < bound.x; x++)
            {
                int pos = x + y * bound.y;
                //start search
                if (colors[pos].a < alpha)
                {
                    //check if assigned color
                    if (origin[pos].a >= alpha)
                    {
                        //it is outline
                        colors[pos] = origin[pos];
                        continue;
                    }
                    else
                    {
                        //search continuous region and assign
                        regionCount++;
                        Color regionColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

                        colors[pos] = regionColor;

                        List<int> searchList = new List<int>();
                        searchList.Add(pos);

                        for (int i = 0; i < searchList.Count; i++)
                        {
                            int start = searchList[i];

                            List<int> around = new List<int>();
                            //search right
                            if (start % bound.y < bound.x - 1)
                            {
                                around.Add(start + 1);
                            }

                            //search left
                            if (start % bound.y >= 1)
                            {
                                around.Add(start - 1);
                            }

                            //search top
                            if (start / bound.y < bound.y - 1)
                            {
                                around.Add(start + bound.y);
                            }

                            //search bottom
                            if (start / bound.y >= 1)
                            {
                                around.Add(start - bound.y);
                            }


                            foreach (int p in around)
                            {
                                //not yet fill color
                                if (colors[p].a < alpha)
                                {
                                    //outline
                                    if (origin[p].a >= alpha)
                                    {
                                        colors[p] = origin[p];
                                    }
                                    else
                                    {
                                        colors[p] = regionColor;
                                        searchList.Add(p);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //after search
        Debug.Log("Found region " + regionCount);
        return colors;
    }
}