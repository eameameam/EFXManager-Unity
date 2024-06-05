using UnityEditor;
using UnityEngine;

public static class FXRenderer
{
    public const string PreviewLayerName = "PreviewLayer";
    public static int previewLayer = -1;
    private static readonly Color DefaultBackgroundColor = new Color(0.05f, 0.05f, 0.05f);

    public static void Initialize()
    {
        previewLayer = LayerMask.NameToLayer(PreviewLayerName);
        if (previewLayer == -1)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty layer = layers.GetArrayElementAtIndex(i);
                if (layer.stringValue == "")
                {
                    layer.stringValue = PreviewLayerName;
                    tagManager.ApplyModifiedProperties();
                    previewLayer = i;
                    break;
                }
            }
        }
    }

    public static void CleanUp()
    {
        if (previewLayer != -1)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty layer = layers.GetArrayElementAtIndex(i);
                if (layer.stringValue == PreviewLayerName)
                {
                    layer.stringValue = "";
                    tagManager.ApplyModifiedProperties();
                    break;
                }
            }
        }
    }

    public static void CreatePreviewTexture(int index, GameObject selectedPrefab, RenderTexture[] renderTextures, Texture2D[] previewTextures, Camera previewCamera, ref GameObject previewInstance, ref ParticleSystem particleSystem)
    {
        if (previewInstance != null)
        {
            GameObject.DestroyImmediate(previewInstance);
        }

        previewInstance = GameObject.Instantiate(selectedPrefab);
        previewInstance.transform.position = Vector3.zero;

        if (previewLayer == -1) Initialize();

        SetLayerRecursively(previewInstance, previewLayer);
        previewCamera.cullingMask = 1 << previewLayer;

        particleSystem = previewInstance.GetComponentInChildren<ParticleSystem>();
        if (particleSystem != null)
        {
            particleSystem.Simulate(particleSystem.main.duration / 2, true, true, true);
            RenderTexture.active = renderTextures[index];
            previewCamera.targetTexture = renderTextures[index];
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = DefaultBackgroundColor;
            previewCamera.Render();
            previewTextures[index] = new Texture2D(renderTextures[index].width, renderTextures[index].height, TextureFormat.RGBA32, false);
            previewTextures[index].ReadPixels(new Rect(0, 0, renderTextures[index].width, renderTextures[index].height), 0, 0);
            previewTextures[index].Apply();
            RenderTexture.active = null;

            GameObject.DestroyImmediate(previewInstance);
            previewInstance = null;
        }
        else
        {
            Debug.Log("No ParticleSystem found on prefab.");
        }
    }

    public static void CreatePreviewInstance(int index, GameObject[] selectedPrefabs, ref GameObject previewInstance, ref ParticleSystem particleSystem)
    {
        if (previewInstance != null)
        {
            GameObject.DestroyImmediate(previewInstance);
        }

        previewInstance = GameObject.Instantiate(selectedPrefabs[index]);
        previewInstance.transform.position = Vector3.zero;

        if (previewLayer == -1) Initialize();

        SetLayerRecursively(previewInstance, previewLayer);

        particleSystem = previewInstance.GetComponentInChildren<ParticleSystem>();
        if (particleSystem != null)
        {
            particleSystem.Play();
            Selection.activeGameObject = particleSystem.gameObject;
        }
        else
        {
            Debug.Log("No ParticleSystem found on prefab.");
        }
    }

    public static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null)
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
