using System;
using System.Threading;
using Auki.ConjureKit.Vikja;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TimerHandler
{
    private int _timer;
    private CancellationTokenSource _timerCancelledTokenSource;
    private uint _entityId;
    private bool _isHost;

    public event Action<int> OnTimerUpdate;

    private const string TimerAction = "Timer.Action";

    public void Setup(uint entityId, bool isHost)
    {
        _entityId = entityId;
        _isHost = isHost;

        AukiSdkWrapper.Instance.EntityActionEvent += OnEntityActionReceived;
    }

    public void Cleanup()
    {
        AukiSdkWrapper.Instance.EntityActionEvent -= OnEntityActionReceived;
    }

    public void ChangeHost(bool selected)
    {
        _isHost = selected;
    }

    public async void RunTimer(int duration, Action timeOutCallback)
    {
        await UniTask.NextFrame();

        _timerCancelledTokenSource = new CancellationTokenSource();
        _timer = duration;

        try
        {
            while (_timer > 0)
            {
                // OnTimerUpdate?.Invoke(_timer);
                SendTimerEntityAction(_timer);
                await UniTask.WaitForSeconds(1, cancellationToken: _timerCancelledTokenSource.Token);
                _timer--;
            }
        }
        catch (OperationCanceledException)
        {
            // OnTimerUpdate?.Invoke(-1);
            SendTimerEntityAction(-1);
            return;
        }
        finally
        {
            _timerCancelledTokenSource?.Dispose();
            _timerCancelledTokenSource = null;
        }

        // OnTimerUpdate?.Invoke(-1);
        SendTimerEntityAction(-1);
        timeOutCallback?.Invoke();
    }

    public void CancelTimer()
    {
        _timerCancelledTokenSource?.Cancel();
    }

    private void SendTimerEntityAction(int timer)
    {
        AukiSdkWrapper.Instance.RequestEntityAction(
        _entityId,
        TimerAction,
        timer.ToJsonByteArray(),
        result =>
        {
            OnTimerUpdate?.Invoke(timer);
        },
        Debug.LogError);
    }

    private void OnEntityActionReceived(EntityAction action)
    {
        if (_isHost) return;

        if (!action.Name.Equals(TimerAction)) return;

        var timer = action.Data.FromJsonByteArray<int>();
        OnTimerUpdate?.Invoke(timer);
    }

}