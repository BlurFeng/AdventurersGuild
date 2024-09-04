using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Collections;

static public class UtilityExtension
{
    public static T[] ToArray<T>(this RepeatedField<T> re)
    {
        T[] array = new T[re.Count];
        for (int i = 0; i < re.Count; i++)
        {
            array[i] = re[i];
        }

        return array;
    }
}
