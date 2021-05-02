using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CombinedObject : MonoBehaviour
{
    // Список объединённых примитивов
    public List<SimpleObject> combinedObjects = new List<SimpleObject>();
    
    // Список объектов, с которым соприкасается этот объект
    public List<Transform> currentCollisions = new List<Transform>();

    private void Start()
    {
        // Вычисляем новое расположение объекта
        // в середеине между всеми его составляющими
        var newPosition = Vector3.zero;

        // Суммируем расположение всех примитивов
        foreach (var obj in combinedObjects)
        {
            newPosition += obj.transform.position;
        }
 
        // И делим на их количество
        transform.position = newPosition / combinedObjects.Count;
        
        // Теперь устанавливаем этот объект родителем
        // для входящих в него примитов
        foreach (var obj in combinedObjects)
        {
            obj.transform.SetParent(transform);
        }
    }
    
    // Присоединяет один примитив
    public void AddObject(SimpleObject newObject)
    {
        // Добавляем новый примитив в список
        combinedObjects.Add(newObject);
        
        // И меняем ему родителя
        newObject.transform.SetParent(transform);
    }
    
    // Присоединяет примитивы
    public void AddObjects(SimpleObject[] newObjects)
    {
        // Добавляем новые примитивы в список
        combinedObjects.AddRange(newObjects);
        
        // Проходимся по всем соединённым примитивам
        foreach (var obj in newObjects)
        {
            // Меняем родителя
            obj.transform.SetParent(transform);
        }
    }

    // Разделяет объект
    public void DetachObjects()
    {
        // Проходимся по всем соединённым примитивам
        foreach (var obj in combinedObjects)
        {
            // Меняем родителя
            obj.transform.SetParent(
                ObjectManager.instance.ObjectsSpace.transform);
        }
        
        // Очищаем список примитивов
        combinedObjects.Clear();
    }
    
    // Устанавливает слой для всех соединённых объектов
    public void SetLayerMask(int layer)
    {
        // Меняем слой самого объекта
        gameObject.layer = layer;

        // Меняем слой каждого входящего в него объекта
        foreach (var obj in combinedObjects)
        {
            obj.gameObject.layer = layer;
        }
    }

    // Добавляет объект в список соприкасаемых 
    private void OnCollisionEnter(Collision other)
    {
        if (other.rigidbody != null)
        {
            var obj = other.rigidbody.transform;
            if (!currentCollisions.Contains(obj))
            {
                currentCollisions.Add(obj);
            }
        }
    }

    // Убирает объект из списока соприкасаемых
    private void OnCollisionExit(Collision other)
    {
        if (other.rigidbody != null)
        {
            var obj = other.rigidbody.transform;
            if (currentCollisions.Contains(obj))
            {
                currentCollisions.Remove(obj);
            }
        }
    }
}
