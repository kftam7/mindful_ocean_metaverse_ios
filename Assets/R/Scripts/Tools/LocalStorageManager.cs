using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class LocalStorageManager : ProGamer.Singleton<LocalStorageManager>
{
    protected List<QuestionnaireData> mQuestionnaireDataList;

    public List<QuestionnaireData> QuestionnaireDataList
    {
        get
        {
            if (mQuestionnaireDataList == null)
            {
                if (!string.IsNullOrEmpty(GameSave.CachedQuestionnaireResultList))
                {
                    mQuestionnaireDataList = JsonConvert.DeserializeObject<List<QuestionnaireData>>(GameSave.CachedQuestionnaireResultList);
                }
                else
                {
                    mQuestionnaireDataList = new List<QuestionnaireData>();
                }
            }

            return mQuestionnaireDataList;
        }
    }

    protected QuestionnaireData cachedData;

    ///Add data and try to upload
    public void AddData(QuestionnaireData questionnaire)
    {
        QuestionnaireDataList.Add(questionnaire);

        Save();
        TryUploadJson(questionnaire);
    }

    protected void TryUploadJson(QuestionnaireData questionnaire)
    {
        cachedData = questionnaire;

        // PluginWrapper.Instance.UploadTextFile(string.Format("{0}.txt", questionnaire.Id), JsonConvert.SerializeObject(questionnaire), null, PluginWrapper.QuestionnairePath, FileUploadCallback);
    }

    protected void FileUploadCallback(bool isSuccess)
    {
        if (isSuccess)
        {
            QuestionnaireDataList.Remove(cachedData);
            Save();

            //Try to upload other saved questionnaire
            if (QuestionnaireDataList.Count > 0)
            {
                cachedData = QuestionnaireDataList[0];
                if (cachedData != null)
                {
                    TryUploadJson(cachedData);
                }
            }
        }
    }

    public void Save()
    {
        GameSave.CachedQuestionnaireResultList = JsonConvert.SerializeObject(QuestionnaireDataList);
    }
}