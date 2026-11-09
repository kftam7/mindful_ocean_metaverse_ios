using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TutorialStep
{
    public int Step { get; set; }
    public string Prefab { get; set; }
    public string Type { get; set; }

    public string[] Target { get; set; }
    public string[] TargetButton { get; set; }

    public string[] PointDirection
    {
        set
        {
            if (value != null && value.Length > 0)
            {
                Direction = new IndicatorDirection[value.Length];

                for (int i = 0; i < value.Length; i++)
                {
                    switch (value[i])
                    {
                        case "Bottom":
                            Direction[i] = IndicatorDirection.Bottom;
                            break;
                        case "Left":
                            Direction[i] = IndicatorDirection.Left;
                            break;
                        case "Right":
                            Direction[i] = IndicatorDirection.Right;
                            break;
                        case "Top":
                        default:
                            Direction[i] = IndicatorDirection.Top;
                            break;
                    }
                }
            }
        }
    }

    public bool[] BlockRaycast { get; set; }

    public IndicatorDirection[] Direction { private set; get; }
    public bool[] ActionValue { get; set; }
    public string[] PointerPrefab { get; set; }

    public string TriggerCondition { get; set; }
    public string TriggeredByParameter { get; set; }
    public string TriggerOnEnd { get; set; }
    public float DelayStart { get; set; }
    public string Description { get; set; } //Show to player
    public bool SetAlignment { get; set; }
    public float TextAlignment { get; set; }
    public int FontSize { get; set; }
    public bool IsBestFit { get; set; }
    public float BGXOffset { get; set; }
    public float BGYOffset { get; set; }
    public string ActionDescription { get; set; } //For log
    public bool IsLogProgress { get; set; }
    public float BlockTime { get; set; }
    public bool EnableTextBG { get; set; }

    public byte[] TextColor
    {
        set
        {
            try
            {
                ColorForText = new Color32(value[0], value[1], value[2], value[3]);
            }
            catch
            {
                ColorForText = Color.white;
            }
        }
    }

    public Color32 ColorForText { get; set; }
}