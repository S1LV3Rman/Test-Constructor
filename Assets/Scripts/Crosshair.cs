using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Crosshair : MonoBehaviour
{
    // Спрайт перекрестия поумолчанию
    public Sprite defaultCrosshair;
    
    // Спрайт перекрестия при наведении на объект
    public Sprite hoverCrosshair;
    
    // Спрайт перекрестия перемещения объекта
    public Sprite holdCrosshair;

    // Компонент изображения для смены спрайтов
    private Image _image;

    private void Start()
    {
        _image = GetComponent<Image>();
    }

    private void Update()
    {
        // Если перекрестие указывает на объект
        if (GameManager.instance.lookingAtObject)
        {
            // Если ЛКМ зажата спрайт держащий объект,
            // иначе спрайт наведения на объект
            _image.sprite = InputManager.instance.LMB ? holdCrosshair : hoverCrosshair;
        }
        else
        {
            // Перед камерой ничего нет, значит обычный спрайт
            _image.sprite = defaultCrosshair;
        }
    }
}
