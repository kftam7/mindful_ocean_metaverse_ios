using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

//Save data
public class PackageCellData
{
    public enum CellType
    {
        PackageCell,
        PackageFrontFaceCell,
        ColoringPageCell
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public Sprite sprite { get; set; }
    public string text { get; set; }
    public int Index { get; set; }
    public bool purchased { get; set; }
    public CellType cellType { get; set; }
    public PrintingMaterialData materialData { get; set; }

    public PackageCellData(string id, string name, Sprite sprite, string text, int index, CellType cellType)
    {
        this.Id = id;
        this.Name = name;
        this.sprite = sprite;
        this.text = text;
        this.cellType = cellType;
        this.Index = index;
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}