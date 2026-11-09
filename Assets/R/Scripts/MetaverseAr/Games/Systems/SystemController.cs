using System;
using System.Collections.Generic;
using Auki.ConjureKit;
using Auki.ConjureKit.ECS;
using UnityEngine;

public class SystemController
{
    protected Session _session;
    protected Dictionary<Type, bool> _systemsInitialized = new();

    protected void RegisterSystem<T>(T system, Action completedCallback) where T : SystemBase
    {
        _session.RegisterSystem(system, () =>
        {
            var type = system.GetType();
            if (_systemsInitialized.ContainsKey(type) && _systemsInitialized[type])
            {
                Logger.Log($"{system} has been initialized, exit register");
                return;
            }

            Logger.Log($"{system} initialized");
            _systemsInitialized[type] = true;
            completedCallback?.Invoke();
        });
    }

    protected void UnregisterSystem<T>(T system) where T : SystemBase
    {
        var type = system.GetType();
        if (!_systemsInitialized[type]) return;

        _systemsInitialized[type] = false;
        _session.UnregisterSystem(system);
    }
}