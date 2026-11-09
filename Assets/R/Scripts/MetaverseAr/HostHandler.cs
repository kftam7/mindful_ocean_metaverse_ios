using System;
using System.Collections.Generic;
using System.Linq;
using Auki.ConjureKit;
using Auki.ConjureKit.Vikja;
using Cysharp.Threading.Tasks;
using UnityEngine;

[Serializable]
public class HostHandler
{
    private Session _session;
    private uint _currentHostId;

    private const string HostAppointAction = "Host.Appoint.Action";

    public bool IsHost => _currentHostId == _session.ParticipantId;

    public static event Action<bool> HostAppointedEvent;

    public void Setup(Session session)
    {
        _session = session;
        AukiSdkWrapper.Instance.EntityActionEvent += EntityActionEvent;
    }

    public void CleanUp()
    {
        AukiSdkWrapper.Instance.EntityActionEvent += EntityActionEvent;
    }

    public void DecideNewHost(uint oldHostId)
    {
        Logger.Log($"[Host Handler] Deciding new host, player left: {oldHostId}, current host: {_currentHostId}");
        if (oldHostId != _currentHostId) return;

        var newHostId = FindClosestValue(_session.GetParticipantsIds(), oldHostId);
        Logger.Log($"[Host Handler] Deciding new host, new host: {newHostId}");

        if (_session.ParticipantId != newHostId) return;

        TakeHostRole(newHostId);
    }

    public async void TakeHostRole(uint participantId)
    {
        await UniTask.WaitUntil(() => _session.GetLocalParticipantEntity() != null);
        Logger.Log($"[Host Handler] Assigning new host to: {participantId}");
        AukiSdkWrapper.Instance.RequestEntityAction(
            _session.GetLocalParticipantEntity().Id,
            HostAppointAction,
            participantId.ToJsonByteArray(),
            success =>
            {
                UpdateHost(success.Data.FromJsonByteArray<uint>());
            },
            err=>Logger.LogError(err));
    }

    private void EntityActionEvent(EntityAction action)
    {
        if (!action.Name.Equals(HostAppointAction)) return;

        var newHostId = action.Data.FromJsonByteArray<uint>();

        UpdateHost(newHostId);
    }

    private void UpdateHost(uint newHostId)
    {
        _currentHostId = newHostId;
        if (_session.ParticipantId == _currentHostId)
        {
            Logger.Log($"[Host Handler] You're now the new host: {_currentHostId}");
        }
        HostAppointedEvent?.Invoke(IsHost);
    }

    private static T FindClosestValue<T>(IEnumerable<T> collection, T target)
        where T : IComparable<T>, IConvertible // Constraints for comparison and numeric operations
    {
        if (collection == null || !collection.Any())
        {
            throw new ArgumentException("Collection cannot be null or empty.");
        }

        // Order the collection by the absolute difference between each item and the target
        // and then take the first element.
        T closest = collection.OrderBy(item => Math.Abs(Convert.ToDouble(target) - Convert.ToDouble(item))).First();

        return closest;
    }
}