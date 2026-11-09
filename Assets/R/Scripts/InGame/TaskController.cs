using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProGamer;
using Newtonsoft.Json;

public enum TaskTarget
{
    Food,
    Score,
    PlayGame
}

public class TaskController : ProGamer.Singleton<TaskController>
{
    protected Dictionary<string, InGameTaskData> mTaskProgress;

    public Dictionary<string, InGameTaskData> TaskProgress
    {
        get
        {
            if (mTaskProgress == null)
            {
                if (!string.IsNullOrEmpty(GameSave.TaskProgress))
                {
                    mTaskProgress = JsonConvert.DeserializeObject<Dictionary<string, InGameTaskData>>(GameSave.TaskProgress);
                }
                else
                {
                    mTaskProgress = new Dictionary<string, InGameTaskData>();
                }
            }

            return mTaskProgress;
        }
    }

    public InGameTaskData GetTaskData(string character)
    {
#if EVENT_DEMO || UNLOCK_ALL
		return null;
#endif

        InGameTaskData data;

        if (TaskProgress.TryGetValue(character, out data))
        {
            if (data.IsFinished)
            {
                if (data.TaskStaticData.IsLastTask)
                {
                    Debug.Log("No more task for this character.");
                    return null;
                }
                else
                {
                    int next = data.TaskStaticData.TaskIndex + 1;
                    return GetTaskByIndex(character, next);
                }
            }
            else
            {
                return data;
            }
        }
        else
        {
            return GetTaskByIndex(character, 0);
        }
    }

    protected InGameTaskData GetTaskByIndex(string character, int taskIndex)
    {
        List<TaskData> taskList;
        if (DataCenter.Instance.TaskDataDic.TryGetValue(character, out taskList) && taskList.Count >= taskIndex)
        {
            TaskData d = taskList[taskIndex];

            return new InGameTaskData(d, 0);
        }
        else
        {
            Debug.LogError("Wrong Game Settings!");
            return null;
        }
    }


    public void SaveTask(InGameTaskData d)
    {
        TaskProgress[d.TaskStaticData.Character] = d;
        SaveTaskToDisk();
    }

    public void SaveTaskToDisk()
    {
        GameSave.TaskProgress = JsonConvert.SerializeObject(TaskProgress);
    }
}