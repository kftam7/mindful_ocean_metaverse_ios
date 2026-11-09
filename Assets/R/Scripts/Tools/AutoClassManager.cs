using UnityEngine;
using System.Collections.Generic;
using System;

namespace ProGamer
{
    public class AutoClassManager : MonoBehaviour
    {
        private static Dictionary<Type, Type> autoClassMap = new Dictionary<Type, Type>();

        protected void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public static Type GetAutoClass(Type baseClassType)
        {
            Type autoClassType;
            if (autoClassMap.TryGetValue(baseClassType, out autoClassType))
            {
                return autoClassType;
            }
            else
            {
                return baseClassType;
            }
        }

        public static void RegisterAutoClass(Type baseClassType, Type autoClassType)
        {
            autoClassMap[baseClassType] = autoClassType;
        }
    }
}