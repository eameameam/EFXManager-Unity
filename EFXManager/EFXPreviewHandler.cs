using UnityEditor;
using UnityEngine;

public class EFXPreviewHandler
{
    private GameObject[] _selectedPrefabs;
    private RenderTexture[] _renderTextures;
    private Texture2D[] _previewTextures;
    private Camera _previewCamera;
    private GameObject _previewInstance;
    private ParticleSystem _particleSystem;
    private GameObject _gridInstance;
    private int _activePreview = -1;
    private float _distance = 2.0f;
    private Vector3 _cameraTarget = Vector3.zero;
    private float _rotationSpeed = 10f;
    private float _scrollSpeed = 2f;
    private Color _backgroundColor;
    private Rect _activePreviewRect;
    private bool _isDragging = false;
    private bool _isStopped = false;
    private bool _isTrailMode = false;
    private TrailMover _trailMover;
    private System.Action _onPreviewStarted;

    public EFXPreviewHandler(Color initialColor, System.Action onPreviewStarted)
    {
        _backgroundColor = initialColor;
        _onPreviewStarted = onPreviewStarted;

#if UNITY_EDITOR
        FXRenderer.Initialize();

        _previewCamera = new GameObject("Preview Camera").AddComponent<Camera>();
        _previewCamera.transform.position = new Vector3(-1.5f, 1.0f, -1.5f);
        _previewCamera.transform.rotation = Quaternion.Euler(15f, 45f, 0);
        _previewCamera.orthographic = false;
        _previewCamera.clearFlags = CameraClearFlags.SolidColor;
        _previewCamera.backgroundColor = _backgroundColor;
        _previewCamera.cullingMask = 1 << FXRenderer.previewLayer;
        _previewCamera.allowHDR = false;
        _previewCamera.allowMSAA = true;

        CreateGrid(0.15f, 0.15f, Color.gray);
#endif
    }

    public void OnDisable()
    {
#if UNITY_EDITOR
        if (_previewInstance != null)
        {
            GameObject.DestroyImmediate(_previewInstance);
        }

        if (_gridInstance != null)
        {
            GameObject.DestroyImmediate(_gridInstance);
        }

        if (_previewCamera != null)
        {
            GameObject.DestroyImmediate(_previewCamera.gameObject);
        }

        if (_renderTextures != null)
        {
            for (int i = 0; i < _renderTextures.Length; i++)
            {
                if (_renderTextures[i] != null)
                {
                    _renderTextures[i].Release();
                    GameObject.DestroyImmediate(_renderTextures[i]);
                }
            }
        }

        FXRenderer.CleanUp();
#endif
    }

    private void CreateGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        _gridInstance = new GameObject("Preview Grid");

        float gridSize = 4.0f;
        int gridLines = Mathf.CeilToInt(gridSize / gridSpacing) * 2;

        for (int i = 0; i <= gridLines; i++)
        {
            float offset = -gridSize + (i * gridSpacing);

            CreateGridLine(new Vector3(-gridSize, 0, offset), new Vector3(gridSize, 0, offset), gridColor, gridOpacity);
            CreateGridLine(new Vector3(offset, 0, -gridSize), new Vector3(offset, 0, gridSize), gridColor, gridOpacity);
        }

        FXRenderer.SetLayerRecursively(_gridInstance, FXRenderer.previewLayer);
    }

    private void CreateGridLine(Vector3 start, Vector3 end, Color color, float opacity)
    {
        GameObject line = new GameObject("GridLine");
        line.transform.parent = _gridInstance.transform;
        LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.005f;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")) { color = new Color(color.r, color.g, color.b, opacity) };
    }

    public void SetGridVisibility(bool isVisible)
    {
        if (_gridInstance != null)
        {
            _gridInstance.SetActive(isVisible);
        }
    }

    public void SetPrefab(int index, GameObject prefab)
    {
        if (index >= _selectedPrefabs.Length || prefab == null)
            return;

        _selectedPrefabs[index] = prefab;

        if (_previewTextures[index] == null)
        {
            FXRenderer.CreatePreviewTexture(index, _selectedPrefabs[index], _renderTextures, _previewTextures, _previewCamera, ref _previewInstance, ref _particleSystem);
        }
    }

    public void OnGUI(float previewSize)
    {
#if UNITY_EDITOR
        int columns = Mathf.FloorToInt(EditorGUIUtility.currentViewWidth / previewSize);

        for (int i = 0; i < _selectedPrefabs.Length; i += columns)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < columns; j++)
            {
                int index = i + j;
                if (index >= _selectedPrefabs.Length)
                    break;

                EditorGUILayout.BeginVertical(GUILayout.Width(previewSize));

                Rect previewRect = GUILayoutUtility.GetRect(previewSize, previewSize);

                if (_activePreview == index && !_isStopped)
                {
                    if (_renderTextures[index] != null)
                    {
                        GUI.DrawTexture(previewRect, _renderTextures[index], ScaleMode.ScaleToFit);
                    }
                    _activePreviewRect = previewRect;
                }
                else if (_previewTextures[index] != null)
                {
                    GUI.DrawTexture(previewRect, _previewTextures[index], ScaleMode.ScaleToFit);
                }

                EditorGUILayout.BeginHorizontal();
                bool hasBrokenTextures = HasBrokenTextures(_selectedPrefabs[index]);
                if (hasBrokenTextures)
                {
                    GUI.color = Color.red;
                    GUILayout.Label(_selectedPrefabs[index] ? _selectedPrefabs[index].name : "No Prefab", EditorStyles.helpBox, GUILayout.Width(previewSize - 25));
                    GUI.color = Color.white;
                }
                else
                {
                    GUILayout.Label(_selectedPrefabs[index] ? _selectedPrefabs[index].name : "No Prefab", EditorStyles.helpBox, GUILayout.Width(previewSize - 25));
                }
                if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    ShowAddMenu(index);
                }
                EditorGUILayout.EndHorizontal();

                if (previewRect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        _isStopped = false;
                        _onPreviewStarted?.Invoke(); 
                        _activePreview = index;
                        FXRenderer.CreatePreviewInstance(index, _selectedPrefabs, ref _previewInstance, ref _particleSystem);
                        if (_particleSystem != null)
                        {
                            _particleSystem.Play();
                        }

                        _isTrailMode = false;
                    }

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 2)
                    {
                        _isDragging = true;
                    }
                }

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        HandleMouseInput();
#endif
    }

    private bool HasBrokenTextures(GameObject prefab)
    {
        ParticleSystemRenderer[] renderers = prefab.GetComponentsInChildren<ParticleSystemRenderer>(true);
        foreach (ParticleSystemRenderer renderer in renderers)
        {
            if (renderer.renderMode == ParticleSystemRenderMode.None)
            {
                continue;
            }

            if (renderer.sharedMaterial == null || renderer.sharedMaterial.name == "Missing (Material)")
            {
                return true;
            }
        }
        return false;
    }

    private void ShowAddMenu(int index)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Add to Scene"), false, () => PrefabAdder.AddPrefabToScene(_selectedPrefabs[index]));
        menu.AddItem(new GUIContent("Add to Selected"), false, () => PrefabAdder.AddPrefabToSelected(_selectedPrefabs[index]));
        menu.ShowAsContext();
    }

    public void HandleMouseInput()
    {
        Event e = Event.current;

        if (_isDragging)
        {
            if (e.type == EventType.MouseUp && e.button == 2)
            {
                _isDragging = false;
            }

            if (e.type == EventType.MouseDrag && e.button == 2)
            {
                float rotationX = e.delta.x * _rotationSpeed * 0.02f;
                float rotationY = e.delta.y * _rotationSpeed * 0.02f;

                _previewCamera.transform.RotateAround(_cameraTarget, Vector3.up, rotationX);
                _previewCamera.transform.RotateAround(_cameraTarget, _previewCamera.transform.right, -rotationY);

                e.Use();
            }
        }

        if (_activePreview != -1 && _activePreviewRect.Contains(Event.current.mousePosition))
        {
            if (e.type == EventType.ScrollWheel)
            {
                _distance -= e.delta.y * _scrollSpeed * 0.1f;
                Vector3 direction = _previewCamera.transform.forward;
                _previewCamera.transform.position = _cameraTarget - direction * _distance;
                e.Use();
            }
        }
    }

    public void UpdatePreview()
    {
#if UNITY_EDITOR
        if (_activePreview != -1 && _previewCamera != null && _previewInstance != null && !_isStopped)
        {
            _previewCamera.targetTexture = _renderTextures[_activePreview];
            _previewCamera.Render();

            if (_particleSystem != null && !_particleSystem.IsAlive(true))
            {
                _particleSystem.Play();
            }
        }

        if (_isTrailMode && _trailMover != null)
        {
            _trailMover.Update();
        }
#endif
    }

    public bool ShouldRepaint()
    {
        return _activePreview != -1 && _previewInstance != null;
    }

    public void UpdatePrefabCount(int count)
    {
        _selectedPrefabs = new GameObject[count];
        _renderTextures = new RenderTexture[count];
        _previewTextures = new Texture2D[count];

        for (int i = 0; i < count; i++)
        {
            _renderTextures[i] = new RenderTexture(256, 256, 16);
            _previewTextures[i] = null;
        }
    }

    public void UpdateBackgroundColor(Color color)
    {
        _backgroundColor = color;
        _previewCamera.backgroundColor = color;
    }

    public void StopPreview()
    {
        _isStopped = true;
        if (_previewInstance != null)
        {
            GameObject.DestroyImmediate(_previewInstance);
        }
    }

    public void StartTrailMode(float speed, float radius)
    {
        if (_previewInstance != null)
        {
            _trailMover = new TrailMover(_previewInstance, speed, radius);
            _isTrailMode = true;
        }
    }

    public void StopTrailMode()
    {
        if (_previewInstance != null)
        {
            _trailMover = null;
            _previewInstance.transform.position = Vector3.zero;
            _isTrailMode = false;
        }
    }

    public void UpdateTrailModeSettings(float speed, float radius)
    {
        if (_trailMover != null)
        {
            _trailMover.SetSpeed(speed);
            _trailMover.SetRadius(radius);
        }
    }
}
