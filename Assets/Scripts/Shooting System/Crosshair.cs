using UnityEngine;
using UnityEngine.InputSystem;

public class Crosshair : MonoBehaviour
{
    private static bool shotFired;
    public float maxExpansion = 5.0f; // Максимальное увеличение при выстреле
    public float expansionSpeed = 100.0f; // Скорость увеличения при выстреле
    public float resetSpeed = 5.0f; // Скорость сжатия после выстрела

    private Vector3 originalScale;


    private void Awake()
    {
        Cursor.visible = false;
    }

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        // Получаем позицию курсора мыши в мировых координатах
        Vector3 cursorPosition = Mouse.current.position.value;

        // Устанавливаем позицию объекта-прицела в позицию курсора
        transform.position = cursorPosition;

        if (shotFired)
        {
            // Увеличиваем прицел
            transform.localScale += Vector3.one * expansionSpeed * Time.deltaTime;

            // Ограничиваем максимальное увеличение
            transform.localScale = Vector3.Min(originalScale * maxExpansion, transform.localScale);
            shotFired = false;
        }
        else
        {
            // Сжимаем прицел после выстрела
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, resetSpeed * Time.deltaTime);
        }
    }

    public static void OnShotFired()
    {
        shotFired = true;
    }
}