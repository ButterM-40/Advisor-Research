using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void TitleScreen()
    {
        Debug.Log("Loading Title Screen");
        SceneManager.LoadScene(0);
    }

    public void Tomai()
    {
        Debug.Log("Loading Professor Tomai's Scene");
        SceneManager.LoadScene(1);
    }

    public void Schweller()
    {
        Debug.Log("Loading Professor Schweller's Scene");
        SceneManager.LoadScene(2);
    }

    public void Ayati()
    {
        Debug.Log("Loading Professor Ayati's Scene");
        SceneManager.LoadScene(3);
    }

    public void Austin()
    {
        Debug.Log("Loading Professor Austin's Scene");
        SceneManager.LoadScene(4);
    }

    public void Wylie()
    {
        Debug.Log("Loading Professor Wylie's Scene");
        SceneManager.LoadScene(5);
    }
} 