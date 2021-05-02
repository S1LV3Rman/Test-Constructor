using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    // Оси перемещения мыши
    [HideInInspector]
    public float mouseX;
    [HideInInspector]
    public float mouseY;
    
    // Оси перемещения игрока
    [HideInInspector]
    public float moveX;
    [HideInInspector]
    public float moveY;
    [HideInInspector]
    public float moveZ;
    
    // Зажата левая кнопка мыши?
    [HideInInspector]
    public bool LMB = false;
    
    // Какая цифровая кнопка была нажата последней
    // [HideInInspector]
    public int lastNumber = 1;
    
    // Массив кнопок, которые можно нажимать
    private string[] _activeNumbers;

    private void Start()
    {
        _activeNumbers = new string[] {"1", "2", "3", "4"};
    }

    void Update()
    {
        // Получаем изменения по осям мыши
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        
        // Получаем изменения по осям лево/право, верх/низ и перед/зад
        moveX = Input.GetAxis("Horizontal");
        moveY = Input.GetAxis("Vertical");
        moveZ = Input.GetAxis("Forward");

        // Определяем нажатие ЛКМ
        LMB = Input.GetMouseButton(0);

        // Проверяем все кнопки
        for (int i = 0; i < _activeNumbers.Length; ++i)
        {
            // Если нашли нажатую кнопку
            if (Input.GetKeyDown(_activeNumbers[i]))
            {
                // Записывем её номер и заканчиваем поиск
                lastNumber = i + 1;
                break;
            }
        }
    }
}
