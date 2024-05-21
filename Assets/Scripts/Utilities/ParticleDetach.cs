using System;
using UnityEngine;

public class ParticleDetach : MonoBehaviour
{
    public ParticleSystem particleSystem;

    void Start()
    {
        if (particleSystem == null)
        {
            particleSystem = GetComponent<ParticleSystem>();
        }
    }

    private void OnDestroy()
    {
        DetachAndDestroy();
    }

    public void DetachAndDestroy()
    {
        // Создаем новый GameObject для системы частиц
        GameObject particleSystemContainer = new GameObject("DetachedParticleSystem");
        particleSystemContainer.transform.position = particleSystem.transform.position;

        // Переносим систему частиц в новый контейнер
        particleSystem.transform.parent = particleSystemContainer.transform;

        // Останавливаем эмиссию частиц
        particleSystem.Stop();

        // Уничтожаем новый контейнер после проигрывания всех частиц
        Destroy(particleSystemContainer, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax);
    }
}