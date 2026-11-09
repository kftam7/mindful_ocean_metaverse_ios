using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameBalanceCalculation
{
    public static int GetCoinByScoreAndTime(int score, int time, float bonus)
    {
        return Mathf.RoundToInt((time + score) * (bonus + 1));
    }

    public static int GetScoreByFood(string foodName, int time)
    {
        return Mathf.RoundToInt(GameSetting.BaseFoodBonus * GameSetting.FoodBonus[Mathf.Clamp(time / 60, 0, GameSetting.FoodBonus.Length - 1)]);
    }
}