using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SoundController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [Header("Steps sounds")]
    [SerializeField] private AudioClip[] stepSounds;
    [Header("Rolling sounds")]
    [SerializeField] private AudioClip startRollingSound;
    [SerializeField] private AudioClip rollingSound;
    [SerializeField] private AudioClip endRollingSound;

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

    public void StartRollSound()
    {
        audioSource.pitch = 0.8f;
        audioSource.PlayOneShot(startRollingSound);
    }

    public void RollSound()
    {
        audioSource.pitch = 0.8f;
        audioSource.PlayOneShot(rollingSound);
    }

    public void EndRollingSound()
    {
        audioSource.pitch = 0.8f;
        audioSource.PlayOneShot(endRollingSound);
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