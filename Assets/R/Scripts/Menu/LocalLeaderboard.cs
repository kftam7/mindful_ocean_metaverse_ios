using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalLeaderboard : MonoBehaviour
{
    public LeaderboardCell LeaderboardCellObject;
    public Transform ScrollviewContent;

    void Start()
    {
        int playerIndex = -1;

        if (DataCenter.Instance.NewestPlayerResult != null)
        {
            playerIndex = DataCenter.Instance.LeaderboardList.IndexOf(DataCenter.Instance.NewestPlayerResult);
        }

        for (int i = 0; i < DataCenter.Instance.LeaderboardList.Count; i++)
        {
            LeaderboardCell go = GameObject.Instantiate(LeaderboardCellObject, ScrollviewContent) as LeaderboardCell;
            go.SetUI(DataCenter.Instance.LeaderboardList[i], i, i == playerIndex);
        }
    }
}