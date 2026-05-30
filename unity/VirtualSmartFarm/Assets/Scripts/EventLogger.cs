using System.Collections.Generic;
using UnityEngine;

public class EventLogger : MonoBehaviour
{
    public static EventLogger Instance;
    public Queue<string> logs = new Queue<string>();
    public int maxLogs = 6;

    void Awake() { Instance = this; }

    public void Log(string message)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        string entry = $"[{timestamp}] {message}";
        logs.Enqueue(entry);
        while (logs.Count > maxLogs) logs.Dequeue();
        Debug.Log(entry);
    }
}
