using UnityEngine;

public class AutoDestroyParticleSystem : MonoBehaviour
{
    private ParticleSystem particleSystem;

    private void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        // Проверяем, проигралась ли система частиц
        if (!particleSystem.isPlaying)
            // Если система частиц не проигрывается, уничтожаем объект
            Destroy(gameObject);
    }
}