using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using System;

public class PackageData
{
    public string Id { get; set; }
    public string AndroidPackage { get; set; }
    public string iOSPackage { get; set; }

    public string PackageType
    {
        set
        {
            try
            {
                PackageProductType = (ProductType)Enum.Parse(typeof(ProductType), value, true);
            }
            catch
            {
                PackageProductType = ProductType.Consumable;
            }
        }
    }

    public string PackageId
    {
        get
        {
#if UNITY_IOS
            return iOSPackage;
#else
			return AndroidPackage;
#endif
        }
    }

    public ProductType PackageProductType { get; private set; }
    public string Price { get; set; }
    public string PackagePreview { get; set; }

    public string Description { get; set; }
    public string PackageName { get; set; }
    public List<string> CombinationPackage { get; set; }
    public string IncludedBy { get; set; }
}