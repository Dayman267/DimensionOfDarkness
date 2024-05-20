using UnityEngine;
using UnityEngine.UI;

public class SetStartButtonActive : MonoBehaviour
{
    public Button bt;
    private void OnEnable()
    {
        IsCollidedWithPlayer.onTriggeredWithPlayer += SetButtonActive;
        IsCollidedWithPlayer.onPlayerTriggerExit += SetButtonNonactive;
    }

    private void OnDisable()
    {
        IsCollidedWithPlayer.onTriggeredWithPlayer -= SetButtonActive;
        IsCollidedWithPlayer.onPlayerTriggerExit -= SetButtonNonactive;
    }

    private void SetButtonActive()
    {
        bt.gameObject.SetActive(true);
    }

    private void SetButtonNonactive()
    {
        bt.gameObject.SetActive(false);
    }
}
