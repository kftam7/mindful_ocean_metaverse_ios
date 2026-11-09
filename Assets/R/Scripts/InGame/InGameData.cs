using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProGamer;

public enum DeadReason
{
    Starve = 0,
    Hurt = 1
}

public class InGameData : SceneInstance<InGameData>
{
    protected int mHP { get; set; }
    protected int mFullHP { get; set; }
    protected int mScore { get; set; }
    protected int mTime { get; set; }
    protected float mCoinBonus { get; set; }
    protected InGameTaskData mTask { get; set; }

    public InGameTaskData Task
    {
        get { return mTask; }
    }

    public int Score
    {
        get { return mScore; }
    }

    public int WholeTime
    {
        get { return mTime; }
    }

    public float CoinBonus
    {
        get { return mCoinBonus; }
    }

    protected bool isAlive = true;

    public void InitGameData(int hp, int scroe, float bonus, int time, InGameTaskData task)
    {
        this.mHP = hp;
        this.mFullHP = hp;
        this.mScore = scroe;
        this.mTime = time;
        this.mTask = task;
        this.mCoinBonus = bonus;
        this.isAlive = true;

        EventManager.TriggerEvent(EventList.OnGameStatusChanged, this.isAlive);
        EventManager.TriggerEvent(EventList.OnTaskProgressUpdated, mTask);
        EventManager.TriggerEvent(EventList.OnHpPercentageUpdated, new BarValue(this.mHP, this.mFullHP));
    }

    ///return whether alive
    public void GotDamage(int damage)
    {
        this.mHP = Mathf.Max(0, this.mHP - damage);

        EventManager.TriggerEvent(EventList.OnHpPercentageUpdated, new BarValue(this.mHP, this.mFullHP));

        if (this.isAlive && this.mHP <= 0)
        {
            this.isAlive = false;
            EndGame(DeadReason.Hurt);
        }
    }

    public void GotHungry(int time)
    {
        this.mHP = Mathf.Max(0, this.mHP - HungryTimeToDamage(time));

        EventManager.TriggerEvent(EventList.OnHpPercentageUpdated, new BarValue(this.mHP, this.mFullHP));

        if (this.isAlive && this.mHP <= 0)
        {
            this.isAlive = false;
            EndGame(DeadReason.Starve);
        }
    }

    public int HungryTimeToDamage(int time)
    {
        //For later setting
        return time;
    }

    public void GotFood(int hp)
    {
        this.mHP = Mathf.Max(0, Mathf.Min(this.mFullHP, this.mHP + hp));

        UpdateTaskProgress(TaskTarget.Food, 1);
        EventManager.TriggerEvent(EventList.OnHpPercentageUpdated, new BarValue(this.mHP, this.mFullHP));
    }

    public void GainTime(int time)
    {
        this.mTime = Mathf.Max(0, this.mTime + time);
        EventManager.TriggerEvent(EventList.OnTimeUpdated, this.mTime);
    }

    public void GainCoin(int coin)
    {
        GameSave.Coin = Mathf.Max(0, GameSave.Coin + coin);
        EventManager.TriggerEvent(EventList.OnCoinUpdated, GameSave.Coin);
    }

    public void GainScore(int scroe)
    {
        this.mScore = Mathf.Max(0, this.mScore + scroe);

        UpdateTaskProgress(TaskTarget.Score, scroe);
        EventManager.TriggerEvent(EventList.OnScoreUpdated, this.mScore);
    }

    public void EndGame(DeadReason reason)
    {
        //Save
        if (mTask != null)
        {
            //Show in menu
            TaskController.Instance.SaveTask(mTask);
        }

        UpdateTaskProgress(TaskTarget.PlayGame, 1);

        DataCenter.Instance.AddElementToLeaderboard(new LeaderboardElement(DataCenter.Instance.PlayerQuestionnaireData.PlayerName, this.Score, GameSave.SelectedModel));

        EventManager.TriggerEvent(EventList.OnGameStatusChanged, this.isAlive);

        StartCoroutine(Gameover(reason));
    }

    IEnumerator Gameover(DeadReason reason)
    {
        yield return new WaitForSeconds(3f);

        //Gameover popup
        Hashtable hashtable = new Hashtable();
        hashtable[PopupProperty.Prefab] = PopupManager.Instance.GetPopupPrefab(PopupPrefab.DeathPopup);
        hashtable["DeadReason"] = reason;
        PopupManager.Instance.AddPopupToQueueAndShow(hashtable);
    }

    public void UpdateTaskProgress(TaskTarget target, int amount)
    {
        if (mTask != null && mTask.TaskStaticData.Target == target)
        {
            mTask.GainProgress(amount);
            EventManager.TriggerEvent(EventList.OnTaskProgressUpdated, mTask);
        }
    }

    float timeCount = 0;

    void Update()
    {
        timeCount += Time.deltaTime;

        if (timeCount >= 1 && isAlive)
        {
            GainTime(1);
            GotHungry(1);
            timeCount -= 1;
        }
    }
}

public class BarValue
{
    public int CurrentValue { get; set; }
    public int FullValue { get; set; }

    public float Percentage
    {
        get { return (float)CurrentValue / FullValue; }
    }

    public BarValue(int current, int full)
    {
        this.CurrentValue = current;
        this.FullValue = full;
    }
}