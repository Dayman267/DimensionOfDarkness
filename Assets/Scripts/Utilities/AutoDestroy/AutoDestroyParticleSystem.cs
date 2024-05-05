using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroyParticleSystem : MonoBehaviour
{
    private ParticleSystem particleSystem;

    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        // Проверяем, проигралась ли система частиц
        if (!particleSystem.isPlaying)
        {
            // Если система частиц не проигрывается, уничтожаем объект
            Destroy(gameObject);
        }
    }
}
