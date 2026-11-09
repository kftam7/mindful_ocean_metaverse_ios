using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class PurchasePopupItem : MonoBehaviour
{
    public Text Title;
    public Text Price;
    public Text OriginalPrice;
    public Text Description;
    public string PackageId;

    [HideInInspector]
    public string OriginalPackageId;

    private PackageData data;

    private void Start()
    {
        DataCenter.Instance.IAPDataDic.TryGetValue(PackageId, out data);
        if (data == null)
        {
            Debug.Log($"[{GetType().Name}] PackageData is null!");
            return;
        }

        Product product = Purchaser.Instance.GetLocalizedProduct(data.PackageId);
        Price.text = $"{(product != null ? product.metadata.localizedPriceString : data.Price)}";

        if (!string.IsNullOrEmpty(OriginalPackageId))
        {
            PackageData originalData;
            DataCenter.Instance.IAPDataDic.TryGetValue(OriginalPackageId, out originalData);
            if (originalData == null)
            {
                Debug.Log($"[{GetType().Name}] Original PackageData is null!");
                return;
            }

            Product originalProduct = Purchaser.Instance.GetLocalizedProduct(originalData.PackageId);
            OriginalPrice.text = $"{(originalProduct != null ? originalProduct.metadata.localizedPriceString : originalData.Price)}";
        }
    }

    private void OnEnable()
    {
        GetComponentInChildren<Button>().onClick.AddListener(Purchase);
    }

    private void OnDisable()
    {
        GetComponentInChildren<Button>().onClick.RemoveListener(Purchase);
    }

    private void Purchase()
    {
        if (data != null)
        {
            NativeHelper.GenerateImpactFeedback();
            Purchaser.Instance.Purchase(data.PackageId);
        }
    }
}