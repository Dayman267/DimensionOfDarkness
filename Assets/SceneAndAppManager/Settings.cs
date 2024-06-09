using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public TMP_Dropdown resolutionDropDown;
    public TMP_Dropdown qualityDropDown;

    private Resolution[] resolutions;
    
    private void Start()
    {
        resolutionDropDown.ClearOptions();
        List<string> options = new List<string>();
        resolutions = Screen.resolutions;
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width}\u00d7{resolutions[i].height} {resolutions[i].refreshRateRatio}Hz";
            options.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }
        
        resolutionDropDown.AddOptions(options);
        resolutionDropDown.RefreshShownValue();
        resolutionDropDown.value = currentResolutionIndex;
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
        PlayerPrefs.SetInt("FullScreen", Convert.ToInt32(Screen.fullScreen));
    }

    // public void SetVSinc(bool isVSincOn)
    // {
    //     
    // }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("Resolution", resolutionDropDown.value);
    }

    public void SetQuality(int qualityIndex)
    {
        switch (qualityIndex)
        {
            case 0: 
                QualitySettings.SetQualityLevel(5);
                break;
            case 1: 
                QualitySettings.SetQualityLevel(3);
                break;
            case 2: 
                QualitySettings.SetQualityLevel(2);
                break;
            case 3: 
                QualitySettings.SetQualityLevel(1);
                break;
        }
        
        PlayerPrefs.SetInt("Quality", qualityDropDown.value);
    }

    public void LoadSettings(int currentResolutionIndex)
    {
        if (PlayerPrefs.HasKey("Quality")) qualityDropDown.value = PlayerPrefs.GetInt("Quality");
        else qualityDropDown.value = 0;

        if (PlayerPrefs.HasKey("Resolution")) resolutionDropDown.value = PlayerPrefs.GetInt("Resolution");
        else resolutionDropDown.value = currentResolutionIndex;

        if (PlayerPrefs.HasKey("FullScreen"))
            Screen.fullScreen = Convert.ToBoolean(PlayerPrefs.GetInt("FullScreen"));
        else Screen.fullScreen = true;
    }
}
