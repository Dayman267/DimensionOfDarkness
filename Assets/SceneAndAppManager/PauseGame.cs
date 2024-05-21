using System;
using UnityEngine;

public class PauseGame : MonoBehaviour
{
    private PlayerActions playerActions;
    
    public static Action OnGamePaused;
    public static Action OnGameResumed;

    public GameObject pausePanel;
    public GameObject settingsMenu;
    public GameObject buttons;
    
    bool isGameplayMapActive = true;
    
    private void Awake()
    {
        playerActions = new PlayerActions();
        playerActions.Gameplay.Enable();
    }

    private void Update()
    {
        if (playerActions.Gameplay.ChangeActionMap.WasReleasedThisFrame())
        {
            if (isGameplayMapActive)
            {
                playerActions.Gameplay.Disable();
                playerActions.UI.Enable();

                isGameplayMapActive = false;
                
                pausePanel.SetActive(true);

                Time.timeScale = 0;
                
                Cursor.visible = true;
                
                OnGamePaused?.Invoke();
            }
        }

        if (playerActions.UI.ChangeActionMap.WasReleasedThisFrame())
        {
            if (settingsMenu.activeSelf)
            {
                buttons.SetActive(true);
                settingsMenu.SetActive(false);
            }
            else
            {
                Resume();
            }
        }
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        
        playerActions.UI.Disable();
        playerActions.Gameplay.Enable();
        isGameplayMapActive = true;
        
        Cursor.visible = false;

        Time.timeScale = 1;
        
        OnGameResumed?.Invoke();
    }
}
