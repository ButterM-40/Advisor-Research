using UnityEngine;
using TMPro;
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private float typewriterSpeed = 0.05f;
    private TextMeshProUGUI textComponent;
    private Coroutine typewriterCoroutine;

    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    public void StartTypewriterEffect(string textToType)
    {
        // Stop any existing typewriter effect
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        typewriterCoroutine = StartCoroutine(TypeText(textToType));
    }

    private IEnumerator TypeText(string text)
    {
        textComponent.text = "";
        
        // Loop through each character
        foreach (char c in text.ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }
}
