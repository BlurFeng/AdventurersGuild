using System;
using System.Collections.Generic;
using UnityEngine;

namespace FsStateSystem
{
    public static class FsStateSystemUtility
    {
        static List<string> m_stateNames = null;

        static void Init()
        {
            if (m_stateNames == null)
            {
                m_stateNames = new List<string>(Enum.GetNames(typeof(State)));
            }
        }

        public static State Parse(string str)
        {
            Init();

            if (string.IsNullOrEmpty(str))
                return State.None;

            if (m_stateNames.Contains(str))
            {
                return (State)Enum.Parse(typeof(State), str);
            }

            Debug.LogWarning("Failed to parse :" + str);

            return State.None;
        }
    }
}
