using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using Auki.ConjureKit;
using Auki.ConjureKit.Manna;
using Auki.ConjureKit.Vikja;
using Auki.Integration.ARFoundation;
using Auki.Util;
using DomainSessionDemo;
using Matterless;
using Matterless.ECS;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.ARFoundation;
using Lighthouse = Auki.ConjureKit.Manna.Lighthouse;
using Dds;
using ProGamer;

public static class EntityActionNameEnum
{
    public const string CreateFish = "CreateFish";
    public const string CreateHeart = "CreateHeart";
    public const string CreateTorus = "CreateTorus";
    public const string CreateBowl = "CreateBowl";
    public const string DeleteEntity = "DeleteEntity";
    public const string StartScroll = "StartScroll";
    public const string StopScroll = "StopScroll";
}

public class AukiSdkWrapper : SceneInstance<AukiSdkWrapper>
{
    #region Serialize Fields

    [SerializeField] private ARCameraManager arCameraManager;
    [SerializeField] private string appKey = "2e2f5aa8-5869-4c5a-9915-40772173d704";
    [SerializeField] private string appSecret = "758323c5-dc7f-42e7-b7b3-1cea5a45131aac95e802-52e7-4993-8ce7-7aeb6f67ea7a";

    [SerializeField] private Transform arSessionOrigin;
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private Material navMeshMaterial;
    [SerializeField] private LineRenderer lineRenderer;

    #endregion

    #region Private Fileds

    private CameraFrameProvider mannaFrameFeeder;
    private DdsHelper _ddsHelper;
    // private StaticLighthouseCalibrationController staticLighthouseCalibrationController;

    private ConjureKit conjureKit;
    private Manna manna;
    private Vikja vikja;
    private Texture2D videoTexture;
    private Session Session => conjureKit.GetSession();
    private bool enableScanner;

    private DomainHelper _domainHelper;
    private MeshSystem _meshSystem;
    private NavMeshPath _path;
    private Action _staticLighthousePoseScanSuccess;
    [SerializeField] private HostHandler _hostHandler = new();
    private readonly DomainEditorDebug _domainEditorDebug = new();

    //private const string DomainId = "834a1d12-b806-4b00-8357-4c7021388e6b";
    private const string DomainId = "b59d4e66-becd-42f6-ae9d-3d20791e60be";

    #endregion

    #region Public Fields

    public string SessionId => Session.Id;
    public uint MyPeerId => Session.ParticipantId;
    public List<Participant> Peers => Session.GetParticipants();
    public bool HasJoined { get; private set; }
    public bool IsHost => _hostHandler.IsHost;
    public bool IsDisconnecting { get; private set; }
    public bool IsInDomain => !string.IsNullOrEmpty(_currentDomainId);
    public State State => conjureKit.GetState();
    public Camera ArCamera { get; private set; }

    public Texture2D Texture
    {
        get
        {
            Type type = manna.GetType();
            FieldInfo fieldInfo = type.GetField("_lighthouseToShow", BindingFlags.Instance | BindingFlags.NonPublic);
            Lighthouse lighthouse = fieldInfo.GetValue(manna) as Lighthouse;
            return lighthouse.Texture;
        }
    }

    #endregion

    #region Events

    public Action<Lighthouse, Pose, bool> LighthouseTrackedEvent;
    private string _currentDomainId;
    private bool _isDestroyInvoked;

    public event Action<Session> JoinedEvent;
    public event Action LeftEvent;
    public event Action<Participant> PeerJoinedEvent;
    public event Action<uint> PeerLeftEvent;
    public event Action<Entity> EntityAddedEvent;
    public event Action<uint> EntityDeletedEvent;
    public event Action<Entity> EntityUpdatePoseEvent;
    public event Action<State> StateChangedEvent;
    public event Action<EntityAction> EntityActionEvent;
    public event Action<string, string> OnHostingDomainSession;
    public event Action<string, string> OnJoinedDomainSession;
    public event Action OnDomainAssetsAvailable;
    public event Action<bool> IsLoading;

    #endregion

    #region Unity Methods

    private void Start()
    {
        AukiDebug.logLevel = AukiDebug.LogLevel.TRACE;

        ArCamera = arCameraManager.GetComponent<Camera>();
        conjureKit = new ConjureKit(ArCamera.transform, appKey, appSecret, AukiDebug.LogLevel.ERROR);
        manna = new Manna(conjureKit);
        vikja = new Vikja(conjureKit);
        conjureKit.OnJoined += OnJoined;
        conjureKit.OnLeft += OnLeft;
        conjureKit.OnParticipantJoined += OnParticipantJoined;
        conjureKit.OnParticipantLeft += OnParticipantLeft;
        conjureKit.OnEntityAdded += OnEntityAdded;
        conjureKit.OnEntityDeleted += OnEntityDeleted;
        conjureKit.OnEntityUpdatePose += OnEntityUpdatePose;
        vikja.OnEntityAction += OnEntityAction;
        manna.OnLighthouseTracked += OnLighthouseTracked;
        manna.VibrateOnReadPose = true;
        manna.VibrateOnReportPose = true;

        // Attach a default GPU frame feeder which will pass the GPU image from ARFoundation to Manna when enabled
        mannaFrameFeeder = CameraFrameProvider.GetOrCreateComponent();
        mannaFrameFeeder.OnNewFrameReady += frame => manna.ProcessVideoFrameTexture(frame.Texture, frame.ARProjectionMatrix, frame.ARWorldToCameraMatrix);
        // Domain and navigation
        _domainHelper = new DomainHelper(conjureKit, manna, arSessionOrigin, ScanStaticLighthousePose);
        _ddsHelper = new DdsHelper(appKey, appSecret, conjureKit);
        _meshSystem = new MeshSystem(navMeshSurface, _domainHelper, navMeshMaterial);
        _path = new NavMeshPath();
        _domainHelper.DomainEntered += OnDomainEntered;
        _domainHelper.DomainLeft += OnDomainLeft;
        _domainHelper.DomainAssetsLoaded += OnDomainAssetsLoaded;
        Logger.Log($"[{GetType().Name}] connecting to auki...");
        EnableScanner(false);
        Connect(session =>
        {
            EnableScanner(false);
        });
    }

    private void ScanStaticLighthousePose(string domainId, string domainName)
    {
        _staticLighthousePoseScanSuccess?.Invoke();
    }

    private void OnDomainEntered(string domainId, string domainName, bool calibrated)
    {
        if (_isDestroyInvoked) return;
        
        ARAquariumUIController.Instance.EnableAdventureButton(false);

        //If Single Player built in domain selected, no need to host it, as we don't want to put different person in a same session in this mode
        if (domainId.Equals(ARMetaverseManager.Instance.GetSinglePlayerDomainId()))
        {
            _currentDomainId = domainId;
            OnHostingDomain(calibrated, domainId, domainName);
            return;
        }

        // Notify the Auki domain backend about scanning into the domain.
        // If there is no current session for this domain, associate the current session with it
        // If there is a session for this domain, join it (another participant is already hosting a session for that domain)
        _ddsHelper.DidSelectDomain(domainId,
        () =>
        {
            OnHostingDomain(calibrated, domainId, domainName);
        }, sessionId =>
        {
            CheckIfSessionExists(sessionId, sessionExists =>
            {
                if (sessionExists)
                {
                    conjureKit.OnStateChanged += StateChange;
                    conjureKit.Disconnect();

                    void StateChange(State state)
                    {
                        if (state != State.Disconnected) return;

                        conjureKit.OnJoined += OnJoinedSession;

                        conjureKit.Connect(sessionId, session =>
                        {
                            Logger.Log($"[DDS] Found hosted session, joining {sessionId} {domainName}.");
                            conjureKit.NotifyARCameraCalibrated();
                            _ddsHelper.DidJoinSession(session);
                            conjureKit.OnStateChanged -= StateChange;
                        },
                        err => Logger.LogError(err));
                    }

                    //broadcast joined domain session only after successfully joined the new session
                    void OnJoinedSession(Session session)
                    {
                        _currentDomainId = domainId;

                        _staticLighthousePoseScanSuccess = () =>
                        {
                            if (conjureKit.GetSession().GetParticipantCount() < 2)
                                OnHostingDomainSession?.Invoke(domainId, domainName);
                            else
                                OnJoinedDomainSession?.Invoke(domainId, domainName);
                                
                            conjureKit.OnJoined -= OnJoinedSession;
                            _staticLighthousePoseScanSuccess = null;
                        };

                        if (!calibrated)
                            ARAquariumUIController.Instance.ShowScannerForDomain(() =>
                            {
                                Logger.Log($"[DDS] Skipped scanning, Joining {domainName}.");
                                _staticLighthousePoseScanSuccess?.Invoke();
                            }, LeaveDomain);
                        else
                        {
                            _staticLighthousePoseScanSuccess?.Invoke();
                        }
                    }
                }
                else
                {
                    //session might not exist anymore, eventhough DDS Helper still reports a session back, 
                    // if that the case, then just force host the domain to the current session
                    _ddsHelper.DidSelectDomain(
                        domainId,
                        () =>
                        {
                            Logger.Log($"[DDS] Found orphaned session, force hosting {domainName}.");
                            OnHostingDomain(calibrated, domainId, domainName);
                        }, null);
                }
            });
        });
    }

    private void OnHostingDomain(bool calibrated, string domainId, string domainName)
    {
        _currentDomainId = domainId;
        
        _staticLighthousePoseScanSuccess = () =>
        {
            OnHostingDomainSession?.Invoke(domainId, domainName);
            _staticLighthousePoseScanSuccess = null;
        };
        
        if (!calibrated)
            ARAquariumUIController.Instance.ShowScannerForDomain(() =>
            {
                Logger.Log($"[DDS] Skipped scanning, Hosting {domainName}.");
                _staticLighthousePoseScanSuccess?.Invoke();
            }, LeaveDomain);
        else
        {
            _staticLighthousePoseScanSuccess?.Invoke();
        }
    }

    private void OnDomainLeft()
    {
        ARAquariumUIController.Instance.EnableAdventureButton(true);
        _currentDomainId = string.Empty;
    }

    private void OnDomainAssetsLoaded(SerializedNavData navMesh, SerializedNavData bakedMesh)
    {
        _meshSystem.Load(navMesh, bakedMesh);
        StartCoroutine(InvokeDomainAssetsAvailable());
    }

    public void LeaveDomain()
    {
        if (!IsInDomain) return;
        _domainHelper.SessionLost();
        _ddsHelper.LeaveDomain();
    }

    public IEnumerator InvokeDomainAssetsAvailable()
    {
        yield return null;

        OnDomainAssetsAvailable?.Invoke();
    }

    public Vector3[] CalculatePath(Vector3 start, Vector3 end)
    {
        var startOnFloor = new Vector3(start.x, 0, start.z);
        var endOnFloor = new Vector3(end.x, 0, end.z);
        var pathOnTheFloor = CalculatePathOnFloor(startOnFloor, endOnFloor);

        pathOnTheFloor[0].y = start.y;

        for (int i = 1; i < pathOnTheFloor.Length; i++)
        {
            pathOnTheFloor[i].y = end.y;
        }

        Logger.Log($"**** {pathOnTheFloor.Length}");

        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = pathOnTheFloor.Length;
        lineRenderer.SetPositions(pathOnTheFloor);

        foreach (var point in pathOnTheFloor)
        {
            Logger.Log(point.ToString());
        }

        return pathOnTheFloor;
    }

    private Vector3[] CalculatePathOnFloor(Vector3 start, Vector3 end)
    {
        if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, _path))
        {
            //Debug.Log($"natural path, {m_Path.corners.Length} corners");
            return _path.corners;
        }

        //if we didnt find a valid path, sample each side to get the closes point to the nav mesh, and try again.
        if (NavMesh.SamplePosition(start, out NavMeshHit foundPosition, 100f, NavMesh.AllAreas)
            && NavMesh.SamplePosition(end, out NavMeshHit foundPosition2, 100f, NavMesh.AllAreas)
            && NavMesh.CalculatePath(foundPosition.position, foundPosition2.position, NavMesh.AllAreas, _path))
        {
            start.y = _path.corners.First().y;
            end.y = _path.corners.Last().y;

            int originalLength = _path.corners.Length;

            //Debug.Log($"sampled path, {m_Path.corners.Length} corners");
            //this gives the path but from the sampled positions, so we manually add the off-nav positions of the original positions

            int length = originalLength + 2;

            Vector3[] fullPositions = new Vector3[length];
            fullPositions[0] = start;
            Array.Copy(_path.corners, 0, fullPositions, 1, originalLength);
            fullPositions[length - 1] = end;

            //fullPositions = RemoveClosePoints(fullPositions, 0.05f).ToArray();

            return fullPositions;
        }

        //Debug.Log($"cannot find path, go A to B");
        return new[] { new Vector3(start.x, end.y, start.z), end };
    }

    private void Update()
    {
        _ddsHelper.Update();

#if UNITY_EDITOR
        EditorUpdate();
#endif
    }

#if UNITY_EDITOR
    private void EditorUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ShowDomains("GL2G5D65Q8M");
        }
    }
#endif

    private void OnDestroy()
    {
        conjureKit.OnJoined -= OnJoined;
        conjureKit.OnLeft -= OnLeft;
        conjureKit.OnParticipantJoined -= OnParticipantJoined;
        conjureKit.OnParticipantLeft -= OnParticipantLeft;
        conjureKit.OnEntityAdded -= OnEntityAdded;
        conjureKit.OnEntityDeleted -= OnEntityDeleted;
        conjureKit.OnEntityUpdatePose -= OnEntityUpdatePose;
        vikja.OnEntityAction -= OnEntityAction;
        manna.OnLighthouseTracked -= OnLighthouseTracked;

        conjureKit?.Disconnect();
        _isDestroyInvoked = true;
    }

    #endregion

    #region Public Methods
    public void ShowDomains(string qrShortId)
    {
        IsLoading?.Invoke(true);
        _domainEditorDebug.MockScan(qrShortId, conjureKit, data =>
        {
            foreach (var p in data.poses)
            {
                Logger.Log($"Poses: {p.domainName} {p.domainId} {p.IsEqualToIdentityPose()}");
            }
            _domainHelper.SelectDomainDebug(qrShortId, data.poses, null, ScanCustomDomain);
            IsLoading?.Invoke(false);
        }, error =>
        {
            IsLoading?.Invoke(false);
            PopupManager.Instance.QuickShowPopup(PopupPrefab.HintPopup, true);
            var popup = PopupManager.Instance.ActivePopup as HintPopup;
            popup.SetTextKeyTop("txt_connection_fail");
            popup.SetDelayClose(3);
        });
    }

    public void Connect(Action<Session> onComplete = null, Action<string> onFailed = null)
    {
        Logger.Log($"[{GetType().Name}] Connect");
        conjureKit.Connect(onComplete, onFailed);
    }

    public void Connect(string spaceId, Action<Session> onComplete = null, Action<string> onFailed = null)
        => conjureKit.Connect(spaceId, onComplete, onFailed);

    public void Disconnect()
    {
        Logger.Log($"[{GetType().Name}] Disconnect");
        if (HasJoined)
        {
            Logger.Log($"[{GetType().Name}] do disconnect");
            IsDisconnecting = true;
            _domainHelper.SessionLost();
            conjureKit.Disconnect(); // will send many events here
            IsDisconnecting = false;
        }
    }

    public void CheckIfSessionExists(string sessionId, Action<bool> callback)
    {
        var conjureKitTemp = new ConjureKit(ArCamera.transform, appKey, appSecret, AukiDebug.LogLevel.ERROR);
        conjureKitTemp.Connect(sessionId, _ => OnResult(true), _ => OnResult(false));

        void OnResult(bool success)
        {
            callback(success);
            // Destroy the bridge game object created by the temporary ConjureKit,
            // Disconnect will happen automatically
            Destroy(conjureKitTemp.GetConjureKitBridge().gameObject);
        }
    }

    public bool HasPeer(uint peerId)
    {
        if (!HasJoined) return false;
        return Peers.Any(peer => peer.Id == peerId);
    }

    public Entity GetEntityById(uint entityId) => Session?.GetEntity(entityId);
    public Pose GetEntityPose(uint entityId) => Session.GetEntityPose(entityId);
    public Pose GetEntityPose(Entity entity) => Session.GetEntityPose(entity);

    public void ShowQrCode(bool value) => manna.SetLighthouseVisible(value);

    // When disabled frameFeeder won't send frames to Manna
    public void EnableScanner(bool value) => mannaFrameFeeder.enabled = value;

    public void SetQrCodePosition(Vector2 position) => manna.SetLighthousePosition(position);

    public void RequestEntityAction(uint entityId, string actionName, byte[] data, Action<EntityAction> onComplete = null, Action<string> onError = null) =>
        vikja.RequestAction(entityId, actionName, data, onComplete, onError);

    public void AddFishEntity(Pose pose, bool persistent, string ownName, string imageUrl, Action<Entity> onComplete = null)
    {
        Logger.Log($"[{GetType().Name}] AddFishEntity: start");
        Session.AddEntity(pose, persistent, entity =>
        {
            Logger.Log($"[{GetType().Name}] AddFishEntity: entity added: " + $"{entity.Id}");
            RequestEntityAction(entity.Id, EntityActionNameEnum.CreateFish,
                DataConverter.AddFishEntityToBytes(ownName, imageUrl, pose.position),
                action =>
                {
                    Logger.Log($"[{GetType().Name}] AddFishEntity: action requested: {action.Name}: " +
                              $"\nentityId{entity.Id}  " +
                              $"\nownName: {ownName}" +
                              $"\nimageUrl: {imageUrl}");
                    onComplete?.Invoke(entity);
                });
        }, null);
    }

    public void AddMagicEntity(string userName, Pose pose, MagicObjectTypeEnum type, bool persistent, Action<Entity> onComplete = null)
    {
        Logger.Log($"[{GetType().Name}] AddMagicEntity: start");
        string actionName = type switch
        {
            MagicObjectTypeEnum.Heart => EntityActionNameEnum.CreateHeart,
            MagicObjectTypeEnum.Torus => EntityActionNameEnum.CreateTorus,
            MagicObjectTypeEnum.Bowl => EntityActionNameEnum.CreateBowl,
            _ => EntityActionNameEnum.CreateHeart
        };
        Session.AddEntity(pose, persistent, entity =>
        {
            Logger.Log($"[{GetType().Name}] AddMagicEntity: entity added: " + $"{entity.Id}");
            RequestEntityAction(entity.Id, actionName, DataConverter.AddMagicEntityToBytes(userName, pose.position),
                action =>
                {
                    Logger.Log($"[{GetType().Name}] AddMagicEntity: action requested: {action.Name}");
                    onComplete?.Invoke(entity);
                });
        }, null);
    }

    public void RequestDeleteEntityAction(uint entityId)
    {
        Logger.Log($"[{GetType().Name}] RequestDeleteEntityAction: {entityId}");
        RequestEntityAction(entityId, EntityActionNameEnum.DeleteEntity, Array.Empty<byte>(),
            action =>
            {
                Logger.Log($"[{GetType().Name}] RequestDeleteEntityAction: action requested: " + $"{action.Name}");
            });
    }

    public void DeleteEntity(uint entityId, Action<uint> onComplete = null)
    {
        Session?.DeleteEntity(entityId, () =>
        {
            onComplete?.Invoke(entityId);
        });
    }

    public void UpdateEntityPose(uint entityId, Pose pose)
    {
        Session.SetEntityPose(entityId, pose);
    }

    #endregion

    #region Event Callbacks

    private void OnLighthouseTracked(Lighthouse lighthouse, Pose pose, bool isCloseEnough)
    {
        Logger.Log($"[{GetType().Name}] OnLighthouseTracked");
        LighthouseTrackedEvent?.Invoke(lighthouse, pose, isCloseEnough);
    }

    private void OnJoined(Session session)
    {
        Logger.Log($"[{GetType().Name}] OnJoined: {session.Id}");
        HasJoined = true;
        _hostHandler.Setup(session);
        if (session.GetParticipantCount() == 1)
            _hostHandler.TakeHostRole(session.ParticipantId);
        //EnableScanner(false);
        JoinedEvent?.Invoke(session);
    }

    private void OnLeft(Session session)
    {
        Logger.Log($"[{GetType().Name}] OnLeft");
        // staticLighthouseCalibrationController.OnLeavingSession();
        HasJoined = false;
        _hostHandler.CleanUp();
        LeftEvent?.Invoke();
    }

    private void OnParticipantJoined(Participant participant)
    {
        Logger.Log($"[{GetType().Name}] OnParticipantJoined: {participant.Id}");
        PeerJoinedEvent?.Invoke(participant);
    }

    private void OnParticipantLeft(uint participantId)
    {
        Logger.Log($"[{GetType().Name}] OnParticipantLeft: {participantId}");
        _hostHandler.DecideNewHost(participantId);
        PeerLeftEvent?.Invoke(participantId);
    }

    private void OnEntityAdded(Entity entity)
    {
        Logger.Log($"[{GetType().Name}] OnEntityAdded: {entity.Id}");
        EntityAddedEvent?.Invoke(entity);
    }

    private void OnEntityDeleted(uint entityId)
    {
        Logger.Log($"[{GetType().Name}] OnEntityDeleted: {entityId}");
        EntityDeletedEvent?.Invoke(entityId);
    }

    private void OnEntityUpdatePose(Entity entity)
    {
        EntityUpdatePoseEvent?.Invoke(entity);
    }

    private void OnEntityAction(EntityAction action)
    {
        Logger.Log($"[{GetType().Name}] OnEntityAction: {action.Name}");
        EntityActionEvent?.Invoke(action);
    }

    #endregion

    #region Private Methods
    private void ScanCustomDomain()
    {
        ARAquariumUIController.Instance.ShowScanner();
    }
    #endregion
}