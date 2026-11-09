using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProGamer;

public class ARModel
{
    public string Id
    {
        get { return Data.AquariumId; }
    }

    public AquariumData Data;

    public GameObject Model;
    public GameObject Preview;

    public GameObject RotationIndicator;
    public GameObject TranslationIndicator;

    public ARModel(GameObject model, GameObject preview)
    {
        this.Model = model;
        this.Preview = preview;
    }

    public ARModel(string model, string preview, Transform container)
    {
        Debug.Log("Load : " + model);
        this.Model = GameObject.Instantiate<GameObject>(Resources.Load(model) as GameObject);
        this.Model.name = "Model";
        Debug.Log("Load : " + preview);
        this.Preview = GameObject.Instantiate<GameObject>(Resources.Load(preview) as GameObject);
        this.Preview.name = "Preview";

        if (container != null)
        {
            this.Model.transform.SetParent(container);
            this.Preview.transform.SetParent(container);
        }

        SetIndicator();
    }

    public void SetIndicator()
    {
        GameObject tGo = GameObject.Instantiate<GameObject>(Resources.Load("Prefabs/TranslationIndicator") as GameObject);
        tGo.transform.SetParent(this.Model.transform);
        tGo.transform.localScale = Vector3.one;
        tGo.transform.rotation = Quaternion.identity;

        this.TranslationIndicator = tGo;

        GameObject rGo = GameObject.Instantiate<GameObject>(Resources.Load("Prefabs/RotationIndicator") as GameObject);
        rGo.transform.SetParent(this.Model.transform);
        rGo.transform.localScale = Vector3.one;
        rGo.transform.rotation = Quaternion.identity;

        this.RotationIndicator = rGo;
    }
}