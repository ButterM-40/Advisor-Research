using UnityEngine;
using TMPro;
using Claudia;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Linq;

public class ClaudePromptScript : MonoBehaviour
{
    private Anthropic anthropic;
    private string professorContext;
    private List<(string role, string content)> messageHistory = new List<(string role, string content)>();
    private StringBuilder displayHistory = new StringBuilder();
    private TypewriterEffect typewriterEffect;

    [Header("Professor Information")]
    [TextArea(3, 5)]
    public string professorName = "Dr. Sarah Mitchell";
    [TextArea(3, 5)]
    public string researchArea = "Specializes in Quantum Computing and Machine Learning, with a focus on developing quantum algorithms for AI applications";
    [TextArea(3, 5)]
    public string hobbies = "Enjoys hiking, playing classical piano, and participating in science outreach programs for high school students";
    [TextArea(3, 5)]
    public string personalBackground = "Originally from Boston, completed PhD at MIT, has been teaching for 15 years and mentored over 50 graduate students";

    [Header("Event Details")]
    [TextArea(3, 5)]
    public string eventDescription = "Annual Department Research Symposium";
    [TextArea(3, 5)]
    public string eventDetails = "Presenting latest findings in quantum machine learning algorithms, followed by a Q&A session and networking reception";
    [TextArea(3, 5)]
    public string eventDateTime = "March 15th, 2024 at 2:00 PM";
    [TextArea(3, 5)]
    public string eventLocation = "Science Building, Room 305";

    [Header("UI References")]
    [SerializeField] private TMP_InputField userInputField;
    [SerializeField] private TMP_Text responseText;
    [SerializeField] private TMP_Text historyText;
    [SerializeField] private Button conclusionButton;
    [SerializeField] private Button continueButton;
    private string mostRecentResponse = "";
    private Anthropic verificationAnthropic;

    private void Start()
    {
        typewriterEffect = responseText.gameObject.GetComponent<TypewriterEffect>();
        if (typewriterEffect == null)
        {
            typewriterEffect = responseText.gameObject.AddComponent<TypewriterEffect>();
        }

        // Initialize both API instances
        InitializeAPI();
        InitializeVerificationAPI();

        // Initialize button state
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        // Generate the professor context once at start
        professorContext = GenerateInitialContext();
    }

    private void InitializeAPI()
    {
        try
        {
            EnvLoader.Load();
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
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize Claude API: {e.Message}");
        }
    }

    private void InitializeVerificationAPI()
    {
        try
        {
            EnvLoader.Load();
            string apiKey = System.Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
            
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogError("API Key not found for verification API!");
                return;
            }

            verificationAnthropic = new Anthropic
            {
                ApiKey = apiKey
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize verification API: {e.Message}");
        }
    }

    private string GenerateInitialContext()
    {
        return $@"You are {professorName}, a Professor at UTRGV. 

Background Information (Only use when relevant to event questions):
- Research Focus: {researchArea}
- Academic Background: {personalBackground}
- Personal Interests: {hobbies}

Current Situation:
- You are actively at: {eventDescription}
- Current Activity: {eventDetails}
- Time and Location: {eventDateTime} at {eventLocation}
- You are taking questions during this presentation

Role-Playing Instructions:
1. Respond as {professorName.Split(' ')[1]} in first person
2. You are currently AT the event, speaking in present tense
3. Keep all responses brief and concise (2-3 sentences maximum)
4. Focus ONLY on questions related to:
   - Your current research presentation
   - The ongoing {eventDescription}
   - Technical questions about your research
5. For off-topic questions:
   - Briefly redirect: 'Let's focus on today's presentation. Do you have any questions about my current research?'
6. Be professional but engaging as an in person professor not email or chatbot nor say Speaking as Professor

Remember: 
- You are actively presenting at {eventDescription}
- Keep responses short and to the point around 2 sentences
- Stay focused on {researchArea}

Previous Conversation Context:
{(messageHistory.Count > 0 ? "\nPrevious Conversation:" : "")}";
    }

    private string GeneratePrompt()
    {
        StringBuilder prompt = new StringBuilder();
        
        // First add the professor's context
        prompt.Append(professorContext);

        // Then add conversation history if any exists
        if (messageHistory.Count > 0)
        {
            prompt.AppendLine("\nPrevious Conversation:");
            foreach (var msg in messageHistory)
            {
                prompt.AppendLine($"{msg.role}: {msg.content}");
            }
        }

        return prompt.ToString();
    }

    public async void OnSendButton()
    {
        if (anthropic == null || userInputField == null || responseText == null)
        {
            Debug.LogError("Required components not initialized");
            return;
        }

        if (string.IsNullOrEmpty(userInputField.text)) return;

        string userInput = userInputField.text;
        AddToHistory("User", userInput);
        
        try
        {
            var contextPrompt = GeneratePrompt();
            
            var messagesList = new List<Message>();
            foreach (var msg in messageHistory)
            {
                if (msg.role == "User")
                {
                    messagesList.Add(new Message { Role = "user", Content = msg.content });
                }
                else if (msg.role == "Assistant")
                {
                    messagesList.Add(new Message { Role = "assistant", Content = msg.content });
                }
            }

            var response = await anthropic.Messages.CreateAsync(new()
            {
                Model = "claude-3-sonnet-20240229",
                MaxTokens = 1024,
                System = contextPrompt,
                Messages = messagesList.ToArray()
            });

            string assistantResponse = response.Content[0].Text;
            mostRecentResponse = assistantResponse;
            AddToHistory("Assistant", assistantResponse);
            
            // Check for event resolution after each response
            bool isResolved = await VerifyEventResolution();
            Debug.Log($"Event resolved: {isResolved}");
            
            typewriterEffect.StartTypewriterEffect(assistantResponse);
            userInputField.text = "";
        }
        catch (Exception e)
        {
            Debug.LogError($"Error: {e.Message}");
            typewriterEffect.StartTypewriterEffect($"Error occurred: {e.Message}");
        }
    }

    private void AddToHistory(string role, string content)
    {
        messageHistory.Add((role, content));
        
        displayHistory.Clear();
        foreach (var message in messageHistory)
        {
            displayHistory.AppendLine($"{message.role}: {message.content}");
            displayHistory.AppendLine("-------------------");
        }
        
        if (historyText != null)
        {
            historyText.text = displayHistory.ToString();
        }
    }

    public async Task<bool> VerifyEventResolution()
    {
        if (string.IsNullOrEmpty(mostRecentResponse))
        {
            Debug.LogWarning("No response to verify");
            return false;
        }

        try
        {
            StringBuilder historyString = new StringBuilder();
            foreach (var msg in messageHistory)
            {
                historyString.AppendLine($"{msg.role}: {msg.content}");
            }

            var response = await verificationAnthropic.Messages.CreateAsync(new()
            {
                Model = "claude-3-sonnet-20240229",
                MaxTokens = 1024,
                System = GenerateResolutionVerificationPrompt(),
                Messages = new Message[] 
                { 
                    new Message 
                    { 
                        Role = "user", 
                        Content = $"Event Context:\n{eventDescription}\n{eventDetails}\n\nConversation History:\n{historyString}\n\nMost Recent Response:\n{mostRecentResponse}" 
                    }
                }
            });

            string verificationResult = response.Content[0].Text.ToLower().Trim();
            bool isResolved = verificationResult.Contains("yes");

            if (isResolved)
            {
                ShowContinueButton();
            }

            return isResolved;
        }
        catch (Exception e)
        {
            Debug.LogError($"Verification error: {e.Message}");
            return false;
        }
    }

    private void ShowContinueButton()
    {
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            // Optional: Add animation or effects
            Debug.Log("Event resolved - Continue button shown");
        }
    }

    public void OnContinueButtonClick()
    {
        Debug.Log("Loading next scene...");
        SceneManager.LoadScene(1);
    }

    private string GenerateResolutionVerificationPrompt()
    {
        return @"You are a verification system analyzing if an academic event's conflict or discussion has reached a positive conclusion.

ANALYSIS CRITERIA:
1. Has the main concern/question been fully addressed?
2. Did the professor provide a satisfactory explanation or solution?
3. Is there a clear sense of understanding between all parties?
4. Has the conversation reached a natural, positive conclusion?
5. Are there any unresolved issues or remaining concerns?

RESPONSE RULES:
- Respond ONLY with 'yes' if ALL of the following are true:
  * The main topic/concern has been fully addressed
  * The conversation has reached a clear, positive conclusion
  * No significant questions remain unanswered
  * The interaction ends on a constructive note

- Respond with 'no' if ANY of the following are true:
  * The main topic/concern remains partially or fully unaddressed
  * There are outstanding questions or concerns
  * The conversation feels incomplete or unresolved
  * The interaction needs further discussion

Respond with ONLY 'yes' or 'no'.";
    }

    // Optional: Method to handle conclusion button click
    public void OnConclusionButtonClick()
    {
        // Handle what happens when the conclusion button is clicked
        // For example, load a new scene or trigger end-of-event actions
        Debug.Log("Event concluded successfully!");
    }
}
