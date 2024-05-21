using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SoundController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] stepSounds;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void FootStepWhileAiming()
    {
        IsStepSoundsAvailable();

        audioSource.pitch = 0.6f;
        PlayNextStepSound();
    }
    
    public void FootStepWhileRun()
    {
        IsStepSoundsAvailable();
        audioSource.pitch = 0.8f;
        PlayNextStepSound();
    }
    
    public void FootStepWhileSprint()
    {
        IsStepSoundsAvailable();
        audioSource.pitch = 1f;
        PlayNextStepSound();
    }

    private int currentIndex = 0;
    public void PlayNextStepSound()
    {
        if (!GetComponent<AudioSource>().isPlaying)
        {
            AudioClip clipToPlay = stepSounds[currentIndex];
            audioSource.PlayOneShot(clipToPlay);
            currentIndex = (currentIndex + 1) % stepSounds.Length;
        }
    }

    public bool IsStepSoundsAvailable()
    {
        bool isSoundsAvailable = stepSounds.Length > 0;
        
        if (!isSoundsAvailable)
        {
            Debug.LogWarning("No step sounds available!");
        }

        return isSoundsAvailable;
    }
}