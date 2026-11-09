using System;
using Auki.ConjureKit.Vikja;
using UnityEngine;

public class GameStateHandler
{
    private const string StateAction = "State.Action";
    private bool _isHost;

    public event Action<ARGameState> OnStateUpdate;

    public void Setup(bool isHost)
    {
        _isHost = isHost;
        AukiSdkWrapper.Instance.EntityActionEvent += OnEntityActionReceived;
    }

    public void CleanUp()
    {
        AukiSdkWrapper.Instance.EntityActionEvent -= OnEntityActionReceived;
    }

    public void ChangeHost(bool select)
    {
        _isHost = select;
    }

    public void SendStateUpdate(uint entityId, ARGameState state)
    {
        AukiSdkWrapper.Instance.RequestEntityAction(
        entityId,
        StateAction,
        state.ToJsonByteArray(),
        null,
        Debug.LogError);

        OnStateUpdate?.Invoke(state);
    }
    
    private void OnEntityActionReceived(EntityAction action)
    {
        if (_isHost) return;

        if (!action.Name.Equals(StateAction)) return;

        var state = action.Data.FromJsonByteArray<ARGameState>();

        OnStateUpdate?.Invoke(state);
    }
}