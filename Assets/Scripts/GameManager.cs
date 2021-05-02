using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    // Используемая камера
    public Camera currentCamera;

    // Поцентру перед камерай находится объект?
    [HideInInspector]
    public bool lookingAtObject = false;

    // Максимальная дистанция
    // для обнаружения объектов перед камерой
    public float maxRayDistance = 10f;

    // Слой на котором находятся объекты
    public LayerMask rayLayer;

    // Игрок держит объект?
    [HideInInspector]
    public bool holdingObject = false;

    // Точка соприкосновения с объектом перед камерой
    private RaycastHit _currentHit;

    // Сдвиг удерживаемого объекта от камеры
    private Vector3 _objectTranslation;

    // Объект, который сейчас выбран
    // (для соединения или захвата)
    private Transform _currentTarget;

    // Объект, выбранный для соединения
    private Transform _connectionTarget;

    // Игрок соединяет объекты?
    private bool connectingObject = false;

    // Можно установить соединение?
    private bool canSetConnection = false;

    // Сейчас создаётся объект?
    private bool creatingObject = false;
    
    private void Start()
    {
        _objectTranslation = Vector3.zero;
    }

    void Update()
    {
        // Если сейчас игрок держит объект
        if (holdingObject)
        {
            // И при этом ЛКМ отжата
            if (!InputManager.instance.LMB)
            {
                // Отпускаем объект
                DropObject();
            }
            // Иначе продолжаем двигать объект
            else if (_currentTarget != null)
            {
                // Двигаем объект за камерой
                // (преобразуем локальный сдвиг в глобальный и
                // добавляем к позиции камеры; можно проще?)
                _currentTarget.position =
                    currentCamera.transform.position + 
                    currentCamera.transform.TransformDirection(_objectTranslation);
            }
        }
        // Если сейчас игрок соединяет объекты
        else if (connectingObject)
        {
            // И при этом ЛКМ отжата
            if (!InputManager.instance.LMB)
            {
                // Заканчиваем соединение
                EndMerge();
            }
            // Иначе выпускаем луч через центр экрана
            else if (Physics.Raycast(
                currentCamera.transform.position,
                currentCamera.transform.forward,
                out _currentHit,
                maxRayDistance,
                rayLayer))
            {
                CheckMerge();
            }
            else
            {
                InterfaceManager.instance.RevertConnection();
                
                canSetConnection = false;
            }
        }
        // Если сейчас создаётся объект
        else if (creatingObject)
        {
            // И при этом ЛКМ отжата
            if (!InputManager.instance.LMB)
            {
                // Заканчиваем создание объекта
                EndCreating();
            }
            // Иначе продолжаем двигать объект
            else if (_currentTarget != null)
            {
                // Двигаем объект за камерой
                // на расстоянии 1/4 maxRayDistance
                _currentTarget.position =
                    currentCamera.transform.position + 
                    currentCamera.transform.forward * (maxRayDistance / 4);
            }
        }
        else
        {
            // Выпускаем луч через центр экрана
            lookingAtObject = Physics.Raycast(
                currentCamera.transform.position,
                currentCamera.transform.forward,
                out _currentHit,
                maxRayDistance,
                rayLayer);
            
            // Определям какой инструмент сейчас активен
            switch (InputManager.instance.lastNumber)
            {
                // Перемещение объекта
                case 1:
                    // Если ЛКМ зажата
                    if (InputManager.instance.LMB)
                    {
                        // Берём объект
                        TakeObject();
                    }
                    break;

                // Объединение объектов
                case 2:
                    // Если ЛКМ зажата
                    if (InputManager.instance.LMB)
                    {
                        // Выбираем объект для объединения
                        StartMerge();
                    }
                    break;

                // Создание куба
                case 3:
                    // Если ЛКМ зажата
                    if (InputManager.instance.LMB)
                    {
                        // Создаём куб
                        StartCreating(SimpleObjectTypes.cube);
                    }
                    break;

                // Создание сферы
                case 4:
                    // Если ЛКМ зажата
                    if (InputManager.instance.LMB)
                    {
                        // Создаём сферу
                        StartCreating(SimpleObjectTypes.sphere);
                    }
                    break;
            }
        }
    }

    // Захватывает объект, на который смотрит камера
    void TakeObject()
    {
        // Если перед камерой есть объект
        if (lookingAtObject)
        {
            // Получаем основной объект
            _currentTarget = _currentHit.collider.transform.parent;

            // Делаем объект кинематиком, чтобы он не падал
            _currentTarget.GetComponent<Rigidbody>().isKinematic = true;

            // Вычисляем позицию объекта относительно камеры
            _objectTranslation =
                currentCamera.transform.InverseTransformDirection(
                    _currentTarget.position - currentCamera.transform.position);

            // Объект взят
            holdingObject = true;
        }
    }

    // Отпускает захваченый объект
    void DropObject()
    {
        // Проверяем, что объект сейчас захвачен
        if (holdingObject)
        {
            // Возвращаем объекту нормальную физику
            _currentTarget.GetComponent<Rigidbody>().isKinematic = false;

            // Сбрасываем признак
            holdingObject = false;
        }
    }

    // Выбирает объект для объединения
    void StartMerge()
    {
        if (lookingAtObject)
        {
            // Получаем основной объект
            _connectionTarget = _currentHit.collider.transform.parent;

            // Переводим объект на слой поумолчанию,
            // чтобы нельзя было объединить его самого с собой
            _connectionTarget.GetComponent<CombinedObject>().SetLayerMask(LayerMask.NameToLayer("Uninteractable"));

            // Создаём линию соединения на экране
            InterfaceManager.instance.CreateConnection(_connectionTarget);

            // Началось соединение объектов
            connectingObject = true;
        }
    }

    // Проверяет правильность соединения
    void CheckMerge()
    {
        // Получаем основной объект
        _currentTarget = _currentHit.collider.transform.parent;

        // Проверяем соприкасаются ли выбранные объекты
        canSetConnection = _connectionTarget.GetComponent<CombinedObject>().
            currentCollisions.Contains(_currentTarget);
        
        // Соединяем линию со вторым объектом
        InterfaceManager.instance.ChangeConnection(_currentTarget, canSetConnection);
    }

    // Прекращает соединение
    void EndMerge()
    {
        // Проверяем, что соединение объектов активно
        if (connectingObject)
        {
            // Возвращаем объект на слой активных объектов
            _connectionTarget.GetComponent<CombinedObject>().SetLayerMask(LayerMask.NameToLayer("Interactable"));
            
            // Убираем линию соединения с экрана
            InterfaceManager.instance.DeleteConnection();
            
            // Проверяем можно ли создать соединение
            // (оба объекта выбраны)
            if (canSetConnection)
            {
                // Объединяем объекты
                ObjectManager.instance.MergeObjects(
                    _currentTarget.GetComponent<CombinedObject>(), 
                    _connectionTarget.GetComponent<CombinedObject>());

                // Сбрасываем признак
                canSetConnection = false;
            }

            // Сбрасываем признак
            connectingObject = false;
        }
    }
    
    // Начинает создание объекта
    void StartCreating(SimpleObjectTypes type)
    {
        // Создаём новый объект и получаем его transform
        _currentTarget = ObjectManager.instance.CreateObject(
            currentCamera.transform.position +
            currentCamera.transform.forward * (maxRayDistance / 4),
            type);

        // Делаем объект кинематиком, чтобы он не падал
        _currentTarget.GetComponent<Rigidbody>().isKinematic = true;
        
        // Объект создаётся
        creatingObject = true;
    }

    // Заканчивает создание объекта
    void EndCreating()
    {
        // Проверяем, что объект создаётся
        if (creatingObject)
        {
            // Возвращаем объекту нормальную физику
            _currentTarget.GetComponent<Rigidbody>().isKinematic = false;

            // Сбрасываем признак
            creatingObject = false;
        }
    }
}
