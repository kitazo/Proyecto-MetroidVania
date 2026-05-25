using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class FileLogger : MonoBehaviour
{
    private static string logPath;
    private static bool initialized = false;

    //Mensajes a ignorar 
    private static readonly HashSet<string> filteredMessages = new HashSet<string>
    {
        "There are 2 audio listeners in the scene. Please ensure there is always exactly one audio listener in the scene."
    };

    void Awake()
    {
        if (initialized) { Destroy(gameObject); return; }

        initialized = true;
        DontDestroyOnLoad(gameObject);

        //Nombre único por proceso: evita que Host y Cliente sobreescriban el mismo archivo
        string processId = System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
        logPath = Path.Combine(Application.persistentDataPath, $"game_log_{processId}.txt");

        File.WriteAllText(logPath, $"=== SESSION START: {DateTime.Now} | PID: {processId} ===\n");
        File.AppendAllText(logPath, $"Ruta: {logPath}\n\n");

        Application.logMessageReceived += OnUnityLog;

        Write("FileLogger listo.");
        Write($"Plataforma: {Application.platform}");
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= OnUnityLog;
        initialized = false;
    }

    private void OnUnityLog(string message, string stackTrace, LogType type)
    {
        //Filtra mensajes de spam conocidos
        if (filteredMessages.Contains(message)) return;

        string prefix = type switch
        {
            LogType.Error     => "[ERROR]",
            LogType.Assert    => "[ASSERT]",
            LogType.Warning   => "[WARN]",
            LogType.Exception => "[EXCEPTION]",
            _                 => "[INFO]"
        };

        AppendToFile($"{prefix} {message}");

        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            if (!string.IsNullOrEmpty(stackTrace))
                AppendToFile($"  >> {stackTrace.Trim()}");
        }
    }

    public static void Write(string message) => AppendToFile($"[LOG] {message}");

    private static void AppendToFile(string line)
    {
        try
        {
            File.AppendAllText(logPath, $"[{DateTime.Now:HH:mm:ss.fff}] {line}\n");
        }
        catch { }
    }
}