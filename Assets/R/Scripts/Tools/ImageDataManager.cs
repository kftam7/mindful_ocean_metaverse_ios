using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class ImageDataManager : Network.Singleton<ImageDataManager>
{
    private const string SaveKey = "ARTWORK";

    public ImageData NewImageData; // new artwork that is in editing and might be added into aquarium. 
    public List<ImageData> ImageDataList { get; private set; }
    public List<ImageData> DefaultImageDataList { get; } = new();
    public List<ImageData> ActiveImageDataList { get; } = new();
    public List<ImageData> ArchivedImageDataList { get; } = new();
    public List<ImageData> NftImageDataList { get; } = new();
    public List<ImageData> InHatcheryImageDataList { get; } = new();
    public List<ImageData> InMetaverseImageDataList { get; } = new();
    public ImageData HatcheryAquariumSnapshotData { get; private set; }
    public ImageData MetaverseAquariumSnapshotData { get; private set; }

    public event Action ImageDataUpdatedEvent;
    public event Action InAquariumImageDataUpdatedEvent;

    private bool useDebugFishes;

    #region Public Methods

    public void Init()
    {
        DontDestroyOnLoad(gameObject);
        LoadDataFromDisk();
        FillImageDataLists(false);
        CheckFishNumberInAquariumsToLimit(); // just to be sure
        ServerCenter.Instance.SignedInEvent += OnSignedIn;
    }

    public void UpdateServerImageData(Action<bool> callback)
    {
        Debug.Log($"[{GetType().Name}] UpdateServerImageData:");
        ServerCenter.Instance.UpdateNftsInfo(
            _ =>
            {
                foreach (NftItemResultData resultData in ServerCenter.Instance.NftDataList)
                {
                    if (!ImageFileNameUtils.IsValidArtworkFileName(resultData.fileName))
                    {
                        Debug.Log($"[{GetType().Name}] invalid remote image file: {resultData.fileName}");
                        continue;
                    }

                    ImageData imageData = FindImageData(resultData.fileName);
                    if (imageData == null)
                    {
                        imageData = new ImageData
                        {
                            ImageFile = resultData.fileName,
                            Url = resultData.imageURL,
                            Submitted = true,
                            IsNft = true,
                            ConfirmedByServer = true,
                            Unlocked = true,
                            InAquarium = false
                        };
                        AddImageData(imageData, false, false);
                        AddImageDataIntoAquarium(imageData, false, false);
                    }
                    else
                    {
                        imageData.Url = resultData.imageURL;
                        imageData.Submitted = true;
                        imageData.IsNft = true;

                        if (!imageData.ConfirmedByServer)
                        {
                            imageData.Unlocked = true;
                            imageData.ConfirmedByServer = true;
                            AddImageDataIntoAquarium(imageData, false, false);
                        }
                    }
                }

                SaveDataToDisk();
                FillImageDataLists(true);
                callback?.Invoke(true);
            },
            () => callback?.Invoke(false));
    }

    public List<ImageData> GetInAquariumImageDataList(string aquariumId)
    {
        return aquariumId == GameSetting.HatcheryAquariumId
            ? InHatcheryImageDataList
            : InMetaverseImageDataList;
    }

    public ImageData GetAquariumSnapshotData(string aquariumId)
    {
        return aquariumId == GameSetting.HatcheryAquariumId
            ? HatcheryAquariumSnapshotData
            : MetaverseAquariumSnapshotData;
    }

    public string GetAquariumSnapshotFilePath(string aquariumId)
    {
        return aquariumId == GameSetting.HatcheryAquariumId
            ? GameSetting.HatcheryAquariumSnapshotFileName
            : GameSetting.MetaverseAquariumSnapshotFileName;
    }

    public void AddImageData(ImageData imageData, bool save, bool sendEvent)
    {
        foreach (ImageData data in ImageDataList)
        {
            if (data == imageData)
            {
                Debug.Log($"!!![{GetType().Name}] Image already exists: {imageData.ImageFile}");
                return;
            }
        }

        ImageDataList.Add(imageData);

        if (imageData.InAquarium)
        {
            // remove older in-aquarium fish correctly.
            AddImageDataIntoAquarium(imageData, save, sendEvent);
        }

        if (save) SaveDataToDisk();
        FillImageDataLists(sendEvent);
    }

    public void RemoveImageData(ImageData imageData, bool removeFile, bool save, bool sendEvent)
    {
        if (removeFile)
        {
            string smallFileName = ImageFileNameUtils.GetSmallImageFullFileName(imageData.ImageFile);
            ImageDataWriter.RemoveImage(imageData.ImageFile);
            ImageDataWriter.RemoveImage(smallFileName);
            TextureCache.Instance.RemoveCache(imageData.ImageFile);
            TextureCache.Instance.RemoveCache(smallFileName);
        }

        for (var i = ImageDataList.Count - 1; i >= 0; i--)
        {
            if (ImageDataList[i] == imageData)
            {
                ImageDataList.Remove(ImageDataList[i]);
            }
        }

        FillImageDataLists(sendEvent);
        if (save) SaveDataToDisk();
    }

    public void AddNewImageDataIntoAquarium(bool save, bool sendEvent)
    {
        AddImageDataIntoAquarium(NewImageData, save, sendEvent);
    }

    public void AddImageDataIntoAquarium(ImageData imageData, bool save, bool sendEvent)
    {
        // In order to achieve FIFO (variant), it is necessary to calculate
        // whether it can be added here. If it is not an NFT, it will be
        // filtered out when loading the Metaverse.
        bool isMetaverse = imageData.Submitted; // see above.
        // Debug.Log($"111111 - {isMetaverse}");
        string aquariumId = isMetaverse ? GameSetting.MetaverseAquariumId : GameSetting.HatcheryAquariumId;
        // Debug.Log($"111111 - {aquariumId}");
        var inAquariumImageDataList = GetInAquariumImageDataList(aquariumId);
        if (inAquariumImageDataList.Contains(imageData)) return;
        // Debug.Log($"111111 - do adding");

        if (inAquariumImageDataList.Count >= GameSetting.MaxFishNumberInAquarium)
        {
            inAquariumImageDataList.Sort(CompareCaptureTime);
            var dataToBeRemoved = inAquariumImageDataList[inAquariumImageDataList.Count - 1];
            dataToBeRemoved.InAquarium = false;
            inAquariumImageDataList.Remove(dataToBeRemoved);
        }

        imageData.InAquarium = true;
        inAquariumImageDataList.Add(imageData);
        if (save) SaveDataToDisk();
        if (sendEvent) InAquariumImageDataUpdatedEvent?.Invoke();
    }

    public void RemoveImageDataFromAquarium(ImageData imageData, bool save, bool sendEvent)
    {
        bool isMetaverse = imageData.Submitted;
        string aquariumId = isMetaverse ? GameSetting.MetaverseAquariumId : GameSetting.HatcheryAquariumId;
        var inAquariumImageDataList = GetInAquariumImageDataList(aquariumId);
        if (inAquariumImageDataList.Contains(imageData))
        {
            inAquariumImageDataList.Remove(imageData);
        }

        imageData.InAquarium = false;
        if (save) SaveDataToDisk();
        if (sendEvent) InAquariumImageDataUpdatedEvent?.Invoke();
    }

    public void FillImageDataLists(bool sendEvent)
    {
        DefaultImageDataList.Clear();
        ActiveImageDataList.Clear();
        ArchivedImageDataList.Clear();
        NftImageDataList.Clear();
        InHatcheryImageDataList.Clear();
        InMetaverseImageDataList.Clear();
        foreach (ImageData data in ImageDataList)
        {
            bool isMetaverse = ServerCenter.Instance.UseArtworkInMetaverse ? data.Submitted : data.IsNft;
            bool isHatchery = !data.Submitted;

            // Hatchery background pic
            if (data.ImageFile == GameSetting.HatcheryAquariumSnapshotFileName)
                HatcheryAquariumSnapshotData = data;
            // Metaverse background pic
            else if (data.ImageFile == GameSetting.MetaverseAquariumSnapshotFileName)
                MetaverseAquariumSnapshotData = data;
            else if (data.IsDefault)
            {
                DefaultImageDataList.Add(data);
                if (data.InAquarium) InMetaverseImageDataList.Add(data);
            }
            // Fish in metaverse list
            else if (isMetaverse)
            {
                NftImageDataList.Add(data);
                // Fish models in metaverse 
                if (data.InAquarium && data.Unlocked) InMetaverseImageDataList.Add(data);
            }
            // Fish in hatchery list
            else if (isHatchery)
            {
                EventStatus status = data.GetStatus();
                if (status == EventStatus.Active || status == EventStatus.Coming)
                {
                    ActiveImageDataList.Add(data);
                }
                else
                {
                    ArchivedImageDataList.Add(data);
                }

                // Fish models in hatchery 
                if (data.InAquarium) InHatcheryImageDataList.Add(data);
            }
        }

        if (useDebugFishes) CreateTestArchivedFishes();
        ActiveImageDataList.Sort(CompareCaptureTime);
        ArchivedImageDataList.Sort(CompareCaptureTime);
        NftImageDataList.Sort(CompareCaptureTime); // temporary order
        InHatcheryImageDataList.Sort(CompareCaptureTime);
        InMetaverseImageDataList.Sort(CompareCaptureTime);
        if (sendEvent) ImageDataUpdatedEvent?.Invoke();
    }

    public void SaveDataToDisk()
    {
        string jsonString = JsonConvert.SerializeObject(ImageDataList);
        if (!string.IsNullOrEmpty(jsonString))
        {
            PlayerPrefs.SetString(SaveKey, jsonString);
        }
        else
        {
            Debug.Log($"[{GetType().Name}] SaveDataToDisk: ImageDataList is null!!!!!");
        }
    }

    public void UseDebugFishes(bool value)
    {
        useDebugFishes = value;
        FillImageDataLists(true);
    }

    #endregion

    #region Private Methods

    private int CompareCaptureTime(ImageData data1, ImageData data2)
    {
        if (data1.CaptureTime < data2.CaptureTime)
            return 1;
        if (data1.CaptureTime == data2.CaptureTime)
            return 0;
        return -1;
    }

    private void CheckFishNumberInAquariumsToLimit()
    {
        while (InHatcheryImageDataList.Count > GameSetting.MaxFishNumberInAquarium)
        {
            InHatcheryImageDataList[InHatcheryImageDataList.Count - 1].InAquarium = false;
            InHatcheryImageDataList.RemoveAt(InHatcheryImageDataList.Count - 1);
        }

        while (InMetaverseImageDataList.Count > GameSetting.MaxFishNumberInAquarium)
        {
            InMetaverseImageDataList[InMetaverseImageDataList.Count - 1].InAquarium = false;
            InMetaverseImageDataList.RemoveAt(InMetaverseImageDataList.Count - 1);
        }
    }

    private void CreateTestArchivedFishes()
    {
        for (int i = ArchivedImageDataList.Count - 1; i >= 0; i--)
        {
            ImageData data = ArchivedImageDataList[i];
            if (data.IsDebugData)
            {
                ArchivedImageDataList.Remove(data);
            }
        }

        foreach (ImageData data in ActiveImageDataList)
        {
            var clone = data.Clone();
            clone.InAquarium = false;
            clone.CaptureTime = DateTime.Today - TimeSpan.FromDays(70);
            clone.Submitted = false;
            clone.InAquarium = false;
            clone.IsDebugData = true;
            ArchivedImageDataList.Add(clone);
        }

        ArchivedImageDataList.Sort(CompareCaptureTime);
    }

    private void LoadDataFromDisk()
    {
        string jsonString = PlayerPrefs.GetString(SaveKey, "[]");
        // TODO: 临时将 2023-06-19 改为 2023-08-19，保持数据有效
        jsonString = jsonString.Replace("2023-06-19", "2023-08-19");
        // -----------------------------
        if (!string.IsNullOrEmpty(jsonString) && jsonString != "null")
        {
            ImageDataList = JsonConvert.DeserializeObject<List<ImageData>>(jsonString);
            ClearMissingData();
        }
        else
        {
            ImageDataList = new List<ImageData>();
        }

        Resources.UnloadUnusedAssets();
    }

    private void ClearMissingData() { }

    private ImageData FindImageData(string fullFileName)
    {
        foreach (ImageData imageData in ImageDataList)
        {
            if (imageData.ImageFile == fullFileName)
            {
                return imageData;
            }
        }

        return null;
    }

    #endregion

    #region Event Callbacks

    private void OnSignedIn(string jsonData)
    {
        UpdateServerImageData(null);
    }

    #endregion
}