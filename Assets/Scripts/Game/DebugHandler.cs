using UnityEngine;
using System.Collections;

public class DebugHandler : MonoBehaviour {

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        Application.ExternalCall("ExternalDebug", logString, stackTrace, type.ToString());
    }

    /*void OnGUI()
    {
        myLog = GUI.TextArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20), myLog);
    }*/
}
