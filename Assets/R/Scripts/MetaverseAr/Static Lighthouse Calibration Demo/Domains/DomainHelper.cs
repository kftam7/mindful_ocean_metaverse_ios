using System;
using System.Collections.Generic;
using System.Linq;
using Auki;
using Auki.ConjureKit;
using Auki.ConjureKit.Manna;
using Auki.Navigation;
using ProGamer;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Matterless
{
    public class DomainHelper
    {
        private bool _enterDomainViaCalibrate;
        public bool IsCalling => m_DomainAssetService.IsCalling;

        private float m_ScannedLighthouseSize;
        private LighthousePose m_LatestLighthousePose;
        private Transform m_ArSessionOriginTransform;
        private Action<string, string> _onHostingSuccess;
        private readonly IConjureKit m_ConjureKit;
        private readonly Manna m_Manna;
        private readonly DomainAssetService m_DomainAssetService;
        private readonly GameObject m_LighthouseScanEffect;
        private readonly GameObject m_UncalibratedLighthouseEffect;

        public DomainHelper(IConjureKit conjureKit, Manna manna, Transform arSessionOriginTransform, Action<string, string> onHostingSuccess)
        {
            m_ConjureKit = conjureKit;
            m_Manna = manna;
            m_ArSessionOriginTransform = arSessionOriginTransform;
            _onHostingSuccess = onHostingSuccess;
            RestService restService = new();
            m_DomainAssetService = new DomainAssetService(restService);

            m_Manna.SetOnBeforeCalibration(OnBeforeCalibration);
            m_Manna.SetStaticLighthousePoseSelector(PoseSelector);
            m_Manna.OnCalibrationSuccess += OnCalibrated;
            m_Manna.OnCalibrationFailed += OnCalibrationFailed;

            m_LighthouseScanEffect = Resources.Load<GameObject>("Prefabs/Domains/LighthouseScanEffect");
            m_UncalibratedLighthouseEffect = Resources.Load<GameObject>("Prefabs/Domains/UncalibratedLighthouseEffect");
        }

        private void OnCalibrationFailed(CalibrationFailureData obj)
        {
            if (m_UncalibratedLighthouseEffect && obj.Lighthouse != null)
            {
                SpawnEffectOnLighthouse(m_UncalibratedLighthouseEffect, obj.Pose.position, obj.Pose.rotation, obj.Lighthouse.PhysicalSize);
            }

            if (m_ExpectingBadLighthouse)
            {
                m_ExpectingBadLighthouse = false;
                return;
            }

            DomainError?.Invoke("scanning an uncalibrated lighthouse", null, false);
        }

        private void OnBeforeCalibration(Lighthouse lighthouse, Action<bool> continueAction)
        {
            //disable joining with dynamic lighthouses

            if (lighthouse.Type == Lighthouse.LighthouseType.Dynamic)
            {
                continueAction.Invoke(true);
                return;
            }

            m_ScannedLighthouseSize = lighthouse.PhysicalSize;
            continueAction.Invoke(true);
        }

        private bool m_DomainEntered;
        private bool m_DomainLoaded;
        private bool m_FakeDomain;

        public string DomainId { get; private set; }
        public string DomainName { get; private set; }

        public event Action<SerializedNavData, SerializedNavData> DomainAssetsLoaded;
        public event Action<string, string, bool> DomainEntered;
        public event Action DomainLeft;
        public event Action<string, RestService.ErrorResponse, bool> DomainError;

        //this is a workaround be cause we need to cancel the poseselector to restart scanning
        private readonly LighthousePose m_NullLighthouse = new();
        private bool m_ExpectingBadLighthouse;

        private void PoseSelector(StaticLighthouseData data, Action<LighthousePose> action)
        {
            Logger.Log($"{data.poses.Length} domains found.");

            m_LatestLighthousePose = null; // Reset first, then set again below with valid pose (if it has any)

            //no lighthouses with calibration
            List<LighthousePose> poseList = data.poses.Where(p => !p.IsEqualToIdentityPose()).ToList();
            if (poseList.Count == 0)
            {
                action.Invoke(m_NullLighthouse);
                return;
            }

            Logger.Log($"{poseList.Count} valid domains found.");

            //there is only one domain, use it
            if (poseList.Count == 1)
            {
                LighthousePose pose = poseList[0];

                //ignore it if we are in a domain and its not this one
                if (IsDomainFound && pose.domainId != DomainId)
                {
                    m_ExpectingBadLighthouse = true;
                    action.Invoke(m_NullLighthouse);
                    return;
                }

                //go (happy path)
                IsDomainFound = true;
                DomainId = pose.domainId;
                DomainName = pose.domainName;
                m_LatestLighthousePose = pose;
                action.Invoke(pose);
                if (!_enterDomainViaCalibrate)
                {
                    _onHostingSuccess?.Invoke(DomainId, DomainName);
                    _enterDomainViaCalibrate = true;
                }
                DomainEntered?.Invoke(DomainId, DomainName, action != null);
                return;
            }

            //we are in a domain and there are two options. Select one which we are already in
            if (IsDomainFound)
            {
                foreach (LighthousePose lighthousePose in poseList)
                {
                    if (lighthousePose.domainId == DomainId)
                    {
                        m_LatestLighthousePose = lighthousePose;
                        action.Invoke(lighthousePose);
                        if (!_enterDomainViaCalibrate)
                        {
                            _onHostingSuccess?.Invoke(lighthousePose.domainId, lighthousePose.domainName);
                            _enterDomainViaCalibrate = true;
                        }
                        return;
                    }
                }

                m_ExpectingBadLighthouse = true;
                action.Invoke(m_NullLighthouse);
                return;
            }

            _enterDomainViaCalibrate = true;
            ShowDomainListPopup(data.shortId, poseList, action, null);
        }

        private void ShowDomainListPopup(string shortId, List<LighthousePose> poseList, Action<LighthousePose> selectPose, Action customDomainCallback)
        {
            PopupManager.Instance.QuickShowPopup(PopupPrefab.SelectDomainPopup, true);
            var selectDomainPopup = (SelectDomainPopup)PopupManager.Instance.ActivePopup;
            //we are not in a domain and there are two options
            selectDomainPopup.Init(shortId, poseList, pose =>
            {
                if (pose == null)
                {
                    m_ExpectingBadLighthouse = true;
                    selectPose?.Invoke(m_NullLighthouse);
                    return;
                }

                if (IsDomainFound && pose.domainId != DomainId)
                {
                    m_ExpectingBadLighthouse = true;
                    selectPose?.Invoke(m_NullLighthouse);
                    return;
                }

                IsDomainFound = true;
                DomainId = pose.domainId;
                DomainName = pose.domainName;

                m_LatestLighthousePose = pose;
                selectPose?.Invoke(pose);

                Logger.Log($"Selected new domain: {DomainName} {DomainId}");

                DomainEntered?.Invoke(DomainId, DomainName, _enterDomainViaCalibrate);
            }, customDomainCallback);
        }

        private void OnCalibrated(Matrix4x4 poseMatrix)
        {
            Logger.Log("DomainHelper: On Calibrated Start!");
            if (m_ConjureKit.GetSession() == null)
                return;
            if (!IsDomainFound)
                return;

            if (m_DomainEntered)
                return;

            m_DomainEntered = true;

            if (m_LighthouseScanEffect != null && m_LatestLighthousePose != null)
            {
                SpawnEffectOnLighthouse(m_LighthouseScanEffect, m_LatestLighthousePose, m_ScannedLighthouseSize);
            }

            Logger.Log($"DomainHelper: On Calibrated Complete! DomainId: {DomainId}");
            LoadDomainAssets();
        }

        private void SpawnEffectOnLighthouse(GameObject scanEffectPrefab, LighthousePose pose, float size)
        {
            var lighthousePosition = new Vector3(pose.px, pose.py, -pose.pz);
            var lighthouseRotation = new Quaternion(-pose.rx, -pose.ry, pose.rz, pose.rw);

            SpawnEffectOnLighthouse(scanEffectPrefab, lighthousePosition, lighthouseRotation, size);
        }

        private void SpawnEffectOnLighthouse(GameObject scanEffectPrefab, Vector3 position, Quaternion rotation, float size)
        {
            var scanEffect = Object.Instantiate(scanEffectPrefab, m_ArSessionOriginTransform);
            scanEffect.transform.position = position;
            scanEffect.transform.rotation = rotation;

            if (size <= 0)
                size = 0.1f;

            scanEffect.transform.localScale = Vector3.one * size;

            Object.Destroy(scanEffect, 5f);
        }

        private void LoadDomainAssets()
        {
            if (m_DomainLoaded)
                return;

            m_DomainAssetService.GetNavMesh(DomainId, AssetType.navmesh, navMeshData =>
            {
                m_DomainAssetService.GetNavMesh(DomainId, AssetType.baked, bakedData =>
                {
                    DomainAssetsLoaded?.Invoke(navMeshData, bakedData);

                    m_DomainLoaded = true;
                },
                response => OnLoadError("loading baked navigation data", response, true));
            },
            response => OnLoadError("loading navigation data", response, true));
        }

        private void OnLoadError(string context, RestService.ErrorResponse error, bool isFatal)
        {
            DomainError?.Invoke(context, error, isFatal);
        }

        public bool IsDomainFound { get; private set; }

        public void SessionLost()
        {
            IsDomainFound = false;
            m_DomainLoaded = false;
            m_DomainEntered = false;
            m_FakeDomain = false;
            DomainId = null;
            DomainName = null;
            DomainLeft?.Invoke();
        }

        public void EditorDomain(string domainId)
        {
            IsDomainFound = true;
            DomainId = domainId;
            DomainName = "Editor";
            OnCalibrated(new Matrix4x4());
        }

        public void SelectDomainDebug(string shortId, LighthousePose[] poseArray, Action<LighthousePose> selectPose, Action customDomainCallback)
        {
            _enterDomainViaCalibrate = false;
            var cleanedPoseList = poseArray.Where(p => !p.IsEqualToIdentityPose()).ToList();
            if (cleanedPoseList.Count == 1)
            {
                DomainEntered?.Invoke(cleanedPoseList[0].domainId, cleanedPoseList[0].domainName, true);
                return;
            }
            ShowDomainListPopup(shortId, cleanedPoseList, selectPose, customDomainCallback);
        }
    }
}