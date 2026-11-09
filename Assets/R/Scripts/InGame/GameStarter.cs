using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStarter : MonoBehaviour
{
    [SerializeField] protected GameObject mainCharacter;

    [SerializeField] protected ModelContainer Model;

    // Use this for initialization
    void Awake()
    {
        CharacterUpgrade data = DataCenter.Instance.GetCharacterUpgradeData(GameSave.SelectedModel);

        Model.InitModelContainer(DataCenter.Instance.CharacterDataDic[GameSave.SelectedModel]);
        InGameData.Instance.InitGameData(data.GetHP(), 0, data.GetBonus(), 0, TaskController.Instance.GetTaskData(GameSave.SelectedModel));

        //Set
        string selectedSkin = GameSave.GetModelSkin(GameSave.SelectedModel);

        if (!string.IsNullOrEmpty(selectedSkin))
        {
            Texture2D t = ImageDataWriter.LoadImage(selectedSkin, true);
            if (t != null)
            {
                mainCharacter.GetComponentInChildren<Renderer>().material.SetTexture("_MainTex", t);
            }
        }
    }
}