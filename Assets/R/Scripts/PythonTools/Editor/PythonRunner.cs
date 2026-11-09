using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEditor;

public class PythonRunner : EditorWindow
{
    [MenuItem("Tools/PythonTools")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        PythonRunner window = (PythonRunner)EditorWindow.GetWindow(typeof(PythonRunner));
        window.Show();
    }

    bool showReplaceTools = true;
    string replaceTarget = "";
    string originString = "";
    string replaceString = "";

    void OnGUI()
    {
        GUILayout.Label("Python tools", EditorStyles.boldLabel);

        showReplaceTools = EditorGUILayout.BeginToggleGroup("Replace", showReplaceTools);
        replaceTarget = EditorGUILayout.TextField("Target", replaceTarget);
        originString = EditorGUILayout.TextField("Find", originString);
        replaceString = EditorGUILayout.TextField("Replace", replaceString);

        if (GUILayout.Button("Replace"))
        {
            if (!string.IsNullOrEmpty(replaceTarget) && !string.IsNullOrEmpty(originString) && !string.IsNullOrEmpty(replaceString))
            {
                Replace(replaceTarget, originString, replaceString);
            }
            else
            {
                UnityEngine.Debug.Log("Cannot leave blank cell in form.");
            }
        }

        EditorGUILayout.EndToggleGroup();
    }

    public static void Replace(string target, string origin, string replacement)
    {
        string command = CommandFormatter(GetScriptPath("replace_tools.py"), target, QuotedString(origin), QuotedString(replacement));
        RunPython(command);
    }

    public static void RunPython(string argument)
    {
        Process myProcess = new Process();
        myProcess.StartInfo.FileName = "python";
        myProcess.StartInfo.Arguments = argument;
        myProcess.StartInfo.UseShellExecute = false;
        myProcess.StartInfo.RedirectStandardOutput = true;
        myProcess.StartInfo.RedirectStandardError = true;

        myProcess.Start();
        string stdout = myProcess.StandardOutput.ReadToEnd();
        string stderr = myProcess.StandardError.ReadToEnd();
        myProcess.WaitForExit();

        UnityEngine.Debug.Log(string.Format("Command: <color=blue>{0}</color>", argument));

        if (!string.IsNullOrEmpty(stdout))
        {
            UnityEngine.Debug.Log(stdout);
        }

        if (!string.IsNullOrEmpty(stderr))
        {
            UnityEngine.Debug.LogError(stderr);
        }

        if (string.IsNullOrEmpty(stdout) && string.IsNullOrEmpty(stderr))
        {
            UnityEngine.Debug.Log("Success without error.");
        }
    }

    public static string CommandFormatter(params string[] parameters)
    {
        string command = "";
        for (int i = 0; i < parameters.Length; i++)
        {
            command += parameters[i] + " ";
        }

        return command.TrimEnd(' ');
    }

    public static string GetScriptPath(string scriptName)
    {
        return string.Format("Assets/R/Scripts/PythonTools/Editor/PythonScript/" + scriptName);
    }

    public static string QuotedString(string input)
    {
        return string.Format("\'{0}\'", input);
    }
}