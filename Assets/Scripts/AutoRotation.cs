using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f;

    private bool isRotating;
    private Quaternion targetRotation;

    void Update()
    {
        if (isRotating)
        {
            // Плавное вращение модели к целевому повороту
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Проверка на завершение вращения
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                isRotating = false;
            }
        }
    }

    // Метод для начала плавного вращения модели к указанному направлению
    public void RotateToDirection(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            isRotating = true;
        }
    }
}
