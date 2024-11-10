using UnityEngine;
using System;
using System.IO;
using Claudia;
using UnityEngine.UI; // If you want to add UI elements

public class ClaudeAudioHandler : MonoBehaviour
{
    private AudioClip recordedClip;
    private bool isRecording = false;
    private string micDevice;
    private Anthropic anthropic;
    
    [SerializeField] private float maxRecordingTime = 30f; // Maximum recording duration in seconds
    [SerializeField] private int recordingFrequency = 44100;
    
    public event System.Action<string> OnResponseReceived;

    private void Start()
    {
        // Initialize microphone and Claude
        if (Microphone.devices.Length > 0)
        {
            micDevice = Microphone.devices[0];
            Debug.Log($"Using microphone: {micDevice}");
        }
        else
        {
            Debug.LogError("No microphone found!");
            return;
        }

        EnvLoader.Load();
        string apiKey = System.Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API Key not found in environment variables!");
            return;
        }

        anthropic = new Anthropic { ApiKey = apiKey };
    }

    public void StartRecording()
    {
        if (isRecording) return;
        
        Debug.Log("Starting recording...");
        recordedClip = Microphone.Start(micDevice, false, (int)maxRecordingTime, recordingFrequency);
        isRecording = true;
    }

    public void StopRecording()
    {
        if (!isRecording) return;

        Microphone.End(micDevice);
        isRecording = false;
        Debug.Log("Recording stopped");

        // Convert AudioClip to WAV and send to Claude
        SendAudioToClaudeAsync();
    }

    private async void SendAudioToClaudeAsync()
    {
        try
        {
            // Convert AudioClip to WAV bytes
            byte[] wavBytes = AudioClipToWav(recordedClip);
            string base64Audio = Convert.ToBase64String(wavBytes);

            // Create the message request
            var message = await anthropic.Messages.CreateAsync(new()
            {
                Model = "claude-3-5-sonnet-20240620",
                MaxTokens = 1024,
                Messages = new Message[] 
                { 
                    new Message 
                    { 
                        Role = "user", 
                        Content = $"Here's my audio message in base64 format: {base64Audio}. Please transcribe and respond to it."
                    } 
                }
            });

            Debug.Log($"Claude's response: {message}");
            OnResponseReceived?.Invoke(message.Content.ToString());
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error sending audio to Claude: {ex.Message}");
            OnResponseReceived?.Invoke($"Error: {ex.Message}");
        }
    }

    private byte[] AudioClipToWav(AudioClip clip)
    {
        using (var memoryStream = new MemoryStream())
        {
            // Get the audio data from the clip
            float[] samples = new float[clip.samples];
            clip.GetData(samples, 0);

            // Convert to 16-bit PCM
            byte[] pcmData = new byte[samples.Length * 2];
            int pcmOffset = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                short value = (short)(samples[i] * short.MaxValue);
                byte[] bytes = BitConverter.GetBytes(value);
                pcmData[pcmOffset++] = bytes[0];
                pcmData[pcmOffset++] = bytes[1];
            }

            // Write WAV header
            uint headerSize = 44;
            uint fileSize = (uint)(pcmData.Length + headerSize - 8);
            uint dataSize = (uint)pcmData.Length;

            // RIFF header
            memoryStream.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"), 0, 4);
            memoryStream.Write(BitConverter.GetBytes(fileSize), 0, 4);
            memoryStream.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"), 0, 4);

            // Format chunk
            memoryStream.Write(System.Text.Encoding.ASCII.GetBytes("fmt "), 0, 4);
            memoryStream.Write(BitConverter.GetBytes(16), 0, 4); // Subchunk1Size
            memoryStream.Write(BitConverter.GetBytes((short)1), 0, 2); // AudioFormat (PCM)
            memoryStream.Write(BitConverter.GetBytes((short)1), 0, 2); // NumChannels (Mono)
            memoryStream.Write(BitConverter.GetBytes(recordingFrequency), 0, 4); // SampleRate
            memoryStream.Write(BitConverter.GetBytes(recordingFrequency * 2), 0, 4); // ByteRate
            memoryStream.Write(BitConverter.GetBytes((short)2), 0, 2); // BlockAlign
            memoryStream.Write(BitConverter.GetBytes((short)16), 0, 2); // BitsPerSample

            // Data chunk
            memoryStream.Write(System.Text.Encoding.ASCII.GetBytes("data"), 0, 4);
            memoryStream.Write(BitConverter.GetBytes(dataSize), 0, 4);
            memoryStream.Write(pcmData, 0, pcmData.Length);

            return memoryStream.ToArray();
        }
    }
} 