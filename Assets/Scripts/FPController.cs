using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPController : MonoBehaviour
{
    // Скорость передвижения
    public float moveSpeed = 10f;
    
    // Скорость поворота
    public float rotationSpeed = 90f;

    private Vector3 translation;
    private Vector3 rotation;

    private void Start()
    {
        translation = Vector3.zero;
        rotation = Vector3.zero;
    }

    void Update()
    {
        // Получаем даные ввода из менеджера
        translation.x = InputManager.instance.moveX;
        translation.y = InputManager.instance.moveY;
        translation.z = InputManager.instance.moveZ;
        
        rotation.x = -InputManager.instance.mouseY;
        rotation.y = InputManager.instance.mouseX;

        // Ограничиваем вектор смещения,
        // чтобы по диагонали не было ускоренного движения
        translation = Vector3.ClampMagnitude(translation, 1f);
        
        // Получаем необходимые смещение и поворот,
        // умножая на скорость и время
        translation *= moveSpeed * Time.deltaTime;
        rotation *= rotationSpeed * Time.deltaTime;
        
        // Получаем текущее направление без оси Y
        var currentDirection = transform.forward;
        currentDirection.y = 0;
        currentDirection = currentDirection.normalized;

        // Создаём сдвиг по вертикали
        var newTranslation = new Vector3(0, translation.y, 0);
        // Добавляем свиг вперёд
        newTranslation += translation.z * currentDirection;
        // И добавляем сдвиг вбок
        newTranslation += translation.x * transform.right;
        
        // Сдвигаем позицию объекта на полученную величину
        transform.position += newTranslation;

        // Поворачиваем объект
        transform.rotation *= Quaternion.Euler(rotation);
        
        // Убираем поворот по оси Z
        var newRotation = transform.eulerAngles;
        newRotation.z = 0.0f;
        transform.rotation = Quaternion.Euler(newRotation);
    }
}
