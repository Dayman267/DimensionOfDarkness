using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    public Image imageFade;
    public TextMeshProUGUI deathText;
    public Button returnToHubButton;
    
    private void StartFadeCoroutine()
    {
        StartCoroutine(StartFade());
    }
    
    private IEnumerator StartFade()
    {
        Color colorImageFade = imageFade.color;
        Color colorDeathText = deathText.color;
        while (colorImageFade.a < 1)
        {
            colorImageFade.a += 0.001f;
            colorDeathText.a += 0.001f;
            imageFade.color = colorImageFade;
            deathText.color = colorDeathText;
            yield return null;
        }
        yield return new WaitForSeconds(3);

        returnToHubButton.onClick.Invoke();
    }

    private void OnEnable()
    {
        PlayerHealth.onPlayerDead += StartFadeCoroutine;
    }

    private void OnDisable()
    {
        PlayerHealth.onPlayerDead -= StartFadeCoroutine;
    }
}
