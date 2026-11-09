using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetDataForEvent : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
#if EVENT_DEMO
		string leaderboard = GameSave.LeaderboardList;
		string questionResult = GameSave.CachedQuestionnaireResultList;

		PlayerPrefs.DeleteAll();

		GameSave.LeaderboardList = leaderboard;
		GameSave.CachedQuestionnaireResultList = questionResult;
#endif
    }
}