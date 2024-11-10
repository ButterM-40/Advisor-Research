using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClaudeAudioUI : MonoBehaviour
{
    [SerializeField] private Button recordButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI responseText;
    private ClaudeAudioHandler audioHandler;
    private bool isRecording = false;

    private void Start()
    {
        // Get or add the audio handler
        audioHandler = GetComponent<ClaudeAudioHandler>();
        if (audioHandler == null)
        {
            audioHandler = gameObject.AddComponent<ClaudeAudioHandler>();
        }

        // Setup UI
        if (recordButton != null)
        {
            recordButton.onClick.AddListener(ToggleRecording);
            recordButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start Recording";
        }

        if (statusText != null)
        {
            statusText.text = "Ready";
        }

        // Subscribe to Claude's response event
        audioHandler.OnResponseReceived += HandleClaudeResponse;
    }

    private void ToggleRecording()
    {
        if (!isRecording)
        {
            StartRecording();
        }
        else
        {
            StopRecording();
        }
    }

    private void StartRecording()
    {
        isRecording = true;
        Debug.Log("Started recording audio...");
        audioHandler.StartRecording();
        recordButton.GetComponentInChildren<TextMeshProUGUI>().text = "Stop Recording";
        statusText.text = "Recording...";
    }

    private void StopRecording()
    {
        isRecording = false;
        audioHandler.StopRecording();
        recordButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start Recording";
        statusText.text = "Processing...";
    }

    private void HandleClaudeResponse(string response)
    {
        statusText.text = "Ready";
        responseText.text = response;
    }
} 