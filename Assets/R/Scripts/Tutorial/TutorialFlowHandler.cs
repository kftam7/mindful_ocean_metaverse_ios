using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//Can start multiple tutorial at the same time
public class TutorialFlowHandler : MonoBehaviour
{
    public enum TutorialFlowStatus
    {
        Empty,
        Ready,
        Progressing,
        Waiting,
        End,
        Terminated
    }

    public string TutorialName
    {
        get { return tutorialName; }
    }

    public TutorialFlowStatus CurrentStatus
    {
        get { return currentStatus; }
    }

    public int CurrentStep
    {
        get { return currentStep; }
    }

    public int StepCount
    {
        get { return stepList.Count; }
    }

    protected string tutorialName;
    protected List<TutorialStep> stepList;
    protected int currentStep = -1;
    protected TutorialFlowStatus currentStatus = TutorialFlowStatus.Empty;

    protected List<GameObject> cachedPrefab = new List<GameObject>();

    public void InitTutorial(string tName, List<TutorialStep> steps)
    {
        this.tutorialName = tName;
        this.stepList = steps;

        SceneManager.sceneLoaded += OnSceneLoaded;
        currentStatus = TutorialFlowStatus.Ready;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void StartTutorial(bool startByTrigger)
    {
        if (startByTrigger)
        {
            currentStep = -1;
            currentStatus = TutorialFlowStatus.Waiting;
        }
        else
        {
            currentStep = 0;
            currentStatus = TutorialFlowStatus.Progressing;
            ExecuteStep();
        }
    }

    protected void NextStep()
    {
        currentStep++;
        if (currentStep >= StepCount)
        {
            EndTutorial();
        }
        else
        {
            ExecuteStep();
        }
    }

    protected void ExecuteStep()
    {
        if (currentStep < StepCount)
        {
            TutorialStep step = stepList[currentStep];
            Debug.Log(string.Format("Start step: {0} - {1}", currentStep, ProGamer.Localization.Get(step.ActionDescription)));
            StartCoroutine(ExecuteStep(step));

            if (step.Step == 13) // tutorial end
            {
                GameSave.IsTutorialFinished = true;
            }
        }
        else
        {
            currentStatus = TutorialFlowStatus.Terminated;
            throw new System.IndexOutOfRangeException(string.Format("Step {0} doesn't exist", currentStep));
        }
    }

    IEnumerator ExecuteStep(TutorialStep step)
    {
        yield return new WaitForSeconds(step.DelayStart);

        //Log tutorial
        if (step.IsLogProgress)
        {
            AnalyticsLogCenter.LogEvent("has_entered_tutorial_step".AddCaption(tutorialName), new Dictionary<string, object>()
            {
                { "step_description".AddCaption(tutorialName), step.ActionDescription },
                { "step_number".AddCaption(tutorialName), step.Step }
            });
        }

        if (!string.IsNullOrEmpty(step.Prefab) && step.Prefab != "na")
        {
            GameObject prefab = GeneralTools.Instantiate(TutorialController.PrefabPath + step.Prefab, TutorialController.Instance.Container);

            // make its button interactable in case being set to false
            Component[] buttons = prefab.GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                button.interactable = true;
            }

            buttons = TutorialController.Instance.Container.GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                button.interactable = true;
            }


            GeneralTutorialProgress progress = prefab.GetComponent<GeneralTutorialProgress>();
            progress.StartProgress(step);
            progress.TutorialProgressEndCallback.AddListener(OnStepEnd);
            progress.TutorialProgressTerminatedCallback.AddListener(OnTutorialBeTerminated);
            progress.TutorialProgressSkippedCallback.AddListener(OnTutorialStepBeSkipped);
            cachedPrefab.Add(prefab);
        }
        else
        {
            //Simple action
            switch (step.Type)
            {
                case "Action":
                    for (int i = 0; i < step.Target.Length; i++)
                    {
                        Transform go = MainCanvas.Instance.CanvasContainer.FindRecursive(step.Target[i]);

                        if (go != null)
                        {
                            go.gameObject.SetActive(step.ActionValue[i]);

                            // lets be honest, the tutorial controller just can't handle user clicking too fast, so we need to temporarily disable buttons to prevent user clicking too quickly
                            if (step.ActionValue[i])
                            {
                                Component[] buttons = go.GetComponentsInChildren<Button>();

                                foreach (Button button in buttons)
                                    ButtonTool.PausingInteractable(button, 1f);
                            }
                        }
                        else
                        {
                            Debug.Log("Cannot find target " + step.Target[i]);
                        }
                    }

                    break;
                case "SceneTransit":

                    SceneTransitHandler.LoadScene(step.Target[0], step.ActionValue[0]);
                    break;
                case "End":

                    PlayerPrefs.SetInt(step.Target[0], step.ActionValue[0] ? 1 : 0);
                    break;
            }

            OnEventTriggered(step.TriggerOnEnd);
        }
    }

    protected void OnStepEnd(TutorialStep step)
    {
        EndStep();
        if (!string.IsNullOrEmpty(step.TriggerOnEnd))
        {
            OnEventTriggered(step.TriggerOnEnd);
        }
    }

    protected void OnTutorialBeTerminated(TutorialStep step)
    {
        Debug.Log(string.Format("TutorialBeTerminated in step {0} : {1}", step.Step, ProGamer.Localization.Get(step.ActionDescription)));
        EndTutorial();

        AnalyticsLogCenter.LogEvent("has_skipped_tutorial".AddCaption(tutorialName));
    }

    protected void OnTutorialStepBeSkipped(TutorialStep step)
    {
        Debug.Log(string.Format("TutorialStepBeSkipped in step {0} : {1}", step.Step, ProGamer.Localization.Get(step.ActionDescription)));
        SkipStep();

        AnalyticsLogCenter.LogEvent("has_skipped_tutorial_step".AddCaption(tutorialName), new Dictionary<string, object>()
        {
            { "step_description".AddCaption(tutorialName), ProGamer.Localization.Get(step.ActionDescription) }
        });
    }

    //Only support skip to next scene tutorial
    protected void SkipStep()
    {
        EndStep();

        TutorialStep step = GetNextStep();

        while (step != null && step.TriggerCondition != "SceneLoaded")
        {
            Debug.Log(string.Format("Skip step {0} : {1}", step.Step, ProGamer.Localization.Get(step.ActionDescription)));

            currentStep++;
            step = GetNextStep();
        }

        if (step == null)
        {
            EndTutorial();
            //End it by return Aquarium
            SceneTransitHandler.LoadScene("Aquarium");
        }
        else
        {
            //Load scene to start other part of tutorial
            SceneTransitHandler.LoadScene(step.TriggeredByParameter);
        }
    }

    protected void EndStep()
    {
        currentStatus = TutorialFlowStatus.Waiting;

        for (int i = 0; i < cachedPrefab.Count; i++)
        {
            Destroy(cachedPrefab[i]);
        }

        cachedPrefab = new List<GameObject>();

        //Clear data
        if (currentStep == StepCount)
        {
            EndTutorial();
        }
    }

    protected void EndTutorial()
    {
        if (currentStatus != TutorialFlowStatus.End)
        {
            currentStatus = TutorialFlowStatus.End;
            //Clear all
            for (int i = 0; i < cachedPrefab.Count; i++)
            {
                Destroy(cachedPrefab[i]);
            }

            cachedPrefab = new List<GameObject>();

            AnalyticsLogCenter.LogEvent("has_finished_tutorial".AddCaption(tutorialName), new Dictionary<string, object>()
            {
                { "tutorial_name", TutorialName }
            });

            GameSave.LastTutorialFinishTime = System.DateTime.UtcNow.ToString();
            Debug.Log("End of tutorial: " + TutorialName);
        }
    }

    protected TutorialStep GetNextStep()
    {
        if (currentStep + 1 < StepCount)
        {
            return stepList[currentStep + 1];
        }
        else
        {
            return null;
        }
    }

    public void OnEventTriggered(string eventName)
    {
        TutorialStep nextStep = GetNextStep();

        if (nextStep == null)
        {
            EndTutorial();
        }
        else
        {
            if (currentStatus == TutorialFlowStatus.Waiting)
            {
                if (nextStep.TriggeredByParameter == eventName)
                {
                    NextStep();
                }
            }
        }
    }

    public void Jump()
    {
        currentStatus = TutorialFlowStatus.Waiting;

        for (int i = 0; i < cachedPrefab.Count; i++)
        {
            Destroy(cachedPrefab[i]);
        }

        cachedPrefab = new List<GameObject>();

        //Clear data
        if (currentStep == StepCount)
        {
            EndTutorial();
        }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        OnEventTriggered(scene.name);
    }
}