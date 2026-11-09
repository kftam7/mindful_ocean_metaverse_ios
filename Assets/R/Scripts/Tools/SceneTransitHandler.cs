using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SceneTransitHandler : MonoBehaviour
{
    public const string GameStartScene = "InitScene";

    public static string NextScene = "";
    public static string PreviousScene = "";

    public static string currentScene;

    public static string CurrentScene
    {
        get
        {
            if (string.IsNullOrEmpty(currentScene))
            {
                return GameStartScene;
            }

            return currentScene;
        }
        set { currentScene = value; }
    }

    //Start from init

    [SerializeField] protected Image progressBar;

    public static void LoadScene(string targetScene, bool viaLoadingScene = false)
    {
        if (EventSystem.current != null && EventSystem.current.gameObject != null)
        {
            EventSystem.current.gameObject.SetActive(false);
        }

        PreviousScene = CurrentScene;

        if (viaLoadingScene)
        {
            NextScene = targetScene;
            SceneManager.LoadScene("LoadingScene");
        }
        else
        {
            CurrentScene = targetScene;
            SceneManager.LoadScene(targetScene);
        }
    }

    /// <summary>
    /// Only record scene transit history
    /// </summary>
    /// <param name="targetScene">Target scene.</param>
    public static void SceneTransitManually(string targetScene)
    {
        NextScene = targetScene;
        PreviousScene = CurrentScene;
    }

    IEnumerator Start()
    {
        Resources.UnloadUnusedAssets(); //release resources

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(NextScene);

        CurrentScene = NextScene;
        NextScene = "";

        while (!asyncLoad.isDone)
        {
            // progressBar.fillAmount = asyncLoad.progress;
            yield return null;
        }
    }

    #region Default function

    public static void BackMainMenu()
    {
        LoadScene("MainMenu");
    }

    #endregion
}