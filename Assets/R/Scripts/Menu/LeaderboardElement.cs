using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardElement
{
    public string PlayerName { get; set; }
    public int Score { get; set; }
    public string ModelName { get; set; }

    public LeaderboardElement(string player, int score, string model)
    {
        this.PlayerName = player;
        this.Score = score;
        this.ModelName = model;
    }
}