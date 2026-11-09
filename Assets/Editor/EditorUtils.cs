#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System;
using UnityEditor.SceneManagement;

namespace ProGamer
{
    public class EditorUtils
    {
        #region delete PlayerPrefs

        [MenuItem("Tools/Delete PlayerPrefs", false, 46)]
        public static void DeletePlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }

        [MenuItem("Tools/Date Data/Decrease Saved Date", false, 46)]
        public static void DecreaseSavedDate()
        {
            var dateValue = DateTime.Today.AddDays(-1).ToString();
            PlayerPrefs.SetString("LAST_SESSION_DATE_TIME", dateValue);
            Debug.Log(dateValue);
        }

        [MenuItem("Tools/Date Data/Increase Saved Date", false, 46)]
        public static void IncreaseSavedData()
        {
            var dateValue = DateTime.Today.AddDays(1).ToString();
            PlayerPrefs.SetString("LAST_SESSION_DATE_TIME", dateValue);
            Debug.Log(dateValue);
        }

        [MenuItem("Tools/Date Data/Print Saved Date", false, 47)]
        public static void PrintDateData()
        {
            var result = PlayerPrefs.GetString("LAST_SESSION_DATE_TIME");
            Debug.Log(result);
        }

        [MenuItem("Tools/Date Data/Reduce Bottles", false, 46)]
        public static void ReduceBottleCollected()
        {
            var value = PlayerPrefs.GetInt("BOTTLES_COLLECTED_TODAY");
            PlayerPrefs.SetInt("BOTTLES_COLLECTED_TODAY", value - 1);
            Debug.Log($"Bottles Amount is now: {value - 1}");
        }

        [MenuItem("Tools/Date Data/Add Bottles", false, 46)]
        public static void AddBottleCollected()
        {
            var value = PlayerPrefs.GetInt("BOTTLES_COLLECTED_TODAY");
            PlayerPrefs.SetInt("BOTTLES_COLLECTED_TODAY", value + 1);
            Debug.Log($"Bottles Amount is now: {value + 1}");
        }

        [MenuItem("Tools/Date Data/Reset Bottles", false, 46)]
        public static void ResetBottleCollected()
        {
            PlayerPrefs.SetInt("BOTTLES_COLLECTED_TODAY", 0);
            var value = PlayerPrefs.GetInt("BOTTLES_COLLECTED_TODAY");
            Debug.Log($"Bottles Amount is now: {value}");
        }

        [MenuItem("Tools/Date Data/Reduce Single Player Bottles", false, 46)]
        public static void ReduceSingleBottleCollected()
        {
            var value = PlayerPrefs.GetInt("SINGLE_PLAYER_BOTTLES_COLLECTED_TODAY");
            PlayerPrefs.SetInt("SINGLE_PLAYER_BOTTLES_COLLECTED_TODAY", value - 1);
            Debug.Log($"Bottles Amount is now: {value - 1}");
        }

        [MenuItem("Tools/Date Data/Add Single Player Bottles", false, 46)]
        public static void AddSingleBottleCollected()
        {
            var value = PlayerPrefs.GetInt("SINGLE_PLAYER_BOTTLES_COLLECTED_TODAY");
            PlayerPrefs.SetInt("SINGLE_PLAYER_BOTTLES_COLLECTED_TODAY", value + 1);
            Debug.Log($"Bottles Amount is now: {value + 1}");
        }

        [MenuItem("Tools/Date Data/Reset Single Player Bottles", false, 46)]
        public static void ResetSingleBottleCollected()
        {
            PlayerPrefs.SetInt("SINGLE_PLAYER_BOTTLES_COLLECTED_TODAY", 0);
            var value = PlayerPrefs.GetInt("SINGLE_PLAYER_BOTTLES_COLLECTED_TODAY");
            Debug.Log($"Bottles Amount is now: {value}");
        }

        [MenuItem("Tools/Date Data/Set Single Player Limit to True", false, 46)]
        public static void EnableDayLimit()
        {
            PlayerPrefs.SetInt("SINGLE_PLAYER_LIMIT", 1);
        }

        [MenuItem("Tools/Date Data/Set Single Player Limit to False", false, 46)]
        public static void DisableDayLimit()
        {
            PlayerPrefs.SetInt("SINGLE_PLAYER_LIMIT", 0);
        }

        [MenuItem("Tools/Date Data/Set Multi Player Limit to True", false, 46)]
        public static void EnableMultiLimit()
        {
            PlayerPrefs.SetInt("MULTI_PLAYER_LIMIT", 1);
        }

        [MenuItem("Tools/Date Data/Set Multi Player Limit to False", false, 46)]
        public static void DisableMultiLimit()
        {
            PlayerPrefs.SetInt("MULTI_PLAYER_LIMIT", 0);
        }

        [MenuItem("Tools/Date Data/Reset Match Today", false, 46)]
        public static void ResetMatchToday()
        {
            GameSave.MultiPlayerModeMatchCount = 0;
        }

        [MenuItem("Tools/Date Data/Print Data Values", false, 47)]
        public static void PrintLimitData()
        {
            var singlePlayerLimit = PlayerPrefs.GetInt("SINGLE_PLAYER_LIMIT");
            var multiPlayerLimit = PlayerPrefs.GetInt("MULTI_PLAYER_LIMIT");
            var singleBottles = GameSave.SinglePlayerBottlesCollectedToday;
            var matchToday = GameSave.MultiPlayerModeMatchCount;

            Debug.Log($"Limit: {singlePlayerLimit == 1}, Multi-Limit: {multiPlayerLimit == 1}, Single Bottles: {singleBottles}, Match today: {matchToday}");
        }

        #endregion


        public static string LoadText(string path)
        {
            TextAsset t = (TextAsset)AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));

            if (t != null)
            {
                return t.text;
            }
            else
            {
                Debug.LogError("Cannot load file: " + path);
                return "";
            }
        }
    }
}
#endif