using UnityEngine;

public class SoundController : MonoBehaviour
{
    [SerializeField] private AudioSource audio;
    [SerializeField] private AudioClip[] stepSounds;

    private int currentIndex;


    public void FootStepWhileAiming()
    {
        IsStepSoundsAvailable();

        audio.pitch = 0.6f;
        PlayNextStepSound();
    }

    public void FootStepWhileRun()
    {
        IsStepSoundsAvailable();
        audio.pitch = 0.8f;
        PlayNextStepSound();
    }

    public void FootStepWhileSprint()
    {
        IsStepSoundsAvailable();
        audio.pitch = 1f;
        PlayNextStepSound();
    }

    public void PlayNextStepSound()
    {
        if (!GetComponent<AudioSource>().isPlaying)
        {
            var clipToPlay = stepSounds[currentIndex];
            audio.PlayOneShot(clipToPlay);
            currentIndex = (currentIndex + 1) % stepSounds.Length;
        }
    }

    public bool IsStepSoundsAvailable()
    {
        var isSoundsAvailable = stepSounds.Length > 0;

        if (!isSoundsAvailable) Debug.LogWarning("No step sounds available!");

        return isSoundsAvailable;
    }
}