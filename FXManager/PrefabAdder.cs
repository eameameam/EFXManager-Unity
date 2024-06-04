using UnityEditor;
using UnityEngine;

public static class PrefabAdder
{
    public static void AddPrefabToScene(GameObject prefab)
    {
        if (prefab != null)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (instance != null)
            {
                instance.transform.position = Vector3.zero;
                instance.transform.rotation = Quaternion.identity;
                Undo.RegisterCreatedObjectUndo(instance, "Add Prefab to Scene");
                Selection.activeGameObject = instance;
            }
            else
            {
                Debug.LogError("Failed to instantiate prefab.");
            }
        }
        else
        {
            Debug.LogError("No prefab provided.");
        }
    }

    public static void AddPrefabToSelected(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("No prefab provided.");
            return;
        }

        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            Debug.LogError("No selected object.");
            return;
        }

        if (IsTemporaryObject(selectedObject) || IsTemporaryObject(selectedObject.transform.parent?.gameObject))
        {
            Debug.LogError("Cannot add prefab to a temporary preview object.");
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        if (instance != null)
        {
            instance.transform.SetParent(selectedObject.transform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            Undo.RegisterCreatedObjectUndo(instance, "Add Prefab to Selected");
            Selection.activeGameObject = instance;
        }
        else
        {
            Debug.LogError("Failed to instantiate prefab.");
        }
    }

    private static bool IsTemporaryObject(GameObject obj)
    {
        if (obj == null) return false;
        return obj.layer == FXRenderer.previewLayer || obj.name.Contains("Preview");
    }
}
