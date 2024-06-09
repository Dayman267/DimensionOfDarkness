using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class Laser : MonoBehaviour
{
    private LineRenderer lr;

    private void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        // Проверяем, зажата ли правая кнопка мыши
        if (PlayerController.IsRightClickDown())
        {
            // Создаем луч от текущей позиции до позиции мыши на экране
            var ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);

            RaycastHit hit;
            Debug.Log(lr == null);
            // Проверяем столкновение луча с объектом на сцене
            if (Physics.Raycast(ray, out hit))
            {
                // Устанавливаем начальную точку лазера в текущем положении
                lr.SetPosition(0, transform.position);
                // Устанавливаем конечную точку лазера в точке попадания луча в объект
                lr.SetPosition(1, hit.point);
            }
            else
            {
                // Если луч не сталкивается с объектом, устанавливаем конечную точку лазера на максимальное расстояние
                lr.SetPosition(0, transform.position);
                lr.SetPosition(1, ray.origin + ray.direction * 5000f);
            }
        }
        else
        {
            // Если правая кнопка мыши не нажата, отключаем лазер
            lr.SetPosition(0, Vector3.zero);
            lr.SetPosition(1, Vector3.zero);
        }
    }

    private void LaserToMousePoint()
    {
        RaycastHit hit;

        // Определите направление лазера от текущей позиции к точке на экране, где находится курсор
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider)
                // Установите конечную точку лазера на точке попадания
                lr.SetPosition(1, hit.point - transform.position);
        }
        else
        {
            // Если луч не сталкивается с объектом, установите длину лазера на максимальное значение
            lr.SetPosition(1, Vector3.forward * 5000);
        }
    }
}