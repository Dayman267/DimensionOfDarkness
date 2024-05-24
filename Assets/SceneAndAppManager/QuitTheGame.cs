using UnityEditor;
using UnityEngine;

public class QuitTheGame : MonoBehaviour
{
    public void Quit()
    {
        #if !UNITY_EDITOR
            Application.Quit();
        #endif
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #endif
    }
}
