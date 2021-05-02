using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SimpleObjectTypes
{
    cube,
    sphere
}

public class SimpleObject : MonoBehaviour, IObject
{
    // Цвет объекта
    [SerializeField]
    private Color _color;
    
    // Интерфейс для доступа к цвету
    public Color Color
    {
        get => _renderer.material.color;
        set
        {
            _color = value;
            if (_renderer != null)
            {
                _renderer.material.color = value;
            }
        }
    }

    // Компонент отрисовки
    private Renderer _renderer;
    
    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _renderer.material.color = _color;
    }
}
