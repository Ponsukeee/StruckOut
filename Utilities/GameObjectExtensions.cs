using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{
    /// <summary>
    /// 孫オブジェクトを除くすべての子オブジェクトを返します
    /// </summary>
    public static GameObject[] GetChildrenWithoutGrandchild(this GameObject self)
    {
        var result = new List<GameObject>();
        foreach (Transform n in self.transform)
        {
            result.Add(n.gameObject);
        }
        return result.ToArray();
    }
}