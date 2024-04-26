using System;
using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshProUGUI))]
public class AmmoDisplayer : MonoBehaviour
{
    [SerializeField] private PlayerGunSelector GunSelector;
    [SerializeField] private float shakeDuration = 0.1f; // Длительность тряски
    [SerializeField] private float shakeMagnitude = 0.1f; // Магнитуда тряски
    private TextMeshProUGUI AmmoText;
    private bool isShaking = false;
    private Color originalColor;

    private Vector3 originalPosition;

    private void Awake()
    {
        AmmoText = GetComponent<TextMeshProUGUI>();
        originalPosition = transform.position;
        originalColor = AmmoText.color;
    }

    private void Update()
    {
        int currentAmmo = GunSelector.ActiveGun.AmmoConfig.CurrentClipAmmo;
        int maxAmmo = GunSelector.ActiveGun.AmmoConfig.ClipSize;

        AmmoText.SetText($"{currentAmmo} / {maxAmmo}");

        // Проверяем, достигли ли мы 0 патронов и начинаем трясти текст, если это так
        if (currentAmmo == 0 && !isShaking)
        {
            StartCoroutine(ShakeText());
            StartCoroutine(ChangeTextColor(Color.red, 0.5f)); // Здесь вызываем корутину для изменения цвета на красный
        }
    }

    private IEnumerator ShakeText()
    {
        isShaking = true;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            // Генерируем случайное смещение в пределах заданной магнитуды
            Vector3 newPos = originalPosition + UnityEngine.Random.insideUnitSphere * shakeMagnitude;
            newPos.z = originalPosition.z; // Мы не хотим трясти текст вдоль оси Z
            transform.position = newPos;

            // Увеличиваем прошедшее время
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Возвращаем текст на исходную позицию
        transform.position = originalPosition;
        isShaking = false;
    }

    private IEnumerator ChangeTextColor(Color targetColor, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Выполняем плавное изменение цвета текста к целевому цвету
            AmmoText.color = Color.Lerp(originalColor, targetColor, elapsedTime / duration);

            // Увеличиваем прошедшее время
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Возвращаем тексту исходный цвет
        AmmoText.color = originalColor;
    }
}