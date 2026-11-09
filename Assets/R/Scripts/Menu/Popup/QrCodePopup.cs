using System.Collections;
using Auki.ConjureKit;
using ProGamer;
using UnityEngine;
using UnityEngine.UI;

public class QrCodePopup : SimplePopup
{
    [SerializeField] private RawImage qrCodeImage;
    [SerializeField] private InputField spaceIdInput;
    [SerializeField] private Text SpaceIdText;
    [SerializeField] private Text InvalidSpaceIdText;
    private bool isStarted;

    private void Start()
    {
        StartCoroutine(ShowQrCode());
        isStarted = true;
    }

    private void OnEnable()
    {
        InvalidSpaceIdText.gameObject.SetActive(false);
        if (isStarted)
        {
            StartCoroutine(ShowQrCode());
        }

        AukiSdkWrapper.Instance.JoinedEvent += OnJoined;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        AukiSdkWrapper.Instance.JoinedEvent -= OnJoined;
    }

    private IEnumerator ShowQrCode()
    {
        while (!AukiSdkWrapper.Instance.HasJoined)
        {
            yield return null;
        }

        var cam = GetComponentInParent<Canvas>().worldCamera;
        // AukiSdkWrapper.Instance.ShowQrCode(true);
        Vector2 position = RectTransformUtility.WorldToScreenPoint(cam, qrCodeImage.transform.position);
        position.y = Screen.height - position.y;
        AukiSdkWrapper.Instance.SetQrCodePosition(position);
        AukiSdkWrapper.Instance.ShowQrCode(true);
        SpaceIdText.text = AukiSdkWrapper.Instance.SessionId;
        // Texture2D texture = AukiSdkWrapper.Instance.Texture;
        // Color[] pixels = texture.GetPixels();
        // var color = new Color(0 / 255f, 158f / 255f, 205f / 255f);
        // for (var i = 0; i < pixels.Length; i++)
        // {
        //     if (pixels[i] != Color.white)
        //     {
        //         pixels[i] = color;
        //     }
        // }
        //
        // texture.SetPixels(pixels);
        // texture.Apply();
        // qrCodeImage.texture = texture;
    }

    public void OnPressCancel()
    {
        Close();
        AukiSdkWrapper.Instance.ShowQrCode(false);
    }

    public void OnPressScan()
    {
        Close();
        // ARMetaverseManager.Instance.EnableOceanEffect(false); // Tested and does not affect scanning
        AukiSdkWrapper.Instance.ShowQrCode(false);
        ARAquariumUIController.Instance.ShowScanner();
        DataCenter.Instance.JoinedVia = AnalyticsEnums.ValueQrCode;
    }

    public void OnPressJoin()
    {
        if (string.IsNullOrEmpty(spaceIdInput.text)) return;
        DataCenter.Instance.JoinedVia = AnalyticsEnums.ValueSpaceId;
        AukiSdkWrapper.Instance.CheckIfSessionExists(spaceIdInput.text, result =>
        {
            if (result)
            {
                InvalidSpaceIdText.gameObject.SetActive(false);
                AukiSdkWrapper.Instance.Connect(spaceIdInput.text, _ => { Close(); });
            }
            else
            {
                InvalidSpaceIdText.gameObject.SetActive(true);
            }
        });
    }

    private void OnJoined(Session obj)
    {
        Close();
        AukiSdkWrapper.Instance.ShowQrCode(false);
    }
}