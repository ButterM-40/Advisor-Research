using System.IO;
using UnityEngine;

public static class EnvLoader
{
    public static void Load()
    {
        string path = Path.Combine(Application.dataPath, "../.env");
        if (!File.Exists(path))
        {
            Debug.LogError(".env file not found!");
            return;
        }

        foreach (string line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
            
            int equalSign = line.IndexOf('=');
            if (equalSign < 0) continue;
            
            string key = line.Substring(0, equalSign).Trim();
            string value = line.Substring(equalSign + 1).Trim();
            
            // Remove quotes if present
            if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                value = value.Substring(1, value.Length - 2);
            }
            
            System.Environment.SetEnvironmentVariable(key, value);
        }
    }
} 