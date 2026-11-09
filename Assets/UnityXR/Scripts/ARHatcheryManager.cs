using System.Collections.Generic;
using System.Collections;
using ProGamer;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARSessionOrigin))]
public class ARHatcheryManager : SceneInstance<ARHatcheryManager>
{
    public GameObject Environment { get; private set; }
    public GameObject PreviewObject { get; private set; }

    public const float VisualizeDistance = 0.5f;
    public const float PlaceLeastDistance = 0.1f;
    public const float DeactivePreviewModifier = 5f;

    [SerializeField] protected GameObject findSurfaceIcon;
    [SerializeField] protected GameObject message;
    [SerializeField] protected GameObject seaGO;
    [SerializeField] protected GameObject waterLightGO;
    [SerializeField] protected GameObject bubblesGO;
    [SerializeField] protected GameObject fadeInGO;
    [SerializeField] protected GameObject fadeOutGO;

    protected ARGate arGate;

    protected ARSessionOrigin m_SessionOrigin;

    protected ARSessionOrigin SessionOrigin
    {
        get
        {
            if (m_SessionOrigin == null)
            {
                m_SessionOrigin = GetComponent<ARSessionOrigin>();
            }

            return m_SessionOrigin;
        }
    }

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    bool isPlacedModel = false;
    bool isVisualizedEnvironment = false;
    bool isFoundGround = false;

    Vector2 screenCenter;
    private ARRaycastManager raycastManager;

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    private void Start()
    {
        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        AquariumData data = DataCenter.Instance.GetAquarium(GameSave.SelectedAquarium);

        PreviewObject = Instantiate(Resources.Load<GameObject>(GameSetting.ModelPrefabPath + data.Data.Preview));

        PreviewObject.transform.position = SessionOrigin.camera.ScreenToWorldPoint(new Vector3(screenCenter.x, screenCenter.y, SessionOrigin.camera.nearClipPlane * DeactivePreviewModifier));
        PreviewObject.transform.forward = -SessionOrigin.camera.transform.forward;
        PreviewObject.transform.localScale = Vector3.one;

        arGate = PreviewObject.GetComponent<ARGate>();

        Animator an = PreviewObject.GetComponent<Animator>();

        an.CrossFade("idle", 0);

        if (this.SessionOrigin != null && this.SessionOrigin.trackablesParent != null)
        {
            this.SessionOrigin.trackablesParent.gameObject.SetActive(false);
        }

        //disable blue layout and water filter
        this.gameObject.transform.Find("AR Camera").GetComponent<CameraFilterPack_Distortion_Dream2>().enabled = false;
        this.gameObject.transform.Find("AR Camera").GetComponent<CameraFilterPack_Color_RGB>().enabled = false;
    }

    void Update()
    {
        if (!isPlacedModel)
        {
            isFoundGround = raycastManager.Raycast(screenCenter, s_Hits, TrackableType.PlaneWithinPolygon);
            if (isFoundGround)
            {
                findSurfaceIcon.SetActive(false);
                Localization.SetText("txt_tap_to_place", message.GetComponent<Text>());
                if (!PreviewObject.activeSelf)
                {
                    PreviewObject.SetActive(true);
                    PreviewObject.GetComponent<Animator>().CrossFade("idle", 0);
                }

                Pose hitPose = s_Hits[0].pose;

                PreviewObject.transform.position = hitPose.position;
                PreviewObject.transform.rotation = hitPose.rotation;
                PreviewObject.transform.forward = -SessionOrigin.camera.transform.forward;
                PreviewObject.transform.eulerAngles = new Vector3(0, PreviewObject.transform.eulerAngles.y, PreviewObject.transform.eulerAngles.z);

                //Place model
                float distance = Vector3.Distance(SessionOrigin.camera.transform.position.ClearY(), PreviewObject.transform.position.ClearY());
                arGate.WaterfloorAttachedTheGround(distance > PlaceLeastDistance);

                if (Input.touchCount > 0 || Input.GetMouseButtonUp(0))
                {
                    if (distance > PlaceLeastDistance)
                    {
                        isPlacedModel = true;
                        Animator an = PreviewObject.GetComponent<Animator>();
                        an.CrossFade("gate", 0);
                        Localization.SetText("txt_tutorial_step_23", message.GetComponent<Text>());

                        if (this.SessionOrigin != null && this.SessionOrigin.trackablesParent != null)
                        {
                            this.SessionOrigin.trackablesParent.gameObject.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                /*PreviewObject.transform.position = SessionOrigin.camera.ScreenToWorldPoint(new Vector3(screenCenter.x, screenCenter.y, SessionOrigin.camera.nearClipPlane * DeactivePreviewModifier));
                PreviewObject.transform.forward = -SessionOrigin.camera.transform.forward;*/
                PreviewObject.SetActive(false);
                arGate.WaterfloorAttachedTheGround(false);

                findSurfaceIcon.SetActive(true);
                Localization.SetText("txt_point_to_ground", message.GetComponent<Text>());
            }
        }
        else
        {
            if (!isVisualizedEnvironment)
            {
                //Check distance
                if (Vector3.Distance(SessionOrigin.camera.transform.position.ClearY(), PreviewObject.transform.position.ClearY()) < VisualizeDistance)
                {
                    message.GetComponent<Text>().text = "";
                    isVisualizedEnvironment = true;
                    VisualizeEnvironment();
                }
            }
        }
    }

    public void VisualizeEnvironment()
    {
        if (seaGO != null)
        {
            seaGO.SetActive(SystemExtension.isFastCPU());
        }

        if (waterLightGO != null)
        {
            waterLightGO.SetActive(SystemExtension.isFastCPU());
        }

        if (bubblesGO != null)
        {
            bubblesGO.SetActive(SystemExtension.isFastCPU());
        }

        if (fadeInGO != null)
        {
            fadeInGO.SetActive(true);
        }

        UnderWaterSoundThread = PlayUnderWaterSound();
        StartCoroutine(UnderWaterSoundThread);

        StartCoroutine(VisualizingEnvironment(2.5f));
    }

    IEnumerator VisualizingEnvironment(float delay)
    {
        yield return new WaitForSeconds(delay);
        ProGamer.SoundManager.Instance.SetBGBListAndPlay(new string[] { "dreamweaver" });
        if (fadeInGO != null)
        {
            fadeInGO.SetActive(false);
        }

        if (fadeOutGO != null)
        {
            fadeOutGO.SetActive(true);
        }

        GameSave.NumEnterARMode++;

        //enable blue layout and water filter
        gameObject.transform.Find("AR Camera").GetComponent<CameraFilterPack_Distortion_Dream2>().enabled = SystemExtension.isFastCPU();
        gameObject.transform.Find("AR Camera").GetComponent<CameraFilterPack_Color_RGB>().enabled = true;

        //replace Preview by Environment
        Environment = GenerateEnvironment(PreviewObject.transform.position, PreviewObject.transform.rotation);
        //Delete preview
        Destroy(PreviewObject);
    }

    public GameObject GenerateEnvironment(Vector3 position, Quaternion rotation)
    {
        AquariumData aquariumData = DataCenter.Instance.GetAquarium(GameSave.SelectedAquarium);
        GameObject tankModel = Instantiate<GameObject>(Resources.Load(GameSetting.ModelPrefabPath + aquariumData.Data.ARGround) as GameObject, position, rotation);

        //manually init
        FishTank tank = tankModel.GetComponentInChildren<FishTank>();
        tank.InitFishTank();

        int fishCount = tank.CreateFishes(aquariumData.DefaultCreature, aquariumData.DefaultSkin);
        int myFishCount = tank.CreateFishes(ImageDataManager.Instance.GetInAquariumImageDataList(aquariumData.AquariumId), 0.2f, Vector3.zero);
        fishCount += myFishCount;

        int fillUpCreaturesToCount = 50;
        CollectionData data;
        if (fillUpCreaturesToCount > fishCount)
        {
            if (myFishCount > 0)
            {
                tank.CreateRandomFishes(ImageDataManager.Instance.GetInAquariumImageDataList(aquariumData.AquariumId),
                    fillUpCreaturesToCount - fishCount, 0.3f, 0);
            }
        }

        if (DataCenter.Instance.CollectionDataDic.TryGetValue("AR", out data))
        {
            tank.CreateRandomFishes(new List<string>(data.DefaultCreature), new List<string>(data.DefaultSkin),
                fillUpCreaturesToCount, 0.4f, 0);
        }

        if (DataCenter.Instance.CollectionDataDic.TryGetValue("AR_PLUS", out data))
        {
            tank.CreateRandomFishes(new List<string>(data.DefaultCreature), new List<string>(data.DefaultSkin), 2, 2.5f, 0);
        }

        //set default size
        tankModel.transform.localScale = Vector3.one * aquariumData.Data.InitScale;
        tank.SetTankSize(tankModel.transform.localScale.x);

        return tankModel;
    }

    IEnumerator UnderWaterSoundThread;
    AdvanceAudioSource underWaterClip;

    IEnumerator PlayUnderWaterSound()
    {
        while (true)
        {
            underWaterClip = ProGamer.SoundManager.Instance.Play("underwater");
            yield return new WaitForSeconds(25);
        }
    }

    public void Back()
    {
        NativeHelper.GenerateImpactFeedback();
        if (UnderWaterSoundThread != null)
        {
            StopCoroutine(UnderWaterSoundThread);
            if (underWaterClip != null)
            {
                underWaterClip.Stop();
            }
        }

        SceneTransitHandler.LoadScene("Aquarium");
    }

    public void GestureInvoked(Gesture gesture)
    {
        switch (gesture)
        {
            case Gesture.ZoomIn:
                //Walkin to open
                //ARObjectsManager.Instance.VisualizeEnvironment();
                break;

            case Gesture.ZoomOut:
                SceneTransitHandler.LoadScene("Aquarium");
                break;
        }
    }
}