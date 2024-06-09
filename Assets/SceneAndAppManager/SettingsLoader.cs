using UnityEngine;

public class SettingsLoader : MonoBehaviour
{
    private Settings settings;
    private Resolution[] resolutions;
    void Start()
    {
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }
        settings.LoadSettings(currentResolutionIndex);
    }
}
