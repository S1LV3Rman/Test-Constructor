using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum GameStages
{
    MovingObject,
    ConnectingObject,
    CreatingObject,
    PlayMode,
    ConstructMode
}

public class GameManager : Singleton<GameManager>
{
    // Используемая камера
    public Camera currentCamera;

    // Поцентру перед камерай находится объект?
    [HideInInspector]
    public bool lookingAtObject = false;

    // Поцентру перед камерай находится игрок?
    [HideInInspector]
    public bool lookingAtPlayer = false;

    // Текущий активный режим
    private GameStages _gameStage = GameStages.ConstructMode;
    
    // Максимальная дистанция
    // для обнаружения объектов перед камерой
    public float maxRayDistance = 10f;

    // Слой на котором находятся объекты
    public LayerMask rayLayer;

    // Точка соприкосновения с объектом перед камерой
    private RaycastHit _currentHit;

    // Сдвиг удерживаемого объекта от камеры
    private Vector3 _objectTranslation;

    // Объект, который сейчас выбран
    // (для соединения или захвата)
    private Transform _currentTarget;

    // Объект, выбранный для соединения
    private Transform _connectionTarget;

    // Можно установить соединение?
    private bool canSetConnection = false;
    
    private void Start()
    {
        _objectTranslation = Vector3.zero;
    }

    void Update()
    {
        // Выбираем действи в зависимости от активного режима
        switch (_gameStage)
        {
            // Режим перемещения объекта ////////////////////////
            case GameStages.MovingObject:

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

                break;

            // Режим соединения объектов //////////////////////////
            case GameStages.ConnectingObject:

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

                break;

            // Режим создания объекта //////////////////////////
            case GameStages.CreatingObject:

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

                break;

            // Стандартный режим конструктора ////////////////////
            case GameStages.ConstructMode:

                // Выпускаем луч через центр экрана
                if (Physics.Raycast(
                    currentCamera.transform.position,
                    currentCamera.transform.forward,
                    out _currentHit,
                    maxRayDistance,
                    rayLayer))
                {
                    lookingAtObject = true;
                    
                    // Получаем основной объект
                    _currentTarget = _currentHit.collider.transform.parent;
                    
                    // Если смотрим на игрока
                    if (_currentTarget.CompareTag("Player"))
                    {
                        lookingAtPlayer = true;

                        // Если нажата кнопка действия
                        if (InputManager.instance.action)
                        {
                            EnterPlayMode();
                        }
                    }
                    else
                    {
                        lookingAtPlayer = false;
                    }
                    
                    // Проверяем какой инструмент,
                    // взаимодействующий с объектами сейчас активен
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
                    }
                }
                else
                {
                    lookingAtObject = false;
                    lookingAtPlayer = false;
                }

                // Проверяем какой инструмент,
                // не взаимодействующий с объектами сейчас активен
                switch (InputManager.instance.lastNumber)
                {
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

                break;
            
            // Режим игры //////////////////////////////////
            case GameStages.PlayMode:
                if (InputManager.instance.action)
                {
                    ExitPlayMode();
                }
                
                break;
        }
    }

    // Захватывает объект, на который смотрит камера
    void TakeObject()
    {
        // Если перед камерой есть объект
        if (lookingAtObject)
        {
            // Проверяем, что цель не игрок
            if (!_currentTarget.CompareTag("Player"))
            {
                // Делаем объект кинематиком, чтобы он не падал
                _currentTarget.GetComponent<Rigidbody>().isKinematic = true;
            }

            // Вычисляем позицию объекта относительно камеры
            _objectTranslation =
                currentCamera.transform.InverseTransformDirection(
                    _currentTarget.position - currentCamera.transform.position);

            // Объект взят
            _gameStage = GameStages.MovingObject;
        }
    }

    // Отпускает захваченый объект
    void DropObject()
    {
        // Проверяем, что объект сейчас захвачен
        if (_gameStage == GameStages.MovingObject)
        {
            // Проверяем, что цель не игрок
            if (!_currentTarget.CompareTag("Player"))
            {
                // Возвращаем объекту нормальную физику
                _currentTarget.GetComponent<Rigidbody>().isKinematic = false;
            }

            // Возвращаемся в обычный режим
            _gameStage = GameStages.ConstructMode;
        }
    }

    // Выбирает объект для объединения
    void StartMerge()
    {
        if (lookingAtObject)
        {
            // Получаем основной объект
            _connectionTarget = _currentTarget;

            // Переводим объект на слой поумолчанию,
            // чтобы нельзя было объединить его самого с собой
            _connectionTarget.GetComponent<CombinedObject>().SetLayerMask(LayerMask.NameToLayer("Uninteractable"));

            // Создаём линию соединения на экране
            InterfaceManager.instance.CreateConnection(_connectionTarget);

            // Началось соединение объектов
            _gameStage = GameStages.ConnectingObject;
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
        if (_gameStage == GameStages.ConnectingObject)
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
            _gameStage = GameStages.ConstructMode;
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
        _gameStage = GameStages.CreatingObject;
    }

    // Заканчивает создание объекта
    void EndCreating()
    {
        // Проверяем, что объект создаётся
        if (_gameStage == GameStages.CreatingObject)
        {
            // Возвращаем объекту нормальную физику
            _currentTarget.GetComponent<Rigidbody>().isKinematic = false;

            // Сбрасываем признак
            _gameStage = GameStages.ConstructMode;
            
            // Меняем текущий инструмент на перемещение
            InputManager.instance.lastNumber = 1;
        }
    }
    
    // Запускает режим игры
    void EnterPlayMode()
    {
        //Проверяем, что мы в режиме конструктора
        if (_gameStage == GameStages.ConstructMode)
        {
            // Переключаем поведение камеры
            currentCamera.GetComponent<FPController>().enabled = false;
            currentCamera.GetComponent<SmoothFollow>().enabled = true;

            // Включаем на объекте управление
            _currentTarget.GetComponent<SimpleController>().enabled = true;

            // Отключаем интерфейс конструктора
            InterfaceManager.instance.constructUI.SetActive(false);
            
            // Переходим в режим игры
            _gameStage = GameStages.PlayMode;

            // Сбрасываем признаки
            lookingAtPlayer = false;
            lookingAtObject = false;
        }
    }
    
    // Выходим из режима игры
    void ExitPlayMode()
    {
        //Проверяем, что мы в режиме игры
        if (_gameStage == GameStages.PlayMode)
        {
            // Переключаем поведение камеры
            currentCamera.GetComponent<FPController>().enabled = true;
            currentCamera.GetComponent<SmoothFollow>().enabled = false;

            // Выключаем на объекте управление
            _currentTarget.GetComponent<SimpleController>().enabled = false;

            // Включаем интерфейс конструктора
            InterfaceManager.instance.constructUI.SetActive(true);

            // Переходим в режим конструктора
            _gameStage = GameStages.ConstructMode;
        }
    }
}
