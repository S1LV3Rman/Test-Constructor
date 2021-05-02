using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Получить доступ к классам UI
using UnityEngine.UI;

public class Indicator : MonoBehaviour {
    
    // Отслеживаемый объект.
    public Transform target;
    
    // Точка отсчёта расстояния.
    public Transform showDistanceTo;
    
    // Надпись для отображения расстояния.
    public Text distanceLabel;
    
    // Расстояние от края экрана.
    public int margin = 25;
    
    // Цвет оттенка изображения.
    public Color color {
        set {
            GetComponent<Image>().color = value;
        }
        get {
            return GetComponent<Image>().color;
        }
    }
    
    // Изначальный размер индикатора
    private Vector3 initialScale;
    
    // Выполняет настройку индикатора
    void Start()
    {
        // Получаем размер индикатора поумолчанию
        initialScale = transform.localScale;

        // Скрыть надпись; она будет сделана видимой
        // в методе Update, если цель target будет назначена
        distanceLabel.enabled = false;
        
        // На запуске дождаться ближайшего кадра перед отображением
        // для предотвращения визуальных артефактов
        GetComponent<Image>().enabled = false;
    }
    
    // Обновляет положение индикатора в каждом кадре
    void Update() {

        // Цель исчезла? Если да, значит, индикатор тоже надо убрать
        if (target == null) {
            Destroy (gameObject);
            return;
        }
        
        // Если цель присутствует, вычислить расстояние до нее
        // и показать в distanceLabel
        if (showDistanceTo != null) {
            
            // Показать надпись
            distanceLabel.enabled = true;
            
            // Вычислить расстояние
            var distance = (int)Vector3.Magnitude(
                showDistanceTo.position - target.position);
            
            // Показать расстояние в надписи
            distanceLabel.text = distance.ToString() + "m";
            
        } else {
            // Скрыть надпись
            distanceLabel.enabled = false;
        }
        
        GetComponent<Image>().enabled = true;
        
        //Определить экранные координаты объекта
        var viewportPoint =
            Camera.main.WorldToViewportPoint(target.position);
        
        // Объект за границей экрана?
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
            
            // Определяем в какой стороне должен находиться индикатор
            viewportPoint = viewportPoint.normalized;
            
            // Сдвигаем точку к границе экрана
            viewportPoint.x = Mathf.Clamp(1f - Mathf.Acos(viewportPoint.x) / 1.57f,-0.5f, 0.5f) + 0.5f;
            viewportPoint.y = Mathf.Clamp(Mathf.Asin(viewportPoint.y) / 1.57f,-0.5f, 0.5f) + 0.5f;
            
            // Устанавливаем размер индикатора на половину от изначального
            transform.localScale = initialScale * 0.5f;
        }
        else
        {
            // Вычисляем положение индикатора на экране
            var onViewportPoint = viewportPoint;
            onViewportPoint.z = 0f;
            onViewportPoint.x -= 0.5f;
            onViewportPoint.y -= 0.5f;
            
            // Вычисляем необходимый размер индикатора
            // в зависимости от растояния от центра экрана
            transform.localScale = initialScale *
                                   Mathf.Clamp(1.0f - onViewportPoint.magnitude,
                                                0.5f, 1.0f);
        }

        // Определить видимые координаты для индикатора
        var screenPoint =
            Camera.main.ViewportToScreenPoint(viewportPoint);
        
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
        var localPosition = new Vector2();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>(),
            screenPoint,
            Camera.main,
            out localPosition);
        
        // Обновить позицию индикатора
        var rectTransform = GetComponent<RectTransform>();
        rectTransform.localPosition = localPosition;
    }
}
