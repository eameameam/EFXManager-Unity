using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EFXPrefabsList", menuName = "ScriptableObjects/EFXPrefabsList", order = 1)]
public class EFXPrefabsList : ScriptableObject
{
    public List<GameObject> prefabs = new List<GameObject>();
}