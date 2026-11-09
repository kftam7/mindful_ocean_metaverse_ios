using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using ProGamer;

public class DataCenter : ProGamer.Singleton<DataCenter>
{
    public QuestionnaireData mPlayerQuestionnaireData;

    public QuestionnaireData PlayerQuestionnaireData
    {
        get
        {
            if (mPlayerQuestionnaireData == null)
            {
                if (string.IsNullOrEmpty(GameSave.QuestionnaireResult))
                {
                    mPlayerQuestionnaireData = new QuestionnaireData(Role.Student, "not_available", "not_available", "not_available", "not_available");
                }
                else
                {
                    mPlayerQuestionnaireData = JsonConvert.DeserializeObject<QuestionnaireData>(GameSave.QuestionnaireResult);
                }
            }

            return mPlayerQuestionnaireData;
        }
        set { mPlayerQuestionnaireData = value; }
    }

    #region V2

    public PanelShowStats UpdateVersionPanelShowStats { get; set; }
    public bool EventInfoShown { get; set; } // only automatically show popup once on game start
    public int CurrentEventId { get; set; } = -1;
    public bool TutorialMainFlowShown { get; set; } // only show once in game life.
    public ImageData CurrentArtwork { get; set; }
    public string JoinedVia { get; set; }

    #endregion

    #region Game Config

    private Dictionary<string, CharacterData> mCharacterDataDic;

    public Dictionary<string, CharacterData> CharacterDataDic
    {
        get
        {
            if (mCharacterDataDic == null)
            {
                mCharacterDataDic = JsonConvert.DeserializeObject<Dictionary<string, CharacterData>>(GameSetting.CharacterConfiguration);
            }

            return mCharacterDataDic;
        }
    }

    private Dictionary<string, ImageTargetData> mImageTargetDataDic;

    public Dictionary<string, ImageTargetData> ImageTargetDataDic
    {
        get
        {
            if (mImageTargetDataDic == null)
            {
                mImageTargetDataDic = JsonConvert.DeserializeObject<Dictionary<string, ImageTargetData>>(GameSetting.ImageTargetConfiguration);
            }

            return mImageTargetDataDic;
        }
    }

    private Dictionary<string, PackageData> mIAPDataDic;

    public Dictionary<string, PackageData> IAPDataDic
    {
        get
        {
            if (mIAPDataDic == null)
            {
                mIAPDataDic = JsonConvert.DeserializeObject<Dictionary<string, PackageData>>(GameSetting.IAPConfiguration);
            }

            return mIAPDataDic;
        }
    }

    private Dictionary<string, List<PrintingMaterialData>> mPrintingMaterialDic;

    public Dictionary<string, List<PrintingMaterialData>> PrintingMaterialDic
    {
        get
        {
            if (mPrintingMaterialDic == null)
            {
                var dic = JsonConvert.DeserializeObject<Dictionary<string, PrintingMaterialData>>(GameSetting.MaterialConfiguration);
                mPrintingMaterialDic = new Dictionary<string, List<PrintingMaterialData>>();

                foreach (var data in dic.Values)
                {
                    if (!mPrintingMaterialDic.ContainsKey(data.IncludedInPackage))
                    {
                        mPrintingMaterialDic[data.IncludedInPackage] = new List<PrintingMaterialData>();
                    }

                    mPrintingMaterialDic[data.IncludedInPackage].Add(data);
                }

                foreach (var list in mPrintingMaterialDic.Values)
                {
                    list.Sort(delegate(PrintingMaterialData x, PrintingMaterialData y)
                    {
                        return x.Id.CompareTo(y.Id);
                    });
                }
            }

            return mPrintingMaterialDic;
        }
    }

    public List<PrintingMaterialData> GetPackageMaterialData(PackageData package)
    {
        List<PrintingMaterialData> result;

        if (package.CombinationPackage != null && package.CombinationPackage.Count > 0)
        {
            result = new List<PrintingMaterialData>();

            for (int i = 0; i < package.CombinationPackage.Count; i++)
            {
                List<PrintingMaterialData> tem;

                if (PrintingMaterialDic.TryGetValue(package.CombinationPackage[i], out tem))
                {
                    result.AddRange(tem);
                }
            }

            return result;
        }
        else
        {
            if (PrintingMaterialDic.TryGetValue(package.Id, out result))
            {
                return result;
            }
            else
            {
                Debug.Log("Cannot find " + package.Id);
                return null;
            }
        }
    }

    public Dictionary<string, bool> mPurchaseStatus;

    public Dictionary<string, bool> PurchaseStatus
    {
        get
        {
            if (mPurchaseStatus == null)
            {
                if (!string.IsNullOrEmpty(GameSave.PurchaseRecord))
                {
                    try
                    {
                        mPurchaseStatus = JsonConvert.DeserializeObject<Dictionary<string, bool>>(GameSave.PurchaseRecord);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.Log(ex.Message);
                        Debug.Log("Game save purchase record corrupt");
                        mPurchaseStatus = new Dictionary<string, bool>();
                    }
                }
                else
                {
                    mPurchaseStatus = new Dictionary<string, bool>();
                }
            }

            return mPurchaseStatus;
        }
    }

    public string UnlockPackage(string packageId)
    {
        foreach (var data in DataCenter.Instance.IAPDataDic.Values)
        {
            //Search package id
            if (data.PackageId == packageId)
            {
                Debug.Log("Unlock package with id: " + data.Id);
                PurchaseStatus[data.Id] = true;

                GameSave.PurchaseRecord = JsonConvert.SerializeObject(PurchaseStatus);

                return data.Id;
            }
        }

        //No matched package id in Dic
        Debug.LogError("Cannot found matched configuration.");
        return "";
    }


    public bool GetPackageStatus(string packageId)
    {
        PackageData data;

        if (IAPDataDic.TryGetValue(packageId, out data))
        {
            if (data.Price == "Free")
            {
                return true;
            }
            else
            {
                bool purchase;

                //Check if bought packages set
                if (!string.IsNullOrEmpty(data.IncludedBy) && data.IncludedBy != "na")
                {
                    if (PurchaseStatus.TryGetValue(data.IncludedBy, out purchase) && purchase)
                    {
                        return true;
                    }
                }

                //check if alreay own all in package set
                if (data.CombinationPackage != null && data.CombinationPackage.Count > 0)
                {
                    bool allBought = true;

                    foreach (var id in data.CombinationPackage)
                    {
                        allBought = allBought && PurchaseStatus.TryGetValue(id, out purchase) && purchase;
                    }

                    if (allBought) return true;
                }

                //check individually
                if (PurchaseStatus.TryGetValue(packageId, out purchase))
                {
                    return purchase;
                }
                else
                {
                    return false;
                }
            }
        }
        else
        {
            return false;
        }
    }


    /// <summary>
    /// return:
    /// 0 - no band purchased
    /// 1 - band 5 purchased
    /// 2 - band 10 purchased
    /// 3 - band 20 purchased
    /// </summary>
    /// <returns></returns>
    public int GetBandPurchaseStatus()
    {
        bool purchased05 = GetPackageStatus("packageBand01");
        bool purchased10 = GetPackageStatus("packageBand02");
        bool purchased20 = GetPackageStatus("packageBand03");
        bool purchased105 = GetPackageStatus("packageBand04");
        bool purchased110 = GetPackageStatus("packageBand05");
        bool purchased115 = GetPackageStatus("packageBand06");

        if (purchased20) return 3;
        if (purchased10 && purchased110) return 3;
        if (purchased05 && purchased115) return 3;
        if (purchased05 && purchased105 && purchased110) return 3;
        if (purchased10) return 2;
        if (purchased05 && purchased105) return 2;
        if (purchased05) return 1;
        return 0;
    }

    public bool IsAccessoryPurchased()
    {
        return Instance.GetPackageStatus("packageBand07");
    }

    private Dictionary<string, CollectionData> mCollectionDataDic;

    public Dictionary<string, CollectionData> CollectionDataDic
    {
        get
        {
            if (mCollectionDataDic == null)
            {
                mCollectionDataDic = JsonConvert.DeserializeObject<Dictionary<string, CollectionData>>(GameSetting.CollectionConfiguration);
            }

            return mCollectionDataDic;
        }
    }

    private Dictionary<string, InGameObjectData> mInGameObjectDataDic;

    public Dictionary<string, InGameObjectData> InGameObjectDataDic
    {
        get
        {
            if (mInGameObjectDataDic == null)
            {
                mInGameObjectDataDic = JsonConvert.DeserializeObject<Dictionary<string, InGameObjectData>>(GameSetting.InGameObjectConfiguration);
            }

            return mInGameObjectDataDic;
        }
    }


    private List<QuestionData> mQuestionList;

    public List<QuestionData> QuestionList
    {
        get
        {
            if (mQuestionList == null)
            {
                mQuestionList = new List<QuestionData>(JsonConvert.DeserializeObject<Dictionary<string, QuestionData>>(GameSetting.QuestionConfiguration).Values);
            }

            return mQuestionList;
        }
    }

    private Dictionary<string, EbookData> mEbookDataDic;

    public Dictionary<string, EbookData> EbookDataDic
    {
        get
        {
            if (mEbookDataDic == null)
            {
                mEbookDataDic = JsonConvert.DeserializeObject<Dictionary<string, EbookData>>(GameSetting.EbookConfiguration);
            }

            return mEbookDataDic;
        }
    }

    private Dictionary<string, List<TaskData>> mTaskDataDic;

    public Dictionary<string, List<TaskData>> TaskDataDic
    {
        get
        {
            if (mTaskDataDic == null)
            {
                Dictionary<string, TaskData> tem = JsonConvert.DeserializeObject<Dictionary<string, TaskData>>(GameSetting.TaskConfiguration);
                mTaskDataDic = new Dictionary<string, List<TaskData>>();

                foreach (TaskData d in tem.Values)
                {
                    if (!mTaskDataDic.ContainsKey(d.Character))
                    {
                        mTaskDataDic[d.Character] = new List<TaskData>();
                    }

                    mTaskDataDic[d.Character].Add(d);
                }

                foreach (List<TaskData> l in mTaskDataDic.Values)
                {
                    l.Sort(delegate(TaskData x, TaskData y)
                    {
                        return x.TaskIndex.CompareTo(y.TaskIndex);
                    });
                }
            }

            return mTaskDataDic;
        }
    }

    #endregion Game Config

    #region Aquarium

    protected Dictionary<string, AquariumData> mAquariumsData;

    /// <summary>
    /// Id -> Data
    /// </summary>
    public Dictionary<string, AquariumData> AquariumsData
    {
        get
        {
            if (mAquariumsData == null)
            {
                mAquariumsData = GameSave.AquariumSave.ReadSave<Dictionary<string, AquariumData>>();
            }
            /*if(mAquariumSave.Count == 0){
                //for (int i=0;i<5;i++){
                    string id = ImageDataWriter.GetGUID();
                    mAquariumSave.Add(id, new AquariumData(id, "txt_default", CollectionDataDic[GameSetting.DefaultAquariumTheme], GameSave.AquariumIndex));
                    GameSave.AquariumIndex = GameSave.AquariumIndex +1;
                    this.SaveAquarium();
                //}

            }*/

            return mAquariumsData;
        }
    }

    public AquariumData GetSelectedAquarium()
    {
        Debug.Log($"[{GetType().Name}] GetSelectedAquarium  :" + GameSave.SelectedAquarium);
        return GetAquarium(GameSave.SelectedAquarium);
    }

    public string GetAquariumIdByName(string name)
    {
        foreach (var pair in AquariumsData)
        {
            if (pair.Value.AquariumName == name)
            {
                return pair.Key;
            }
        }

        return null;
    }

    public AquariumData GetAquarium(string id)
    {
        AquariumData data;

        if (AquariumsData.TryGetValue(id, out data))
        {
            return data;
        }
        else
        {
            foreach (AquariumData d in AquariumsData.Values)
            {
                return d;
            }

            return null;
        }
    }

    public void AddAquarium(AquariumData data)
    {
        AquariumsData.Add(data.AquariumId, data);
        SaveAquarium();
    }

    public void SaveAquarium()
    {
        GameSave.AquariumSave = JsonConvert.SerializeObject(AquariumsData);
    }

    #endregion

    #region Purchase

    protected Dictionary<string, bool> mAvailabilityOfModel;

    public Dictionary<string, bool> AvailabilityOfModel
    {
        get
        {
            if (mAvailabilityOfModel == null)
            {
                if (!string.IsNullOrEmpty(GameSave.AvailabilityOfModel))
                {
                    mAvailabilityOfModel = JsonConvert.DeserializeObject<Dictionary<string, bool>>(GameSave.AvailabilityOfModel);
                }
                else
                {
                    mAvailabilityOfModel = new Dictionary<string, bool>();
                }
            }

            return mAvailabilityOfModel;
        }
    }

    public bool GetAvailabilityOfModel(string model)
    {
#if EVENT_DEMO || UNLOCK_ALL
		return true;
#endif

        bool isAvailable = false;

        if (AvailabilityOfModel.TryGetValue(model, out isAvailable))
        {
            return isAvailable;
        }

        CharacterData data;

        if (CharacterDataDic.TryGetValue(model, out data))
        {
            return data.Price == 0;
        }

        return isAvailable;
    }


    public void SetAvailabilityOfModel(string model, bool value)
    {
        AvailabilityOfModel[model] = value;
        GameSave.AvailabilityOfModel = JsonConvert.SerializeObject(AvailabilityOfModel);
    }

    #endregion

    protected LeaderboardElement newestPlayerResult;

    public LeaderboardElement NewestPlayerResult
    {
        get { return newestPlayerResult; }
    }

    protected List<LeaderboardElement> leaderboardList;

    public List<LeaderboardElement> LeaderboardList
    {
        get
        {
            if (leaderboardList == null)
            {
                if (!string.IsNullOrEmpty(GameSave.LeaderboardList))
                {
                    leaderboardList = JsonConvert.DeserializeObject<List<LeaderboardElement>>(GameSave.LeaderboardList);

                    leaderboardList.Sort(delegate(LeaderboardElement x, LeaderboardElement y)
                    {
                        return y.Score.CompareTo(x.Score);
                    });
                }
                else
                {
                    leaderboardList = new List<LeaderboardElement>();
                }
            }

            return leaderboardList;
        }
    }

    public void AddElementToLeaderboard(LeaderboardElement element)
    {
        newestPlayerResult = element;

        LeaderboardList.Add(element);
        LeaderboardList.Sort(delegate(LeaderboardElement x, LeaderboardElement y)
        {
            return y.Score.CompareTo(x.Score);
        });

        GameSave.LeaderboardList = JsonConvert.SerializeObject(LeaderboardList);
    }

    protected Dictionary<string, CharacterUpgrade> characterUpgradeDic;

    public Dictionary<string, CharacterUpgrade> CharacterUpgradeDic
    {
        get
        {
            if (characterUpgradeDic == null)
            {
                if (!string.IsNullOrEmpty(GameSave.CharacterUpgrade))
                {
                    characterUpgradeDic = JsonConvert.DeserializeObject<Dictionary<string, CharacterUpgrade>>(GameSave.CharacterUpgrade);
                }
                else
                {
                    characterUpgradeDic = new Dictionary<string, CharacterUpgrade>();
                }
            }

            return characterUpgradeDic;
        }
    }

    public void UpgradeHP(string modelName)
    {
        CharacterUpgrade data = GetCharacterUpgradeData(modelName);
        int cost = data.GetHPUpgradeCost();
        if (GameSave.Coin >= cost)
        {
            GameSave.Coin -= cost;
            data.HPLevelUpgrade();
            GameSave.CharacterUpgrade = JsonConvert.SerializeObject(CharacterUpgradeDic);
            EventManager.TriggerEvent(EventList.OnCoinUpdated, GameSave.Coin);
        }
        else
        {
            PopupManager.Instance.ShowMessagePopup("txt_notice", "txt_not_enough_money");
        }
    }

    public void UpgradeBonus(string modelName)
    {
        CharacterUpgrade data = GetCharacterUpgradeData(modelName);
        int cost = data.GetBonusUpgradeCost();
        if (GameSave.Coin >= cost)
        {
            GameSave.Coin -= cost;
            data.BonusLevelUpgrade();
            GameSave.CharacterUpgrade = JsonConvert.SerializeObject(CharacterUpgradeDic);
            EventManager.TriggerEvent(EventList.OnCoinUpdated, GameSave.Coin);
        }
        else
        {
            PopupManager.Instance.ShowMessagePopup("txt_notice", "txt_not_enough_money");
        }
    }

    public CharacterUpgrade GetCharacterUpgradeData(string modelName)
    {
        CharacterUpgrade data;
        if (CharacterUpgradeDic.TryGetValue(modelName, out data))
        {
            return data;
        }
        else
        {
            data = new CharacterUpgrade(modelName);
            CharacterUpgradeDic[modelName] = data;

            return data;
        }
    }

    protected Dictionary<string, int> mEbookUnlockedStatus;

    public Dictionary<string, int> EbookUnlockedStatus
    {
        get
        {
            if (mEbookUnlockedStatus == null)
            {
                if (!string.IsNullOrEmpty(GameSave.EbookUnlockStatus))
                {
                    mEbookUnlockedStatus = JsonConvert.DeserializeObject<Dictionary<string, int>>(GameSave.EbookUnlockStatus);
                }
                else
                {
                    mEbookUnlockedStatus = new Dictionary<string, int>();
                }
            }

            return mEbookUnlockedStatus;
        }
    }

    public int GetEbookUnlockedIndex(string modelName)
    {
#if EVENT_DEMO || UNLOCK_ALL
		return 4;
#endif

        if (modelName == "Plastic") return 4;

        if (EbookUnlockedStatus.ContainsKey(modelName))
        {
            return EbookUnlockedStatus[modelName];
        }

        return -1;
    }

    public void SetEbookUnlockedIndex(string modelName, int index)
    {
#if EVENT_DEMO || UNLOCK_ALL
		return;
#endif

        EbookUnlockedStatus[modelName] = index;
        SetIsNewPage(modelName, index);
        GameSave.EbookUnlockStatus = JsonConvert.SerializeObject(EbookUnlockedStatus);
    }


    protected Dictionary<string, List<int>> mNewEbookPages;

    public Dictionary<string, List<int>> NewEbookPages
    {
        get
        {
            if (mNewEbookPages == null)
            {
                if (!string.IsNullOrEmpty(GameSave.NewEbookPages))
                {
                    mNewEbookPages = JsonConvert.DeserializeObject<Dictionary<string, List<int>>>(GameSave.NewEbookPages);
                }
                else
                {
                    mNewEbookPages = new Dictionary<string, List<int>>();
                }
            }

            return mNewEbookPages;
        }
    }

    public bool GetIsNewEbookContent()
    {
#if EVENT_DEMO || UNLOCK_ALL
		return false;
#endif
        return NewEbookPages.Count > 0;
    }

    public bool GetIsNewTab(string model)
    {
#if EVENT_DEMO || UNLOCK_ALL
		return false;
#endif
        return NewEbookPages.ContainsKey(model);
    }

    public bool GetIsNewPage(string model, int index)
    {
#if EVENT_DEMO || UNLOCK_ALL
		return false;
#endif
        return NewEbookPages.ContainsKey(model) && NewEbookPages[model].Contains(index);
    }

    public void SetIsNewPage(string model, int index)
    {
#if EVENT_DEMO || UNLOCK_ALL
		return;
#endif
        if (!NewEbookPages.ContainsKey(model))
        {
            NewEbookPages[model] = new List<int>();
        }

        if (!NewEbookPages[model].Contains(index))
        {
            NewEbookPages[model].Add(index);

            GameSave.NewEbookPages = JsonConvert.SerializeObject(NewEbookPages);
        }
    }

    public bool SetOpenedPage(string model, int index)
    {
#if EVENT_DEMO || UNLOCK_ALL
		return false;
#endif
        if (NewEbookPages.ContainsKey(model) && NewEbookPages[model].Contains(index))
        {
            NewEbookPages[model].Remove(index);
            if (NewEbookPages[model].Count == 0)
            {
                NewEbookPages.Remove(model);
            }

            GameSave.NewEbookPages = JsonConvert.SerializeObject(NewEbookPages);
            return true;
        }

        return false;
    }
}

public enum PanelShowStats
{
    NotShown, IsShowing, IsNotShowing, Shown
}