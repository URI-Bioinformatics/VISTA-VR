using UnityEditor;
using UnityEngine;
using System.IO;

public class SequenceStepScriptCreator : EditorWindow
{
    private string className = "NewSequenceStep"; // Default class name

    [MenuItem("Assets/Create/Sequence Step Script", false, 80)]
    private static void ShowWindow()
    {
        SequenceStepScriptCreator window = GetWindow<SequenceStepScriptCreator>(true, "Create Sequence Step Script", true);
        window.maxSize = new Vector2(300, 100);
        window.minSize = window.maxSize;
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Enter Sequence Step Name", EditorStyles.boldLabel);
        className = EditorGUILayout.TextField("Class Name:", className);

        if (GUILayout.Button("Create"))
        {
            if (IsValidClassName(className))
            {
                CreateScript(className);
                Close();
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid Name", "Class name must be a valid C# identifier (no spaces or special characters).", "OK");
            }
        }
    }

    private static bool IsValidClassName(string name)
    {
        return !string.IsNullOrEmpty(name) && System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(name);
    }

    private static void CreateScript(string scriptName)
    {
        string path = GetSelectedPathOrFallback();
        string fullPath = Path.Combine(path, scriptName + ".cs");

        // Ensure unique filename
        int counter = 1;
        while (File.Exists(fullPath))
        {
            fullPath = Path.Combine(path, scriptName + counter + ".cs");
            counter++;
        }

        File.WriteAllText(fullPath, GetScriptTemplate(scriptName));

        // Refresh Unity's Asset Database
        AssetDatabase.Refresh();

        // Select and open the newly created script
        Object asset = AssetDatabase.LoadAssetAtPath(fullPath, typeof(Object));
        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
        AssetDatabase.OpenAsset(asset);
    }

    private static string GetSelectedPathOrFallback()
    {
        string path = "Assets";
        foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            string objPath = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(objPath) && Directory.Exists(objPath))
            {
                return objPath;
            }
        }
        return path;
    }

    private static string GetScriptTemplate(string scriptName)
    {
        return $@"using UnityEngine;

public class {scriptName} : SequenceStep
{{
    public override void InitializeStep()
    {{
        Debug.Log(""{scriptName} initialized."");
    }}

    public override void StepUpdate()
    {{
        Debug.Log(""{scriptName} updating."");
    }}

    public override void StepFixedUpdate()
    {{
        Debug.Log(""{scriptName} fixed updating."");
    }}

    public override void Cleanup()
    {{
        Debug.Log(""{scriptName} cleaned up."");
    }}
}}";
    }
}
