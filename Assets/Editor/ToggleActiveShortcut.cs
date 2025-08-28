using UnityEngine;
using UnityEditor;

public class ToggleActiveShortcut
{
    [MenuItem("Tools/Toggle Active _d")] // "_" = просто клавиша без модификаторов
    static void ToggleActive()
    {
        foreach (var obj in Selection.gameObjects)
        {
            Undo.RecordObject(obj, "Toggle Active");
            obj.SetActive(!obj.activeSelf);
        }
    }
}
