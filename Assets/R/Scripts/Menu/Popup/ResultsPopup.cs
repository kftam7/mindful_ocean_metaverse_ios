using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProGamer;
using UnityEngine.UI;
using System.IO;

public class ResultsPopup : SimplePopup
{
    [SerializeField] protected Text score;
    [SerializeField] protected Text coinReward;
    [SerializeField] protected GameObject taskContainer;
    [SerializeField] protected Text taskDescription;
    [SerializeField] protected Text taskProgressText;
    [SerializeField] protected Image taskProgressBar;
    [SerializeField] protected GameObject isTaskFinishTick;

    [SerializeField] protected Text totalCoin;

    protected override void InitPopup()
    {
        base.InitPopup();

        taskContainer.SetActive(InGameData.Instance.Task != null);
        if (InGameData.Instance.Task != null)
        {
            SetTaskData(InGameData.Instance.Task);
        }

        score.text = InGameData.Instance.Score.ToString();
        int gainCoin = GameBalanceCalculation.GetCoinByScoreAndTime(InGameData.Instance.Score, InGameData.Instance.WholeTime, InGameData.Instance.CoinBonus);
        InGameData.Instance.GainCoin(gainCoin);

        coinReward.text = gainCoin.ToString();
        totalCoin.text = GameSave.Coin.ToString();

        string skin = GameSave.GetModelSkin(GameSave.SelectedModel);

        if (!string.IsNullOrEmpty(skin) && Application.internetReachability != NetworkReachability.NotReachable)
        {
            string object_id = Path.GetFileNameWithoutExtension(skin);
            GameSave.SearchCode = object_id;

            Debug.Log("Upload data with id " + object_id);

            Dictionary<string, string> result = new Dictionary<string, string>()
            {
                { "object_id", object_id },
                { "object_title", GameSave.SelectedModel },
                { "object_desc", DataCenter.Instance.PlayerQuestionnaireData.Wishes },
                { "user_name", DataCenter.Instance.PlayerQuestionnaireData.PlayerName },
                { "score", InGameData.Instance.Score.ToString() },
                { "object_model", GameSave.SelectedModel },
            };

            Texture2D img = ImageDataWriter.LoadImage(skin, false);
            if (img != null)
            {
                UploadResult(result, object_id + ".jpg", img);
            }
        }
    }

    protected void SetTaskData(InGameTaskData data)
    {
        taskDescription.text = data.TaskStaticData.TargetDescription;
        taskProgressText.text = string.Format("{0}/{1}", data.CurrentProgress, data.TaskStaticData.TargetAmount);
        taskProgressBar.fillAmount = (float)data.CurrentProgress / data.TaskStaticData.TargetAmount;
        isTaskFinishTick.SetActive(data.IsFinished);
        //Unlock ebook
        if (data.IsFinished)
        {
            GameSave.IsNewInfoUnlocked = true;
            DataCenter.Instance.SetEbookUnlockedIndex(data.TaskStaticData.Character, data.TaskStaticData.Reward);
        }
    }

    public void BackToMainMenu()
    {
#if EVENT_DEMO
		SceneTransitHandler.LoadScene("ThankScene");
#else
        SceneTransitHandler.BackMainMenu();
#endif
    }

    public void UploadResult(Dictionary<string, string> metadata, string fileName, Texture2D img)
    {
#if !UNITY_EDITOR
		// var new_metadata = new Firebase.Storage.MetadataChange(){
		//
		// 	CustomMetadata = metadata,
		// 	CacheControl = "public, max-age=31536000"
		// };

		//Uploaded skin
		// PluginWrapper.Instance.UploadFile(fileName, img.EncodeToJPG(), new_metadata, PluginWrapper.SkinsStoragePath);

		//Uploaded thumbnail
		Texture2D thumbnail = ImageDataWriter.Scaled(img, 128, 128);
		// PluginWrapper.Instance.UploadFile(fileName, thumbnail.EncodeToJPG(), new_metadata, PluginWrapper.ThumbnailPath);
#endif
    }
}