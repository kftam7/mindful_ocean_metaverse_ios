using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishPrinterTutorial : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.Find("PrinterTutorial"))
        {
            GameSave.IsPrinterTutorialFinished = true;
        }
    }

    // Update is called once per frame
    void Update() { }
}