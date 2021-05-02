using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Connection : MonoBehaviour
{
    // Объект от которого линия рисуется
    public Transform startTarget;
    
    // Объект к которому линия рисуется
    public Transform endTarget;

    // Компонент отрисовки линии
    private LineRenderer _lineRenderer;
    
    // Цвет линии
    private Color color;
    
    // Интерфейс для доступа к цвету
    public Color Color
    {
        get => _lineRenderer.startColor;
        set
        {
            color = value;
            _lineRenderer.startColor = value;
            _lineRenderer.endColor = value;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        _lineRenderer.SetPosition(0, ClampToScreen(startTarget.position));
        _lineRenderer.SetPosition(1, ClampToScreen(endTarget.position));
    }

    // Определяет распложение точки пространства на экране
    // (преобразует координаты точки в координаты viewport,
    // сложные вычисления, чтобы точка не выходила за границы,
    // потом преобразует в координаты на экране,
    // а потом в локальные координаты холста)
    Vector3 ClampToScreen(Vector3 point)
    {
        //Определить экранные координаты точки
        var viewportPoint =
            GameManager.instance.currentCamera.WorldToViewportPoint(point);
        
        // Точка за границей экрана?
        if (viewportPoint.z < 0 ||
            viewportPoint.x < 0 || viewportPoint.x > 1 ||
            viewportPoint.y < 0 || viewportPoint.y > 1)
        {
            // Сдвигаем координаты в центр экрана
            // и инвертируем, если точка позади нас
            if (viewportPoint.z < 0)
            {
                viewportPoint.x = 0.5f - viewportPoint.x;
                viewportPoint.y = 0.5f - viewportPoint.y;
            }
            else
            {
                viewportPoint.x -= 0.5f;
                viewportPoint.y -= 0.5f;
            }
            
            // Сдвигаем точку к плоскости экрана
            viewportPoint.z = 0;
            
            // Определяем в какой стороне должна находиться точка
            viewportPoint = viewportPoint.normalized;
            
            // Сдвигаем точку к границе экрана
            viewportPoint.x = Mathf.Clamp(1f - Mathf.Acos(viewportPoint.x) / 1.57f,-0.5f, 0.5f) + 0.5f;
            viewportPoint.y = Mathf.Clamp(Mathf.Asin(viewportPoint.y) / 1.57f,-0.5f, 0.5f) + 0.5f;
        }
        else
        {
            // Вычисляем положение точки на экране
            var onViewportPoint = viewportPoint;
            onViewportPoint.z = 0f;
            onViewportPoint.x -= 0.5f;
            onViewportPoint.y -= 0.5f;
        }

        // Определить видимые координаты для точки
        var screenPoint =
            GameManager.instance.currentCamera.ViewportToScreenPoint(viewportPoint);

        // Получаем отступ от края экрана
        var margin = InterfaceManager.instance.margin;
        
        // Ограничить краями экрана
        screenPoint.x = Mathf.Clamp(
            screenPoint.x,
            margin,
            Screen.width - margin);
        screenPoint.y = Mathf.Clamp(
            screenPoint.y,
            margin,
            Screen.height - margin);
        
        // Определить, где в области холста находится видимая координата
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>(),
            screenPoint,
            GameManager.instance.currentCamera,
            out var localPosition);
        
        // Возвращаем позицию точки на экране
        return localPosition;
    }
}
