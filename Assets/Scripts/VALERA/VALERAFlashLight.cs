using UnityEngine;

public class VALERAFlashLight : MonoBehaviour
{
    [SerializeField] private float maxIntensity = 2f; // Максимальная интенсивность света
    [SerializeField] private float minIntensity = 0.5f; // Минимальная интенсивность света
    private Light lightSource; // Ссылка на источник света

    private void Start()
    {
        lightSource = GetComponent<Light>();
    }

    private void Update()
    {
        var angle = Vector3.Angle(Vector3.down, transform.forward);
        var intensity = Mathf.Lerp(minIntensity, maxIntensity, angle / 90f);
        lightSource.intensity = intensity;
    }
}