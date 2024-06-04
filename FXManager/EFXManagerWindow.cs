using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class EFXManagerWindow : EditorWindow
{
    private EFXPreviewHandler _previewHandler;
    private List<GameObject> _prefabs;
    private List<GameObject> _filteredPrefabs;
    private float _previewSize = 150f;
    private Vector2 _scrollPosition;
    private Texture2D _headerImage;
    private Color _backgroundColor = Color.black;
    private bool _showGrid = true;
    private bool _showInstructions = false;
    private bool _showSettings = false;
    private string _searchString = "";
    private bool _isStopped = false;
    private bool _isTrailMode = false;
    private float _trailSpeed = 1.0f;
    private float _trailRadius = 2.0f;
    GUIStyle buttonStyle;

    [MenuItem("Escripts/FXManager")]
    public static void ShowWindow()
    {
        GetWindow<EFXManagerWindow>("FXManager");
    }

    private void OnEnable()
    {
        _previewHandler = new EFXPreviewHandler(_backgroundColor, OnPreviewStarted);
        _headerImage = Resources.Load<Texture2D>("FXManager");
        EditorApplication.update += UpdateWindow;
        EditorApplication.update += _previewHandler.UpdatePreview;
        EFXPrefabManagerWindow.OnPrefabsUpdated += UpdatePrefabsList;
        FetchPrefabsList();
    }

    private void OnDisable()
    {
        if (_previewHandler != null)
        {
            _previewHandler.OnDisable();
        }
        EditorApplication.update -= UpdateWindow;
        EditorApplication.update -= _previewHandler.UpdatePreview;
        EFXPrefabManagerWindow.OnPrefabsUpdated -= UpdatePrefabsList;
    }

    private void OnGUI()
    {
        DrawBackground();
        DrawGrid(20, 0.05f, Color.gray);
        DrawGrid(100, 0.1f, Color.gray);

        EditorGUILayout.Space(15);
        if (_headerImage != null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.BeginVertical();
            GUILayout.Label(_headerImage, GUILayout.Width(200), GUILayout.Height(35));
            
            GUILayout.BeginHorizontal();
            string newSearchString = GUILayout.TextField(_searchString, "SearchTextField", GUILayout.Width(200));
            if (newSearchString != _searchString)
            {
                _searchString = newSearchString;
                FilterPrefabs();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 10,
                normal = { textColor = Color.white, background = MakeTex(2, 2, new Color(0.25f, 0.25f, 0.25f)) },
                hover = { background = MakeTex(2, 2, new Color(0.35f, 0.35f, 0.35f)) },
                active = { background = MakeTex(2, 2, new Color(0.45f, 0.45f, 0.45f)) }
            };

            if (GUILayout.Button("SET", buttonStyle, GUILayout.Height(20), GUILayout.Width(40)))
            {
                EFXPrefabManagerWindow.ShowWindow();
            }

            GUILayout.Space(5);

            if (GUILayout.Button(new GUIContent("?", "Show Instructions"), buttonStyle, GUILayout.Height(20), GUILayout.Width(20)))
            {
                _showInstructions = !_showInstructions;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            if (GUILayout.Button(new GUIContent("Settings", "Show Preview Settings"), buttonStyle, GUILayout.Height(20), GUILayout.Width(68)))
            {
                _showSettings = !_showSettings;
            }
            GUILayout.EndVertical();
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            if (_showSettings)
            {
                GUILayout.Space(15);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical(GUILayout.Width(150));
                GUILayout.Label("Preview settings", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(150));
                _previewSize = EditorGUILayout.Slider(_previewSize, 100f, 512f, GUILayout.Width(150));
                EditorGUILayout.BeginHorizontal();
                _backgroundColor = EditorGUILayout.ColorField(_backgroundColor, GUILayout.Width(107));
                _previewHandler.UpdateBackgroundColor(_backgroundColor);

                if (GUILayout.Button(_showGrid ? "Grid" : "Grid", buttonStyle, GUILayout.Width(40)))
                {
                    _showGrid = !_showGrid;
                    _previewHandler.SetGridVisibility(_showGrid);
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(15);

                GUILayout.Label("Trail Mode Settings", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(150));
                GUILayout.Label("speed", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(150));
                float newTrailSpeed = EditorGUILayout.Slider(_trailSpeed, 0.1f, 10f, GUILayout.Width(150));
                if (newTrailSpeed != _trailSpeed)
                {
                    _trailSpeed = newTrailSpeed;
                    if (_isTrailMode) _previewHandler.UpdateTrailModeSettings(_trailSpeed, _trailRadius);
                }

                GUILayout.Label("radius", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(150));
                float newTrailRadius = EditorGUILayout.Slider(_trailRadius, 0.1f, 10f, GUILayout.Width(150));
                if (newTrailRadius != _trailRadius)
                {
                    _trailRadius = newTrailRadius;
                    if (_isTrailMode) _previewHandler.UpdateTrailModeSettings(_trailSpeed, _trailRadius);
                }

                EditorGUILayout.EndVertical();
                GUILayout.Space(15);
                GUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.BeginVertical();

        EditorGUILayout.Space(10);

        if (_showInstructions)
        {
            EditorGUILayout.HelpBox(
                "Instructions:\n" +
                "FX Prefab Manager:\n" +
                "1. Add prefabs to the manager by dragging and dropping them into the separate FX Prefab Manager window.\n" +
                "2. The FX Prefab Manager window lists all the added prefabs.\n" +
                "3. A ScriptableObject holds the list of all prefabs in the assets and will be created at /Assets/.\n" +
                "    Then you can move it wherever you want.\n" +
                "\n" +
                "Preview Settings:\n" +
                "1. Preview Size slider changes the size of the prefab previews.\n" +
                "2. Background Color picker changes the background color of the preview area.\n" +
                "3. Hide Grid button toggles the visibility of the grid in the preview area.\n" +
                "4. Trail Mode settings adjust the speed and radius of the trail effect.\n" +
                "\n" +
                "Preview Controls:\n" +
                "1. Left Mouse Button (LMB) on a prefab to play its effect.\n" +
                "2. Middle Mouse Button (MMB) to rotate the view around the effect.\n" +
                "3. Scroll Wheel to zoom in and out of the preview area.\n" +
                "\n" +
                "Scene Controls:\n" +
                "1. 'Add to Scene' button adds the prefab to the scene at the origin.\n" +
                "2. 'Add to Selected' button adds the prefab to the currently selected object in the scene.\n",
                MessageType.Info);
        }

        GUILayout.Space(10);

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        int columns = Mathf.FloorToInt(position.width / _previewSize);

        for (int i = 0; i < _filteredPrefabs?.Count; i += columns)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < columns; j++)
            {
                int index = i + j;
                if (index < _filteredPrefabs.Count)
                {
                    _previewHandler.SetPrefab(index, _filteredPrefabs[index]);
                }
                else
                {
                    break;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        _previewHandler.OnGUI(_previewSize);
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();

        _previewHandler.HandleMouseInput();

        EditorGUILayout.BeginHorizontal();

        buttonStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            normal = { textColor = _isTrailMode ? Color.red : Color.gray }
        };
        string trailButtonText = _isTrailMode ? "Trail Mode ON" : "Trail Mode OFF";

        if (GUILayout.Button(trailButtonText, buttonStyle, GUILayout.Height(20), GUILayout.Width(80)))
        {
            _isTrailMode = !_isTrailMode;
            if (_isTrailMode)
            {
                _previewHandler.StartTrailMode(_trailSpeed, _trailRadius);
            }
            else if (_isStopped)
            {
                _previewHandler.StopTrailMode();
            }
            else
            {
                _previewHandler.StopTrailMode();
            }
        }

        GUILayout.FlexibleSpace();

        buttonStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            normal = { textColor = Color.red }
        };
        string buttonText = _isStopped ? "Stopped" : "Stop Playing";

        if (GUILayout.Button(buttonText, buttonStyle, GUILayout.Height(20), GUILayout.Width(80)))
        {
            if (!_isStopped)
            {
                _previewHandler.StopPreview();
                _isStopped = true;
                _isTrailMode = false;
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private void OnPreviewStarted()
    {
        _isStopped = false;
    }

    private void FilterPrefabs()
    {
        if (string.IsNullOrEmpty(_searchString))
        {
            _filteredPrefabs = new List<GameObject>(_prefabs);
        }
        else
        {
            _filteredPrefabs = _prefabs.FindAll(prefab => prefab.name.IndexOf(_searchString, System.StringComparison.OrdinalIgnoreCase) >= 0);
        }
        _previewHandler.UpdatePrefabCount(_filteredPrefabs.Count);
    }

    private void UpdateWindow()
    {
        if (_previewHandler != null && _previewHandler.ShouldRepaint())
        {
            Repaint();
        }
    }

    private void FetchPrefabsList()
    {
        _prefabs = EFXPrefabManagerWindow.GetPrefabs();
        FilterPrefabs();
    }

    private void UpdatePrefabsList(List<GameObject> prefabs)
    {
        _prefabs = prefabs;
        FilterPrefabs();
    }

    private void DrawBackground()
    {
        Color backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), backgroundColor);
    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, 0, 0), new Vector3(gridSpacing * i, position.height, 0));
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(0, gridSpacing * j, 0), new Vector3(position.width, gridSpacing * j, 0));
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }
}
