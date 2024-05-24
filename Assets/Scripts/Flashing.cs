using System.Collections;
using UnityEngine;

public class Flashing : MonoBehaviour
{
    [Range(0f, 10f)] public float intensity;
    [Range(0f, 10f)] public float seconds;
    [Range(1f, 50f)] public float upperLimit;
    [Range(0f, 5f)] public float lowerLimit;

    private void Start()
    {
        StartCoroutine(FlashingCoroutine());
    }

    private IEnumerator FlashingCoroutine()
    {
        var light = GetComponent<Light>();
        while (true)
        {
            var localIntesity = light.intensity + Random.Range(-intensity, intensity);
            if (localIntesity > upperLimit) light.intensity = upperLimit;
            else if (localIntesity < lowerLimit) light.intensity = lowerLimit;
            else light.intensity = localIntesity;

            yield return new WaitForSeconds(Random.Range(-seconds, seconds));
        }
    }
}