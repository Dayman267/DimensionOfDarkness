using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroyAudioSource : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!audioSource.isPlaying || !gameObject.activeSelf)
        {
            Debug.Log("Deleted");
            Destroy(gameObject);
        }
    }
}
