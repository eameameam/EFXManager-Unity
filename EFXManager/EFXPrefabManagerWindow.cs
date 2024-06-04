using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class EFXPrefabManagerWindow : EditorWindow
{
    private EFXPrefabsList _prefabsList;
    private Vector2 _scrollPosition;

    public delegate void PrefabsUpdatedHandler(List<GameObject> prefabs);
    public static event PrefabsUpdatedHandler OnPrefabsUpdated;

    public static void ShowWindow()
    {
        GetWindow<EFXPrefabManagerWindow>("EFX Prefab Manager");
    }

    private void OnEnable()
    {
        LoadPrefabsList();
    }

    private void OnDisable()
    {
        SavePrefabsList();
    }

    private void LoadPrefabsList()
    {
        string[] guids = AssetDatabase.FindAssets("t:EFXPrefabsList");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _prefabsList = AssetDatabase.LoadAssetAtPath<EFXPrefabsList>(path);
        }

        if (_prefabsList == null)
        {
            _prefabsList = ScriptableObject.CreateInstance<EFXPrefabsList>();
            AssetDatabase.CreateAsset(_prefabsList, "Assets/EFXPrefabsList.asset");
            AssetDatabase.SaveAssets();
        }
    }

    private void SavePrefabsList()
    {
        EditorUtility.SetDirty(_prefabsList);
        AssetDatabase.SaveAssets();
    }

    public static List<GameObject> GetPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:EFXPrefabsList");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            EFXPrefabsList prefabsList = AssetDatabase.LoadAssetAtPath<EFXPrefabsList>(path);
            if (prefabsList != null)
            {
                return prefabsList.prefabs;
            }
        }
        return new List<GameObject>();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        DrawHeaderSection();
        DrawDragAndDropArea();
        DrawPrefabsList();
    }

    private void DrawHeaderSection()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        GUILayout.BeginVertical();
        GUILayout.Label("EFX Prefab Manager", EditorStyles.largeLabel);
        GUILayout.Label("Drag and drop prefabs with Particle Systems here,\nor select them manually. Only prefabs containing\nParticle Systems will be accepted.", EditorStyles.miniLabel);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        DrawActionButtons();
        GUILayout.Space(30);
        GUILayout.EndHorizontal();
    }

    private void DrawActionButtons()
    {
        GUILayout.BeginVertical();
        GUI.backgroundColor = new Color(0.6f, 0.2f, 0.2f);
        if (GUILayout.Button("Clear List", GUILayout.Height(40), GUILayout.Width(100)))
        {
            _prefabsList.prefabs.Clear();
            OnPrefabsUpdated?.Invoke(_prefabsList.prefabs);
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndVertical();
    }

    private void DrawDragAndDropArea()
    {
        GUILayout.Space(10);
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "\nDrag Prefabs Here To Add To The List");
        HandleDragAndDrop(dropArea);
    }

    private void DrawPrefabsList()
    {
        GUILayout.Space(10);

        if (_prefabsList.prefabs.Count == 0)
        {
            GUILayout.Label("No Prefabs Found", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        int? removeIndex = null;
        for (int i = 0; i < _prefabsList.prefabs.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            _prefabsList.prefabs[i] = (GameObject)EditorGUILayout.ObjectField(_prefabsList.prefabs[i], typeof(GameObject), false);
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                removeIndex = i;
            }
            EditorGUILayout.EndHorizontal();
        }
        if (removeIndex.HasValue)
        {
            _prefabsList.prefabs.RemoveAt(removeIndex.Value);
            OnPrefabsUpdated?.Invoke(_prefabsList.prefabs);
        }
        EditorGUILayout.EndScrollView();
    }

    private void HandleDragAndDrop(Rect dropArea)
    {
        Event evt = Event.current;
        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            if (!dropArea.Contains(evt.mousePosition)) return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                foreach (Object obj in DragAndDrop.objectReferences)
                {
                    if (obj is GameObject prefab && ContainsParticleSystem(prefab) && !_prefabsList.prefabs.Contains(prefab))
                    {
                        _prefabsList.prefabs.Add(prefab);
                    }
                }

                OnPrefabsUpdated?.Invoke(_prefabsList.prefabs);
            }
        }
    }

    private bool ContainsParticleSystem(GameObject prefab)
    {
        return prefab.GetComponentInChildren<ParticleSystem>(true) != null;
    }
}
