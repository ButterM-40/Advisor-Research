using UnityEngine;
using Claudia;
using System;

public class ClaudeTest1 : MonoBehaviour
{
    private Anthropic anthropic;

    private void Awake()
    {
        EnvLoader.Load(); // Load environment variables
        string apiKey = System.Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
        
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API Key not found in environment variables!");
            return;
        }

        anthropic = new Anthropic
        {
            ApiKey = apiKey
        };
    }

    public async void GeneratePrompt()
    {
        if (anthropic == null)
        {
            Debug.LogError("Anthropic not initialized!");
            return;
        }

        try
        {
            var message = await anthropic.Messages.CreateAsync(new()
            {
                Model = "claude-3-sonnet-20240229",
                MaxTokens = 1024,
                Messages = new Message[] { new Message { Role = "user", Content = "What is the capital of France?" } }
            });
            Debug.Log(message.Content[0].Text);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error generating response: {e.Message}");
        }
    }
}

