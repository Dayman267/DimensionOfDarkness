using UnityEngine;

public class VALERAFlashLight : MonoBehaviour
{
    private Light lightSource; // Ссылка на источник света
    [SerializeField] private float maxIntensity = 2f; // Максимальная интенсивность света
    [SerializeField] private float minIntensity = 0.5f; // Минимальная интенсивность света

    private void Start()
    {
        lightSource = GetComponent<Light>();
    }

    void Update()
    {
        float angle = Vector3.Angle(Vector3.down, transform.forward);
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, angle / 90f);
        lightSource.intensity = intensity;
    }
}
