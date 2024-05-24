using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string startGameScene = "MainScene";
    [SerializeField] private string hubScene = "Hub";
    [SerializeField] private string mainMenuScene = "MainMenu";

    public void StartGame()
    {
        LoadingScreen.SwitchScene(startGameScene);
    }

    public void EnterTheHub()
    {
        LoadingScreen.SwitchScene(hubScene);
    }

    public void ReturnToMainMenu()
    {
        LoadingScreen.SwitchScene(mainMenuScene);
    }
}
