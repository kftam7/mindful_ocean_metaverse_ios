using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ProGamer;
using System.Threading;
using System;

public class TipView : MonoBehaviour
{
    [SerializeField] private RectTransform tipPanel;
    [SerializeField] private Toggle tipCheckmark;
    [SerializeField] private Text tipText;
    [SerializeField] private Button closeTipButton;

    private Queue<TipQueue> _tipQueue;
    private Tween _showPanel, _hidePanel;
    private CancellationTokenSource _closeToken;

    void Start()
    {
        InitializeView();
        GameController.OnStateChange += StateChange;
    }

    void OnDestroy()
    {
        GameController.OnStateChange -= StateChange;
    }

    private void StateChange(ARGameState state)
    {
        if (state == ARGameState.GameEnd) CloseTip();
    }

    private void InitializeView()
    {
        _hidePanel = tipPanel.DOAnchorPosY(75f, 0.35f).SetAutoKill(false).Pause();
        _showPanel = tipPanel.DOAnchorPosY(-75f, 0.35f).SetAutoKill(false).Pause();
        _hidePanel.Complete();

        closeTipButton.onClick.AddListener(CloseTip);

        tipCheckmark.isOn = false;
    }

    private void CloseTip()
    {
        if (_closeToken != null)
        {
            _tipQueue.Clear();
            _closeToken.Cancel();
            _closeToken.Dispose();
            _closeToken = null;
            _ = Hide();
        }
    }

    private async UniTask Show()
    {
        if (_showPanel.IsComplete())
            _showPanel.Restart();
        else
            _showPanel.Play();

        await _showPanel.AsyncWaitForCompletion();
    }

    private async UniTask Hide()
    {
        if (_hidePanel.IsComplete())
            _hidePanel.Restart();
        else
            _hidePanel.Play();
            
        await _hidePanel.AsyncWaitForCompletion();
    }

    public void QueueTips(Queue<TipQueue> tips)
    {
        if (GameSave.SinglePlayerModeLimitReached) return;
        
        _tipQueue = tips;

        ShowTipInOrder();
    }

    private async void ShowTipInOrder()
    {
        _closeToken = new CancellationTokenSource();
        while (_tipQueue.Count > 0)
        {
            var tip = _tipQueue.Dequeue();
            tipCheckmark.isOn = false;
            tipText.text = Localization.Get(tip.messageKey);
            LayoutRebuilder.ForceRebuildLayoutImmediate(tipPanel);
            await Show();
            try
            {
                await UniTask.WaitUntil(() => tip.ConditionMet(), cancellationToken: _closeToken.Token);
                tipCheckmark.isOn = true;
                await UniTask.WaitForSeconds(1.5f);
                tip.Deinitialize();
                await Hide();
            }
            catch (OperationCanceledException e)
            {
                Logger.LogWarning($"Tip cancelled gracefully: {e.Message}");
            }
        }
    }
}