using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FishTankUI : MonoBehaviour
{
    public Text FishCountText;

    public void BackToMain()
    {
        SceneTransitHandler.BackMainMenu();
    }

    public void ToggleARCameraMode()
    {
        ARFishTankController.Instance.ToggleARCameraMode();
    }
}