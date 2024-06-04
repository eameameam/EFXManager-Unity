using UnityEngine;
using UnityEditor;

public class ColorPickerWindow : EditorWindow
{
    private static Color _color;
    private static EFXPreviewHandler _handler;

    public static void ShowColorPicker(ref Color color, EFXPreviewHandler handler)
    {
        _color = color;
        _handler = handler;
        ColorPickerWindow window = ScriptableObject.CreateInstance<ColorPickerWindow>();
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 200, 100);
        window.ShowPopup();
    }

    private void OnGUI()
    {
        _color = EditorGUILayout.ColorField("Background Color", _color);
        if (GUILayout.Button("Apply"))
        {
            _handler.UpdateBackgroundColor(_color);
            Close();
        }
    }
}
