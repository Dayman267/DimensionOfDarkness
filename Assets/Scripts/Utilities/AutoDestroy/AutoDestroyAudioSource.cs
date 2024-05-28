using UnityEngine;

public class AutoDestroyAudioSource : MonoBehaviour
{
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!audioSource.isPlaying || !gameObject.activeSelf)
        {
            Debug.Log("Deleted");
            Destroy(gameObject);
        }
    }
}