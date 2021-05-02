using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceManager : Singleton<InterfaceManager>
{
    // Отступ элементов интерфейса от края экрана
    public float margin = 10f;
    
    // Прекрестие в центре экрана
    public Crosshair crosshair;

    // Интерфейс в режиме конструктора
    public GameObject constructUI;

    // Префаб соединения
    public GameObject connectionPrefab;

    // Элементы на панели инструментов
    public RectTransform[] tools;
    
    // Изображение выделяющее активный инструмент
    public RectTransform highlight;
    
    // Текущее активное соединение
    private Connection _currentConnection;

    // Номер текущего выбранного инструмента
    private int _currentTool = 1;

    private void Update()
    {
        // Смотрим последнюю нажатую цифру
        var currentTool = InputManager.instance.lastNumber;
        
        // Если текущий инструмент поменялся
        if (_currentTool != currentTool)
        {
            // Сдвигаем выделение под текущий инструмент
            highlight.position = tools[currentTool - 1].position;
            _currentTool = currentTool;
        }
    }

    // Создаёт соединение с объектом и центром экрана
    // (т.е. выбран ещё только 1 объект)
    public void CreateConnection(Transform startPoint)
    {
        // Удаляем соединение, если оно уже существует
        if (_currentConnection != null)
        {
            Destroy(_currentConnection.gameObject);
        }

        // Создаём новое соединение
        _currentConnection = Instantiate(connectionPrefab, constructUI.transform).
            GetComponent<Connection>();

        // Задаём соединению начальную (первый объект)
        // и конечную (центр экрана) точки
        _currentConnection.startTarget = startPoint;
        _currentConnection.endTarget = crosshair.transform;
    }

    // Меняет второй объект соединения
    // (isValid для выбора цвета)
    public void ChangeConnection(Transform endPoint, bool isValid)
    {
        // Проверяем, что соединение существует
        if (_currentConnection != null)
        {
            // Задаём конечную точку соединения
            _currentConnection.endTarget = endPoint;
        
            // Зелёный цвет, если соединение корректно, иначе - красный
            _currentConnection.Color = isValid ? Color.green : Color.red;
        }
    }

    // Отменяет соединение со вторым объектом
    // (т.е. второй точкой опять становится центр экрана)
    public void RevertConnection()
    {
        // Проверяем, что соединение существует
        if (_currentConnection != null)
        {
            // Задаём конечную точку соединения
            _currentConnection.endTarget = crosshair.transform;
        
            // Зелёный цвет, если соединение корректно, иначе - красный
            _currentConnection.Color = Color.red;
        }
    }

    // Удаляет соединение
    // (если соединение объектов завершено или прервано)
    public void DeleteConnection()
    {
        // Удаляем соединение, если оно существует
        if (_currentConnection != null)
        {
            Destroy(_currentConnection.gameObject);
        }
    }
}
