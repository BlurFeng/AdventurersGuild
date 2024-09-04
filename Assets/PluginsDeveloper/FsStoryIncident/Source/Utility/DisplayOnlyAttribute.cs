using System;
using UnityEngine;

namespace FsStoryIncident
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    public class DisplayOnlyAttribute : PropertyAttribute
    {
    }
}