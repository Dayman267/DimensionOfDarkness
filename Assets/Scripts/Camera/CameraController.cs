using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player; // Игрок, за которым будет следовать камера
    public float offsetX = 0f; // Смещение камеры по оси X
    public float offsetY = 5f; // Смещение камеры по оси Y
    public float offsetZ = -5f; // Смещение камеры по оси Z
    public float lookOffset = 2f; // Насколько далеко камера смещается в сторону взгляда мыши
    public float rotation = 60f;
    public float smoothSpeed = 0.125f; // Скорость интерполяции

    private Camera cam; // Главная камера
    private Vector3 initialOffset; // Начальное смещение камеры
    private Vector3 targetPosition; // Целевая позиция камеры

    private bool isLookingAround = false; // Флаг для проверки, используется ли смещение

    void Start()
    {
        cam = Camera.main;
        // Рассчитываем начальное смещение камеры относительно игрока
        initialOffset = new Vector3(offsetX, offsetY, offsetZ);
        transform.eulerAngles = new Vector3(rotation, 0f);
        targetPosition = player.position + initialOffset; // Инициализация целевой позиции
    }

    void Update()
    {
        if (!isLookingAround)
        {
            // Если смещение не используется, камера следует за игроком с начальным смещением
            targetPosition = player.position + initialOffset;
        }
        
        // Плавное перемещение камеры к целевой позиции
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
    }

    public void LookAround(Vector3 mousePosition)
    {
        // Получаем текущее положение игрока
        Vector3 playerPos = player.position;

        // Рассчитываем смещение камеры на основе позиции мыши
        Vector3 mouseViewportPos = cam.ScreenToViewportPoint(mousePosition);
        float mouseXOffset = (mouseViewportPos.x - 0.5f) * lookOffset;
        float mouseYOffset = (mouseViewportPos.y - 0.5f) * lookOffset;

        // Новая позиция камеры с учётом смещения от позиции мыши
        targetPosition = playerPos + initialOffset;
        targetPosition.x += mouseXOffset;
        targetPosition.z += mouseYOffset; // Используем mouseYOffset для смещения по Z

        // Устанавливаем флаг, что смещение используется
        isLookingAround = true;
    }

    public void StopLookingAround()
    {
        // Останавливаем использование смещения
        isLookingAround = false;
    }

    public void ResetCameraPosition()
    {
        // Сброс позиции камеры на начальную
        targetPosition = player.position + initialOffset;
        isLookingAround = false;
    }
}
