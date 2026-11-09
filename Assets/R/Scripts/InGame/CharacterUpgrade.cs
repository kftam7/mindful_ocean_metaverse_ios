using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterUpgrade
{
    public string ModelName { get; set; }
    public int HPLevel { get; set; }
    public int BonusLevel { get; set; }

    public CharacterUpgrade(string modelName)
    {
        this.ModelName = modelName;
        this.HPLevel = 1;
        this.BonusLevel = 1;
    }

    public int GetHPUpgradeCost()
    {
        return HPLevel * 100;
    }

    public int GetBonusUpgradeCost()
    {
        return BonusLevel * 150;
    }

    public float GetBonus()
    {
        return GetBonusAtLevel(BonusLevel);
    }

    public float GetBonusAtLevel(int level)
    {
        return Mathf.Atan(Mathf.Pow(0.1f * (level - 1), 2)) * 10f;
    }

    public int GetHP()
    {
        return GetHPAtLevel(HPLevel);
    }

    public int GetHPAtLevel(int level)
    {
        return GameSetting.HPOriginLimit + Mathf.RoundToInt((level - 1) * 1.5f);
    }

    public void HPLevelUpgrade()
    {
        HPLevel++;
    }

    public void BonusLevelUpgrade()
    {
        BonusLevel++;
    }
}