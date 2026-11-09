using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Network;
using Newtonsoft.Json;
using UnityEngine;

public class ServerCenter : Network.Singleton<ServerCenter>
{
    public void Init()
    {
        DontDestroyOnLoad();
        getEventsResultData = JsonConvert.DeserializeObject<GetEventsResultData>(GameSave.ServerEvents)
                              ?? new GetEventsResultData();
        getNftsResultData = JsonConvert.DeserializeObject<GetNftsResultData>(GameSave.ServerNfts)
                            ?? new GetNftsResultData();
        getUserInfoResultData = JsonConvert.DeserializeObject<GetUserInfoResultData>(GameSave.ServerUserInfo)
                                ?? new GetUserInfoResultData();
        getArtworksResultData = JsonConvert.DeserializeObject<GetArtworksResultData>(GameSave.ServerArtworks)
                                ?? new GetArtworksResultData();
        ArtworkFileList = JsonConvert.DeserializeObject<List<ArtworkFileItemResultData>>(GameSave.ServerArtworkFiles)
                          ?? new List<ArtworkFileItemResultData>();
    }

    #region Events

    public event Action<string> SignedInEvent;
    public event Action EventDataUpdatedEvent;
    public event Action UserInfoUpdatedEvent;
    public event Action AppNftsInfoUpdatedEvent;
    public event Action VersionInfoUpdatedEvent;
    public event Action ArtworkListUpdatedEvent;
    public event Action ArtworkPostEvent;
    public event Action ArtworkFileListUpdatedEvent;
    public event Action ArtworkFileUpdatedEvent;
    public event Action ArtworkFilePostEvent;

    #endregion

    #region Private Fields

    private string appleFirstName;
    private string appleLastName;
    private string appleUserId;
    private string appleIdToken;
    private string appleAuthCode;
    private string appleEmail;
    private GetEventsResultData getEventsResultData;
    private GetNftsResultData getNftsResultData;
    private GetUserInfoResultData getUserInfoResultData;
    private GetArtworksResultData getArtworksResultData;

    #endregion

    #region Public Fields

    public bool UseArtworkInMetaverse
    {
        get => PlayerPrefs.GetInt("USE_ARTWORK_IN_METAVERSE", 0) == 1;
        set => PlayerPrefs.SetInt("USE_ARTWORK_IN_METAVERSE", value ? 1 : 0);
    }

    public bool IsLoggedIn { get; private set; }
    public List<EventItemResultData> EventDataList => getEventsResultData.data;
    public List<NftItemResultData> NftDataList => getNftsResultData.data;
    public UserInfoResultData UserInfoData => getUserInfoResultData.data;
    public List<ArtworkFileItemResultData> ArtworkFileList = new();
    public List<ArtworkItemResultData> ArtworkList => getArtworksResultData.data;
    public LatestVersionResultData LatestVersionData;

    public VersionStats VersionStats => LatestVersionData == null
        ? VersionStats.Unkown
        : LatestVersionData.versionId > GameSetting.Instance.VersionId
            ? VersionStats.NeedUpdate
            : VersionStats.Latest;

    public string AppleUserId
    {
        get => Application.isEditor ? "romi.fauzi" : (string.IsNullOrEmpty(appleUserId) ? UserInfoData.appleId : appleUserId);
        set => appleUserId = value;
    }

    public string AppleIdToken
    {
        get => string.IsNullOrEmpty(appleIdToken) ? GameSave.AppleIdToken : appleIdToken;
        set
        {
            appleIdToken = value;
            GameSave.AppleIdToken = value;
        }
    }

    public string AppleAuthCode
    {
        get => string.IsNullOrEmpty(appleAuthCode) ? GameSave.AppleAuthCode : appleAuthCode;
        set
        {
            appleAuthCode = value;
            GameSave.AppleAuthCode = value;
        }
    }

    public string AppleEmail
    {
        get => string.IsNullOrEmpty(appleEmail) ? UserInfoData.email : appleEmail;
        set
        {
            if (string.IsNullOrEmpty(AppleEmail))
            {
                appleEmail = value;
            }
        }
    }

    public string AppleFirstName
    {
        get => string.IsNullOrEmpty(appleFirstName) ? UserInfoData.firstName : appleFirstName;
        set
        {
            if (string.IsNullOrEmpty(AppleFirstName))
            {
                appleFirstName = value;
                UserInfoData.firstName = value;
            }
        }
    }

    public string AppleLastName
    {
        get => string.IsNullOrEmpty(appleLastName) ? UserInfoData.lastName : appleLastName;
        set
        {
            if (string.IsNullOrEmpty(AppleLastName))
            {
                appleLastName = value;
                UserInfoData.lastName = value;
            }
        }
    }

    public string AppleName => $"{AppleFirstName} {AppleLastName}";

    #endregion

    #region Common

    private string TimeStamp
    {
        get
        {
            TimeSpan elapse = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            return elapse.TotalSeconds.ToString("0", CultureInfo.InvariantCulture);
        }
    }

    private string Secret
    {
        get
        {
            string source = $"{AppleUserId}{TimeStamp}";
            byte[] sourceBytes = System.Text.Encoding.UTF8.GetBytes(source);
            string result = BitConverter.ToString(MD5.Create().ComputeHash(sourceBytes));
            result = result.Replace("-", "").ToLower();
            return result;
        }
    }

    private void AddAuthHeader(WebRequest request)
    {
        request.SetHeader("time_stamp", TimeStamp);
        request.SetHeader("appleid", AppleUserId);
        request.SetHeader("refresh_token", AppleAuthCode);
        request.SetHeader("secret", Secret);
    }

    #endregion

    #region Version

    public void UpdateLatestVersion(Action<string> onSuccess = null, Action onFail = null)
    {
        StartCoroutine(RequestVersionInfo(
            jsonData =>
            {
                GetLatestVersionData getLatestVersionData = JsonConvert.DeserializeObject<GetLatestVersionData>(jsonData);
                LatestVersionData = getLatestVersionData.data.Length > 0 
                    ? getLatestVersionData.data[0]
                    : new LatestVersionResultData();
                onSuccess?.Invoke(jsonData);
                VersionInfoUpdatedEvent?.Invoke();
            },
            () =>
            {
                onFail?.Invoke();
            }));
    }

    #endregion

    #region Terms

    public void GetTerms(Action<string> onSuccess, Action onFail)
    {
        StartCoroutine(RequestTerms(
            jsonData =>
            {
                onSuccess?.Invoke(jsonData);
            },
            () =>
            {
                onFail?.Invoke();
            }));
    }

    public void AcceptTerms(Action<string> onSuccess = null, Action onFail = null)
    {
        StartCoroutine(PostTerms(
            jsonData =>
            {
                onSuccess?.Invoke(jsonData);
            },
            () =>
            {
                onFail?.Invoke();
            }));
    }

    #endregion

    #region Account

    public void SignInWithApple(Action<string> onSuccess, Action onFail, Action onBanned)
    {
        // local apple sign in
        AppleLogin.Instance.LoginToApple(
            (user, idToken, authCode, email, firstName, lastName) =>
            {
                {
                    AppleUserId = user;
                    AppleIdToken = idToken;
                    AppleAuthCode = authCode;
                    AppleEmail = email;
                    AppleFirstName = firstName;
                    AppleLastName = lastName;
                    StartCoroutine(LoginToServer(
                        jsonData =>
                        {
                            var resultData = JsonConvert.DeserializeObject<LoginResultData>(jsonData);
                            if (resultData.message == "Account Disabled")
                            {
                                onBanned?.Invoke();
                            }
                            else
                            {
                                IsLoggedIn = true;
                                UpdateUserInfo();
                                onSuccess?.Invoke(jsonData);
                                SignedInEvent?.Invoke(jsonData);
                                Firebase.Analytics.FirebaseAnalytics
                                    .LogEvent(Firebase.Analytics.FirebaseAnalytics.EventLogin);
                            }
                        },
                        () => onFail?.Invoke()
                    ));
                }
            },
            () =>
            {
                onFail?.Invoke();
            });
    }

    public void UpdateUserInfo(Action<string> onSuccess = null, Action onFail = null)
    {
        StartCoroutine(RequestUserInfo(
            jsonData =>
            {
                getUserInfoResultData = JsonConvert.DeserializeObject<GetUserInfoResultData>(jsonData);
                GameSave.ServerUserInfo = jsonData;
                onSuccess?.Invoke(jsonData);
                UserInfoUpdatedEvent?.Invoke();
            },
            () =>
            {
                onFail?.Invoke();
            }));
    }

    #endregion

    #region Events

    public void UpdateEvents(Action onSuccess = null, Action onFail = null)
    {
        StartCoroutine(RequestEvents(
            jsonData =>
            {
                getEventsResultData = JsonConvert.DeserializeObject<GetEventsResultData>(jsonData);
                GameSave.ServerEvents = jsonData;
                onSuccess?.Invoke();
                EventDataUpdatedEvent?.Invoke();
            },
            () =>
            {
                onFail?.Invoke();
            }));
    }

    /// <summary>
    /// Get the nearest active event or upcoming event.
    /// UpdateEvents() should be called first.
    /// </summary>
    public EventItemResultData GetCurrentEventData()
    {
        EventItemResultData result = null;
        TimeSpan timeSpan = TimeSpan.MaxValue;
        foreach (EventItemResultData data in EventDataList)
        {
            EventStatus eventStatus = data.GetStatus();
            if (eventStatus == EventStatus.Active
                && data.timeSpan < timeSpan && data.image.Contains("tpl_"))
            {
                timeSpan = data.timeSpan;
                result = data;
            }
        }

        if (result == null)
        {
            timeSpan = TimeSpan.MaxValue;
            foreach (EventItemResultData data in EventDataList)
            {
                EventStatus eventStatus = data.GetStatus();
                if (eventStatus == EventStatus.Coming
                    && data.timeSpan < timeSpan && data.image.Contains("tpl_"))
                {
                    timeSpan = data.timeSpan;
                    result = data;
                }
            }
        }

        if (Debugger.Instance.EventIndex >= 0)
        {
            var nowUtc = DateTime.UtcNow;
            foreach (EventItemResultData data in EventDataList)
            {
                // 0: coming soon
                if (Debugger.Instance.EventIndex == 0 && data.eventId == 0)
                {
                    data.startTime = nowUtc + new TimeSpan(20, 15, 31, 23);
                    data.endTime = nowUtc + new TimeSpan(40, 15, 31, 23);
                    result = data;
                    break;
                }

                // 1: start in 7 days
                if (Debugger.Instance.EventIndex == 1 && data.eventId == 1)
                {
                    data.startTime = nowUtc + new TimeSpan(6, 15, 31, 23);
                    data.endTime = nowUtc + new TimeSpan(40, 15, 31, 23);
                    result = data;
                    break;
                }

                // 2: start soon (in 1 day)
                if (Debugger.Instance.EventIndex == 2 && data.eventId == 2)
                {
                    data.startTime = nowUtc + new TimeSpan(0, 8, 31, 23);
                    data.endTime = nowUtc + new TimeSpan(40, 15, 31, 23);
                    result = data;
                    break;
                }

                // 3: active (more than 1 day left)
                if (Debugger.Instance.EventIndex == 3 && data.eventId == 3)
                {
                    data.startTime = nowUtc - new TimeSpan(6, 15, 31, 23);
                    data.endTime = nowUtc + new TimeSpan(3, 15, 31, 23);
                    result = data;
                    break;
                }

                // 4: end soon (in 1 day)
                if (Debugger.Instance.EventIndex == 4 && data.eventId == 4)
                {
                    data.startTime = nowUtc - new TimeSpan(6, 15, 31, 23);
                    data.endTime = nowUtc + new TimeSpan(0, 15, 31, 23);
                    result = data;
                    break;
                }

                // 5: ended
                if (Debugger.Instance.EventIndex == 5 && data.eventId == 5)
                {
                    data.startTime = nowUtc - new TimeSpan(14, 15, 31, 23);
                    data.endTime = nowUtc - new TimeSpan(4, 15, 31, 23);
                    result = data;
                    break;
                }
            }
        }

        if (Debugger.Instance.EventImageIndex >= 0)
        {
            // Define the regex pattern to match the file name
            bool isMock = result.image.Contains("download?file");
            string pattern = isMock ? @"file=([^/]+)$" : @"/([^/]+)$";
            string preString = isMock ? "file=" : "/";
            Regex regex = new Regex(pattern);
            result.image = Debugger.Instance.EventImageIndex switch
            {
                1 => regex.Replace(result.image, preString + "tpl_outline_BlueShark_Cartoon.png"),
                2 => regex.Replace(result.image, preString + "tpl_outline_Dolphin_Cartoon.png"),
                3 => regex.Replace(result.image, preString + "tpl_outline_MobulaRay_Cartoon.png"),
                4 => regex.Replace(result.image, preString + "tpl_outline_Orca_Cartoon.png"),
                5 => regex.Replace(result.image, preString + "tpl_outline_Pike_Cartoon.png"),
                6 => regex.Replace(result.image, preString + "tpl_outline_RainbowCichlid_Cartoon.png"),
                7 => regex.Replace(result.image, preString + "tpl_outline_Shark_Cartoon.png"),
                8 => regex.Replace(result.image, preString + "tpl_outline_SunFish_Cartoon.png"),
                9 => regex.Replace(result.image, preString + "tpl_outline_TangFish_Cartoon.png"),
                10 => regex.Replace(result.image, preString + "tpl_outline_Turtle_Cartoon.png"),
                11 => regex.Replace(result.image, preString + "tpl_outline_Whale_Cartoon.png"),
                12 => regex.Replace(result.image, preString + "tpl_outline_BoxFish_Cartoon.png"),
                13 => regex.Replace(result.image, preString + "tpl_outline_BlueMarlin_Cartoon.png"),

                // 1 => regex.Replace(result.image, preString + "tpl_outline_ArcherFish_Cartoon.png"),
                // 2 => regex.Replace(result.image, preString + "tpl_outline_Barracuda_Cartoon.png"),
                // 3 => regex.Replace(result.image, preString + "tpl_outline_BlueMarlin_Cartoon.png"),
                // 2 => regex.Replace(result.image, preString + "tpl_outline_BoxFish_Cartoon.png"),
                // 5 => regex.Replace(result.image, preString + "tpl_outline_CoralFish_Cartoon"),
                // 7 => regex.Replace(result.image, preString + "tpl_outline_JellyFish_Cartoon.png"),
                // 5 => regex.Replace(result.image, preString + "tpl_outline_Nemo_Cartoon.png"),
                // 10 => regex.Replace(result.image, preString + "tpl_outline_Octopus_Cartoon.png"),
                // 9 => regex.Replace(result.image, preString + "tpl_outline_RoyalDottyBack_Cartoon.png"),
                // 16 => regex.Replace(result.image, preString + "tpl_outline_StarFish_Cartoon.png"),
                _ => regex.Replace(result.image, preString + "tpl_outline_TangFish_Cartoon.png")
            };
        }

        return result;
    }

    public string GetTemplateUrlByFileName(string templateFile)
    {
        string result = null;
        foreach (EventItemResultData eventData in EventDataList)
        {
            string file = ImageFileNameUtils.GetFullFileNameFromUrl(eventData.image);
            if (file == templateFile)
            {
                result = eventData.image;
                break;
            }
        }

        return result;
    }

    #endregion

    #region Artwork

    public void GetImageFromCacheOrUrl(string url, bool small, Action<Texture2D> onSuccess = null, Action onFail = null)
    {
        StartCoroutine(RequestImage(url, small, false, texture =>
        {
            onSuccess?.Invoke(texture);
        }, () =>
        {
            onFail?.Invoke();
        }));
    }

    public void SubmitArtwork(ImageData imageData, Action<ImageData, bool> onSuccess, Action onFail)
    {
        if (NetworkSettings.Instance.IsMock)
        {
            Debug.Log($"[{GetType().Name}] Submit artwork successfully: {imageData.ImageFile}");
            onSuccess?.Invoke(imageData, true);
        }
        else
        {
            UpdateArtworkList(
                _ =>
                {
                    foreach (ArtworkItemResultData data in ArtworkList)
                    {
                        if (data.name == imageData.ImageFile)
                        {
                            Debug.Log($"[{GetType().Name}] Artwork data exists on server already.");
                            onSuccess?.Invoke(imageData, true);
                            return;
                        }
                    }

                    UploadArtworkFile(imageData, _ =>
                        {
                            Debug.Log($"[{GetType().Name}] Submit artwork file successfully: {imageData.ImageFile}");
                            UpdateArtworkFileList(
                                _ =>
                                {
                                    ArtworkFileItemResultData data = null;
                                    foreach (ArtworkFileItemResultData resultData in ArtworkFileList)
                                    {
                                        if (resultData.name == imageData.ImageFile)
                                        {
                                            data = resultData;
                                            imageData.Url = data.url;
                                            break;
                                        }
                                    }

                                    if (data != null)
                                    {
                                        UploadArtwork(imageData.EventId, imageData.Type, data.url,
                                            () =>
                                            {
                                                Debug.Log($"[{GetType().Name}] Submit artwork successfully: {imageData.ImageFile}");
                                                onSuccess?.Invoke(imageData, true);
                                            },
                                            () => onFail?.Invoke()
                                        );
                                    }
                                },
                                () => onFail?.Invoke()
                            );
                        },
                        () => onFail?.Invoke()
                    );
                },
                () => onFail?.Invoke());
        }
    }

    public void UpdateNftsInfo(Action<string> onSuccess, Action onFail)
    {
        StartCoroutine(RequestAppNftsInfo(jsonData =>
        {
            try
            {
                getNftsResultData = JsonConvert.DeserializeObject<GetNftsResultData>(jsonData);
                GameSave.ServerNfts = jsonData;
                foreach (NftItemResultData data in getNftsResultData.data)
                {
                    if (ImageFileNameUtils.IsValidArtworkFileName(data.fileName))
                    {
                        StartCoroutine(RequestImage(data.imageURL, false, true, DestroyImmediate, null));
                    }
                }
            }
            catch (Exception)
            {
                Debug.Log($"[{GetType().Name}] wrong nfts info: {jsonData}");
                getNftsResultData = new GetNftsResultData();
            }

            onSuccess?.Invoke(jsonData);
            AppNftsInfoUpdatedEvent?.Invoke();
        }, () =>
        {
            onFail?.Invoke();
        }));
    }

    public void UpdateArtworkList(Action<string> onSuccess, Action onFail)
    {
        StartCoroutine(RequestArtworkList(jsonData =>
        {
            getArtworksResultData = JsonConvert.DeserializeObject<GetArtworksResultData>(jsonData);
            GameSave.ServerArtworks = jsonData;
            onSuccess?.Invoke(jsonData);
            ArtworkFileListUpdatedEvent?.Invoke();
            // may be too much

            // float delay = 0;
            // foreach (ArtworkItemResultData data in getArtworksResultData.data)
            // {
            //     if (ImageFileNameUtils.IsValidArtworkFileName(data.name))
            //     {
            //         StartCoroutine(RequestImage(data.URL, false, true, DestroyImmediate, null, delay));
            //     }
            //
            //     delay += 0.2f;
            // }
        }, () =>
        {
            onFail?.Invoke();
        }));
    }

    public void UploadArtwork(
        int eventId,
        string type,
        string url, // used to save artwork file id
        Action onSuccess,
        Action onFail)
    {
        StartCoroutine(PostArtwork(eventId, type, url,
            _ =>
            {
                onSuccess?.Invoke();
                ArtworkFilePostEvent?.Invoke();
            }, () =>
            {
                onFail?.Invoke();
            }));
    }

    public void UpdateArtworkFileList(Action<string> onSuccess, Action onFail)
    {
        StartCoroutine(RequestArtworkFileList(jsonData =>
        {
            ArtworkFileList = JsonConvert.DeserializeObject<List<ArtworkFileItemResultData>>(jsonData);
            GameSave.ServerArtworkFiles = jsonData;
            onSuccess?.Invoke(jsonData);
            ArtworkFileListUpdatedEvent?.Invoke();
        }, () =>
        {
            onFail?.Invoke();
        }));
    }

    public void DownArtworkFile(string artworkFileId, Action<Texture2D> onSuccess, Action onFail)
    {
        StartCoroutine(RequestArtworkFile(artworkFileId, texture =>
        {
            onSuccess?.Invoke(texture);
            ArtworkFileUpdatedEvent?.Invoke();
        }, () =>
        {
            onFail?.Invoke();
        }));
    }

    public void UploadArtworkFile(ImageData imageData, Action<string> onSuccess, Action onFail)
    {
        StartCoroutine(PostArtworkFile(imageData.ImageFile, jsonData =>
        {
            onSuccess?.Invoke(jsonData);
            ArtworkFilePostEvent?.Invoke();
        }, () =>
        {
            onFail?.Invoke();
        }));
    }

    #endregion

    #region API

    // checkOnly: don't load local texture into memory
    private IEnumerator RequestImage(string url, bool small, bool checkOnly, Action<Texture2D> onSuccess, Action onFail, float delay = 0)
    {
        if (delay > float.Epsilon)
        {
            yield return new WaitForSeconds(delay);
        }

        string fullFileName = ImageFileNameUtils.GetFullFileNameFromUrl(url);
        Texture2D texture;
        if (ImageDataWriter.CheckImageExists(fullFileName, small))
        {
            if (checkOnly)
            {
                yield break;
            }
            else
            {
                texture = TextureCache.Instance.Load(fullFileName, small);
                if (texture != null)
                {
                    Debug.Log($"[{GetType().Name}] Get image from cache: {ImageFileNameUtils.GetSmallImageFullFileName(fullFileName)}");
                    onSuccess?.Invoke(texture);
                    yield break;
                }
            }
        }

        if (NetworkSettings.Instance.IsMock)
        {
            url = url.Replace(":2021", ":" + MockServer.Instance.PortNumber);
        }

        Debug.Log($"[{GetType().Name}] Request image from server: {url}");

        var request = WebRequest.GetTexture(url)
            .SetMaxRetries(3)
            .SetTimeout(30);

        yield return StartCoroutine(request.Send(MockJsonFileNameEnum.None, null));

        if (request.IsSuccessful())
        {
            texture = request.GetTexture();
            if (texture != null)
            {
                Debug.Log($"[{GetType().Name}] Get image from server successfully: {url}");
                ServerResponseRecorder.Instance.SaveMockImageFile(fullFileName, texture); // mock and in editor
                ImageDataWriter.SaveImage(texture, fullFileName);
                Destroy(texture);
                if (!checkOnly)
                {
                    texture = TextureCache.Instance.Load(fullFileName, small);
                    onSuccess?.Invoke(texture);
                }
            }
            else
            {
                Debug.Log($"[{GetType().Name}] Get image from server failed: {url}");
                if (!checkOnly)
                {
                    onFail?.Invoke();
                }
            }
        }
        else
        {
            Debug.Log($"[{GetType().Name}] Get image from server failed: {url}");
            if (!checkOnly)
            {
                onFail?.Invoke();
            }
        }
    }

    private IEnumerator RequestTerms(Action<string> onSuccess, Action onFail)
    {
        Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.GetTerms}");
        var request = WebRequest
            .Get(WebRequest.GetApiUrl(ServerApiEnum.Terms) + $"/{AppleUserId}")
            .SetMaxRetries(NetworkSettings.Instance.MaxRetryTimes)
            .SetTimeout(NetworkSettings.Instance.Timeout);
        yield return StartCoroutine(request.Send(MockJsonFileNameEnum.GetTerms, null));
        if (request.IsSuccessful())
        {
            Debug.Log($"[{GetType().Name}] RequestTerms 成功获取数据：{request.GetText()}");
            onSuccess?.Invoke(request.GetText());
        }
        else
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.GetTerms} 获取数据失败 {request}");
            onFail?.Invoke();
        }
    }

    private IEnumerator PostTerms(Action<string> onSuccess, Action onFail)
    {
        string jsonData = "";
        Debug.Log($"[{GetType().Name}] PostTerms 上传数据：{jsonData}");
        var request = WebRequest
            .Post(WebRequest.GetApiUrl(ServerApiEnum.Terms) + $"/{AppleUserId}", jsonData)
            .SetMaxRetries(NetworkSettings.Instance.MaxRetryTimes)
            .SetTimeout(NetworkSettings.Instance.Timeout);
        AddAuthHeader(request);
        yield return StartCoroutine(request.Send(MockJsonFileNameEnum.PostTerms, null));
        if (request.IsSuccessful())
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.PostTerms} 成功获取数据：{request.GetText()}");
            onSuccess?.Invoke(request.GetText());
        }
        else
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.PostTerms} 获取数据失败");
            onFail?.Invoke();
        }
    }

    private IEnumerator LoginToServer(Action<string> onSuccess, Action onFail)
    {
        string jsonData = JsonConvert.SerializeObject(new LoginRequestData
        {
            name = new LoginRequestNameData
            {
                firstName = AppleFirstName ?? "",
                lastName = AppleLastName ?? ""
            },
            email = AppleEmail ?? "",
            refresh_token = AppleAuthCode,
            id_token = AppleIdToken
        });
        Debug.Log($"[{GetType().Name}] LoginToServer 上传数据：{jsonData}");
        var request = WebRequest
            .Post(WebRequest.GetApiUrl(ServerApiEnum.Login) + $"/{AppleUserId}", jsonData)
            .SetMaxRetries(NetworkSettings.Instance.MaxRetryTimes)
            .SetTimeout(NetworkSettings.Instance.Timeout);
        AddAuthHeader(request);
        yield return StartCoroutine(request.Send(MockJsonFileNameEnum.Login, null));
        if (request.IsSuccessful())
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.Login} 成功获取数据：{request.GetText()}");
            onSuccess?.Invoke(request.GetText());
        }
        else
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.Login} 获取数据失败");
            onFail?.Invoke();
        }
    }

    private IEnumerator RequestUserInfo(Action<string> onSuccess, Action onFail)
    {
        Debug.Log($"[{GetType().Name}] RequestUserInfo");
        var request = WebRequest
            .Get(WebRequest.GetApiUrl(ServerApiEnum.User + $"/{AppleUserId}"))
            .SetMaxRetries(NetworkSettings.Instance.MaxRetryTimes)
            .SetTimeout(NetworkSettings.Instance.Timeout);
        AddAuthHeader(request);
        yield return StartCoroutine(request.Send(MockJsonFileNameEnum.GetUserInfo, null));
        if (request.IsSuccessful())
        {
            string jsonData = request.GetText();
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.GetUserInfo} 成功获取数据：{jsonData}");
            GetUserInfoResultData data = JsonConvert.DeserializeObject<GetUserInfoResultData>(jsonData);
            AppleEmail = data.data.email;
            AppleFirstName = data.data.firstName;
            AppleLastName = data.data.lastName;
            onSuccess?.Invoke(jsonData);
        }
        else
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.GetUserInfo} 获取数据失败");
            onFail?.Invoke();
        }
    }

    private IEnumerator RequestEvents(Action<string> onSuccess, Action onFail)
    {
        Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.GetEvents}");
        var request = WebRequest
            .Get(WebRequest.GetApiUrl(ServerApiEnum.GetEvent))
            .SetMaxRetries(NetworkSettings.Instance.MaxRetryTimes)
            .SetTimeout(NetworkSettings.Instance.Timeout);
        yield return StartCoroutine(request.Send(MockJsonFileNameEnum.GetEvents, null));
        if (request.IsSuccessful())
        {
            Debug.Log($"[{GetType().Name}] RequestEvents 成功获取数据：{request.GetText()}");
            onSuccess?.Invoke(request.GetText());
        }
        else
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.GetEvents} 获取数据失败");
            onFail?.Invoke();
        }
    }

    private IEnumerator RequestAppNftsInfo(Action<string> onSuccess, Action onFail)
    {
        Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.GetAppNftsInfo}");
        var request = WebRequest
            .Get(WebRequest.GetApiUrl(ServerApiEnum.GetAppNftsInfo + $"/{AppleUserId}"))
            .SetMaxRetries(NetworkSettings.Instance.MaxRetryTimes)
            .SetTimeout(NetworkSettings.Instance.Timeout);
        AddAuthHeader(request);
        yield return StartCoroutine(request.Send(MockJsonFileNameEnum.GetAppNftsInfo, null));
        if (request.IsSuccessful())
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.GetAppNftsInfo} 成功获取数据：{request.GetText()}");
            onSuccess?.Invoke(request.GetText());
        }
        else
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.GetAppNftsInfo} 获取数据失败");
            onFail?.Invoke();
        }
    }

    private IEnumerator RequestVersionInfo(Action<string> onSuccess, Action onFail)
    {
        Debug.Log($"[{GetType().Name}] RequestVersionInfo");
        var request = WebRequest
            .Get(WebRequest.GetApiUrl(ServerApiEnum.GetLatestVersion))
            .SetMaxRetries(NetworkSettings.Instance.MaxRetryTimes)
            .SetTimeout(NetworkSettings.Instance.Timeout);
        yield return StartCoroutine(request.Send(MockJsonFileNameEnum.GetLatestVersion, null));
        if (request.IsSuccessful())
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.GetLatestVersion} 成功获取数据：{request.GetText()}");
            onSuccess?.Invoke(request.GetText());
        }
        else
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.GetLatestVersion} 获取数据失败");
            onFail?.Invoke();
        }
    }

    private IEnumerator RequestArtworkList(Action<string> onSuccess, Action onFail)
    {
        Debug.Log($"[{GetType().Name}] RequestArtworkList");
        var request = WebRequest
            .Get(WebRequest.GetApiUrl(ServerApiEnum.Artwork + $"/{AppleUserId}"))
            .SetMaxRetries(NetworkSettings.Instance.MaxRetryTimes)
            .SetTimeout(NetworkSettings.Instance.Timeout);
        AddAuthHeader(request);
        yield return StartCoroutine(request.Send(MockJsonFileNameEnum.GetArtworkList, null));
        if (request.IsSuccessful())
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.GetArtworkList} 成功获取数据：{request.GetText()}");
            onSuccess?.Invoke(request.GetText());
        }
        else
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.GetArtworkList} 获取数据失败");
            onFail?.Invoke();
        }
    }

    private IEnumerator PostArtwork(
        int eventId,
        string type,
        string url, // used to save artwork file id
        Action<string> onSuccess,
        Action onFail)
    {
        var data = new ArtworkItemRequestData
        {
            appleId = AppleUserId,
            eventId = eventId,
            URL = url,
            type = type,
            artistMessage = "normal",
            ifHomepage = 0,
            ifMinted = 0,
            ifRedeemed = 0,
            isActive = 1
        };
        string jsonData = JsonConvert.SerializeObject(new[] { data });
        Debug.Log($"[{GetType().Name}] PostArtwork 上传数据：{jsonData}");
        var request = WebRequest
            .Post(WebRequest.GetApiUrl(ServerApiEnum.Artwork), jsonData)
            .SetMaxRetries(NetworkSettings.Instance.MaxRetryTimes)
            .SetTimeout(NetworkSettings.Instance.Timeout);
        AddAuthHeader(request);
        yield return StartCoroutine(request.Send(MockJsonFileNameEnum.PostArtwork, null));
        if (request.IsSuccessful())
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.PostArtwork} 成功获取数据：{request.GetText()}");
            onSuccess?.Invoke(request.GetText());
        }
        else
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.PostArtwork} 获取数据失败");
            onFail?.Invoke();
        }
    }

    // BUG: server always return appleId doesn't exist.
    private IEnumerator PutArtwork(
        string name, // the same as ImageData.ImageFile
        string url, // used to save artwork file id
        string createTime,
        Action<string> onSuccess,
        Action onFail)
    {
        var data = new ArtworkItemRequestData
        {
            appleId = AppleUserId,
            eventId = 1,
            URL = url,
            type = "fish",
            artistMessage = "normal",
            ifHomepage = 0,
            ifMinted = 0,
            ifRedeemed = 0,
            isActive = 1
        };
        string jsonData = JsonConvert.SerializeObject(new[] { data });
        Debug.Log($"[{GetType().Name}] PutArtwork 上传数据：{jsonData}");
        var request = WebRequest
            .Put(WebRequest.GetApiUrl(ServerApiEnum.Artwork + $"/{AppleUserId}"), jsonData)
            .SetMaxRetries(NetworkSettings.Instance.MaxRetryTimes)
            .SetTimeout(NetworkSettings.Instance.Timeout);
        AddAuthHeader(request);
        yield return StartCoroutine(request.Send(MockJsonFileNameEnum.PostArtwork, null));
        if (request.IsSuccessful())
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.PostArtwork} 成功获取数据：{request.GetText()}");
            onSuccess?.Invoke(request.GetText());
        }
        else
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.PostArtwork} 获取数据失败");
            onFail?.Invoke();
        }
    }

    private IEnumerator RequestArtworkFileList(Action<string> onSuccess, Action onFail)
    {
        Debug.Log($"[{GetType().Name}] RequestArtworkFileList");
        var request = WebRequest
            .Get(WebRequest.GetApiUrl(ServerApiEnum.ArtworkFile))
            .SetMaxRetries(NetworkSettings.Instance.MaxRetryTimes)
            .SetTimeout(NetworkSettings.Instance.Timeout);
        AddAuthHeader(request);
        yield return StartCoroutine(request.Send(MockJsonFileNameEnum.GetArtworkFileList, null));
        if (request.IsSuccessful())
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.GetArtworkFileList} 成功获取数据：{request.GetText()}");
            onSuccess?.Invoke(request.GetText());
        }
        else
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.GetArtworkFileList} 获取数据失败");
            onFail?.Invoke();
        }
    }

    private IEnumerator RequestArtworkFile(string artworkFileId, Action<Texture2D> onSuccess, Action onFail)
    {
        Debug.Log($"[{GetType().Name}] RequestArtworkFile");

        string url;
        if (NetworkSettings.Instance.IsMock)
        {
            url = "http://127.0.0.1:2021/download?file=tpl_outline_TangFish_Cartoon.png";
            url = url.Replace(":2021", ":" + MockServer.Instance.PortNumber);
        }
        else
        {
            url = WebRequest.GetApiUrl(ServerApiEnum.ArtworkFile + $"/{artworkFileId}");
        }

        var request = WebRequest.GetTexture(url)
            .SetMaxRetries(NetworkSettings.Instance.MaxRetryTimes)
            .SetTimeout(NetworkSettings.Instance.Timeout);
        AddAuthHeader(request);
        yield return StartCoroutine(request.Send(MockJsonFileNameEnum.GetArtworkFile, null));
        if (request.IsSuccessful())
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.GetArtworkFile} 成功获取数据：{request.GetText()}");
            Texture2D texture = request.GetTexture();
            if (texture != null)
            {
                Debug.Log($"[{GetType().Name}] Get artwork file from server successfully: {artworkFileId}");
                onSuccess?.Invoke(texture);
            }
            else
            {
                Debug.Log($"[{GetType().Name}] Get artwork file from server failed: {artworkFileId}");
                onFail?.Invoke();
            }
        }
        else
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.GetArtworkFile} 获取数据失败");
            onFail?.Invoke();
        }
    }

    private IEnumerator PostArtworkFile(string fullFileName, Action<string> onSuccess, Action onFail)
    {
        Debug.Log($"[{GetType().Name}] PostArtworkFile");

        var form = new WWWForm();
        form.AddBinaryData("file", ImageDataWriter.GetImageRawData(fullFileName), fullFileName, "image/jpg");
        var request = WebRequest
            .Post(WebRequest.GetApiUrl(ServerApiEnum.ArtworkFile), form)
            .SetMaxRetries(NetworkSettings.Instance.MaxRetryTimes)
            .SetTimeout(NetworkSettings.Instance.Timeout);
        AddAuthHeader(request);
        yield return StartCoroutine(request.Send(MockJsonFileNameEnum.PostArtworkFile, null));
        if (request.IsSuccessful())
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.PostArtworkFile} 成功获取数据：{request.GetText()}");
            onSuccess?.Invoke(request.GetText());
        }
        else
        {
            Debug.Log($"[{GetType().Name}] {MockJsonFileNameEnum.PostArtworkFile} 获取数据失败");
            onFail?.Invoke();
        }
    }

    #endregion
}

public enum VersionStats
{
    Unkown, NeedUpdate, Latest
}