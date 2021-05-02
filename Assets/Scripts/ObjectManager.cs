using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class ObjectManager : Singleton<ObjectManager>
{
    // Префабы для создания примитивов
    public SimpleObjectDictionary simpleObjectPrefabs;

    // Префаб для создания комбиниованного объекта
    public GameObject combinedObjectPrefab;

    // Пространство, в котором располагаются все объекты
    // (будет использоваться как родитель поумолчанию для новых объектов)
    public Transform ObjectsSpace;

    // Список всех объектов на сцене
    public List<CombinedObject> objects = new List<CombinedObject>();

    private void Start()
    {
        // Ищем все комбинированные объекты на сцене
        // в заданом пространстве (дочерние к ObjectsSpace)
        var combinedObjects = ObjectsSpace.GetComponentsInChildren<CombinedObject>();

        // Добавляем найденые объекты в список
        objects.AddRange(combinedObjects);
    }
    
    // Присоединяет объект к существующему
    private void AttachObjects(CombinedObject mainObject, CombinedObject objectToAttach)
    {
        AttachObjects(mainObject, new CombinedObject[] { objectToAttach }); 
    }

    // Присоединяет объекты к существующему
    private void AttachObjects(CombinedObject mainObject, CombinedObject[] objectsToAttach)
    {
        // Добавляем все объекты в главный
        foreach (var obj in objectsToAttach)
        {
            // Добавляем объекты
            mainObject.AddObjects(obj.combinedObjects.ToArray());

            // Удаляем этот объект из списка объектов на сцене
            objects.Remove(obj);

            // И уничтожаем сам GameObject
            Destroy(obj.gameObject);
        }
    }
    
    // Объединяет 2 объекта в новый
    public void MergeObjects(CombinedObject firstObjectToMerge, CombinedObject secondObjectToMerge)
    {
        MergeObjects(new CombinedObject[] { firstObjectToMerge, secondObjectToMerge }); 
    }
    
    // Объединяет объекты в новый
    public void MergeObjects(CombinedObject[] objectsToMerge)
    {
        // Создаём новый комбинированый объект
        var newCombinedObject =
            Instantiate(combinedObjectPrefab, ObjectsSpace);

        // Получаем компонент CombinedObject нового объекта
        var сombinedObject = newCombinedObject.GetComponent<CombinedObject>();

        // Добавляем все объекты в новый
        foreach (var obj in objectsToMerge)
        {
            // Добавляем всесь список объектов в новый
            сombinedObject.combinedObjects.AddRange(
                obj.combinedObjects);

            // Отсоединяем собраные в нём объекты
            obj.DetachObjects();

            // Удаляем этот объект из списка объектов на сцене
            objects.Remove(obj);

            // И уничтожаем сам GameObject
            Destroy(obj.gameObject);
        }
        
        // Добавляем новый объект в список объектов на сцене
        objects.Add(сombinedObject);
    }

    // Создаёт абсолютно новый объект заданного типа в заданной позиции
    public Transform CreateObject(Vector3 position, SimpleObjectTypes type)
    {
        // Создаём новый комбинированый объект
        var newCombinedObject = Instantiate(combinedObjectPrefab, ObjectsSpace);
        
        // Создаём примитив
        var newSimpleObject = Instantiate(simpleObjectPrefabs[type], position, Quaternion.identity);
        
        // Задаём примитиву случайный цвет с 50% прозрачностью
        var simpleObject = newSimpleObject.GetComponent<SimpleObject>();
        
        simpleObject.Color = Random.ColorHSV(0f, 1f, 
                                            0f, 1f, 
                                            0f, 1f, 
                                            0.5f, 0.5f);
        
        // Добавляем примитив в основной объект
        var сombinedObject = newCombinedObject.GetComponent<CombinedObject>();
        сombinedObject.combinedObjects.Add(simpleObject);
        
        // Добавляем новый объект в список объектов на сцене
        objects.Add(сombinedObject);

        return newCombinedObject.transform;
    }
    
    // Удаляет существующий объект
    public void DeleteObject(CombinedObject deletingObject)
    {
        // Проверяем, что удаляемый объект существует
        // и находится на сцене
        if (deletingObject == null || !objects.Contains(deletingObject)) return;
        
        // Удаляем этот объект из списка объектов на сцене
        objects.Remove(deletingObject);

        // И уничтожаем сам GameObject
        Destroy(deletingObject.gameObject);
    }
}
