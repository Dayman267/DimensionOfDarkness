using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Audio Config", menuName = "Guns/Audio Config", order = 5)]
public class AudioConfigSO : ScriptableObject, ICloneable
{
    [Range(0,1f)]
    public float Volume = 1f;

    public AudioClip[] FireClips;
    public AudioClip EmptyClip;
    public AudioClip ReloadClip;
    public AudioClip LastBulletClip;

    public void PlayShootingClip(AudioSource AudioSource, bool IsLastBullet = false)
    {
        if (IsLastBullet && LastBulletClip != null)
        {
            AudioSource.PlayOneShot(LastBulletClip,Volume);
        }
        else
        {
            AudioSource.PlayOneShot(FireClips[Random.Range(0, FireClips.Length)], Volume);
        }
    }

    public void PlayOutOfAmmoClip(AudioSource AudioSource)
    {
        if (EmptyClip != null)
        {
            AudioSource.PlayOneShot(EmptyClip,Volume);
        }
    }

    public void PlayReloadClip(AudioSource AudioSource) 
    {
        if (ReloadClip != null)
        {
            AudioSource.PlayOneShot(ReloadClip,Volume);
        }
    }

    public object Clone()
    {
        AudioConfigSO config = CreateInstance<AudioConfigSO>();
        
        Utilities.CopyValues(this, config);

        return config;
    }
}
