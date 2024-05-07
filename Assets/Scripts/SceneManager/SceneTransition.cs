using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    public void StartGame()
    {
        LoadingScreen.SwitchScene("DimaScene");
    }
}
