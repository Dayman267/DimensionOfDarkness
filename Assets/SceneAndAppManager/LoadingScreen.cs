using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public TextMeshProUGUI LoadingScreenPercentage;
    public Image LoadingProgressBar;
    
    private static LoadingScreen instance;
    private static bool shouldPlayOpeningAnimation = false;
    
    private Animator componentAnimator;
    private AsyncOperation loadingScreenOperation;

    public static void SwitchScene(string sceneName)
    {
        instance.componentAnimator.SetTrigger("SceneClose");
        
        instance.loadingScreenOperation = SceneManager.LoadSceneAsync(sceneName);
        instance.loadingScreenOperation.allowSceneActivation = false;
    }

    private void Start()
    {
        instance = this;

        componentAnimator = GetComponent<Animator>();
        
        if(shouldPlayOpeningAnimation) componentAnimator.SetTrigger("SceneOpening");
    }

    private void Update()
    {
        if (loadingScreenOperation != null)
        {
            LoadingScreenPercentage.text = Mathf.Round(loadingScreenOperation.progress * 100) + "%";
            LoadingProgressBar.fillAmount = loadingScreenOperation.progress;
        }
    }

    public void OnAnimationOver()
    {
        shouldPlayOpeningAnimation = true;
        loadingScreenOperation.allowSceneActivation = true;
    }
}
