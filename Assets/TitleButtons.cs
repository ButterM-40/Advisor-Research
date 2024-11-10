using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleButtons : MonoBehaviour
{
    public void StartGame()
    {
        Debug.Log("Loading game...");
        SceneManager.LoadScene(1); // Loads scene index 1
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

}
