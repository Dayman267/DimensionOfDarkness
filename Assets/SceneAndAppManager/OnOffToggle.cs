using UnityEngine;
using UnityEngine.UI;

public class OnOffToggle : MonoBehaviour
{
    void Start()
    {
        GetComponent<Toggle>().isOn = Screen.fullScreen;
    }
}
