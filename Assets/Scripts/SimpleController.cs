using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleController : MonoBehaviour
{
    // Скорость передвижения
    public float moveSpeed = 10f;
    
    // Скорость поворота
    public float rotationSpeed = 90f;

    private float movement = 0;
    private float rotation = 0;

    // Update is called once per frame
    void Update()
    {
        // Получаем даные ввода из менеджера
        movement = InputManager.instance.moveZ;
        rotation = InputManager.instance.moveX;
        
        // Получаем необходимые смещение и поворот,
        // умножая на скорость и время
        movement *= moveSpeed * Time.deltaTime;
        rotation *= rotationSpeed * Time.deltaTime;

        // Move translation along the object's z-axis
        transform.Translate(0, 0, movement);

        // Rotate around our y-axis
        transform.Rotate(0, rotation, 0);
    }
}
