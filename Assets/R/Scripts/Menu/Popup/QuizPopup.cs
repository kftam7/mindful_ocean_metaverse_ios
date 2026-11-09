using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ProGamer;
using UnityEngine.UI;

public class QuizPopup : SimplePopup
{
    public Button[] Answers;
    public Transform MainImageContainer;
    public Text MainText;
    protected QuestionData questionData;

    protected bool isFinish = false;
    protected GameObject mainImageObject;


    protected override void InitPopup()
    {
        base.InitPopup();

        QuestionData data = DataCenter.Instance.QuestionList[Random.Range(0, DataCenter.Instance.QuestionList.Count)];
        this.questionData = data;

        MainText.text = Localization.Get(data.QuestionText);
        if (mainImageObject != null)
        {
            Destroy(mainImageObject);
        }

        mainImageObject = ProGamer.ReuseableObjectManager.Instantiate("Prefabs/Quiz/" + data.MainImage, MainImageContainer);

        for (int i = 0; i < data.Answer.Length; i++)
        {
            Text[] texts = Answers[i].GetComponentsInChildren<Text>(true);
            foreach (Text t in texts)
            {
                t.text = Localization.Get(data.Answer[i]);
            }

            Answers[i].GetComponent<SetCorrect>().SetStatus(AnswerStatus.Default);
            Answers[i].interactable = true;
        }

        isFinish = false;
    }

    public void ChooseAnswer(int index)
    {
        if (!isFinish)
        {
            if (index == questionData.RightAnswerIndex)
            {
                SoundManager.Instance.Play("correct_anwer");

                isFinish = true;
                foreach (Button b in Answers)
                {
                    b.interactable = false;
                }

                Answers[index].GetComponent<SetCorrect>().SetStatus(AnswerStatus.Right);
                StartCoroutine(EnterGame());
            }
            else
            {
                SoundManager.Instance.Play("wrong_anwer");

                Answers[index].GetComponent<SetCorrect>().SetStatus(AnswerStatus.Wrong);
                Answers[index].interactable = false;
            }
        }
    }

    IEnumerator EnterGame()
    {
        yield return new WaitForSeconds(1f);
        UnityAction callback = (UnityAction)this.parameters["QuizCallback"];
        callback.Invoke();

        base.Close();
    }

    public override void Close()
    {
        if (!isFinish)
        {
            if (UIController.Instance != null) UIController.Instance.SwitchState(UIController.State.CharMenu);
            base.Close();
        }
    }
}