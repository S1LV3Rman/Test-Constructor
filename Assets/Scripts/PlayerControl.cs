using UnityEngine;
using System.Collections;

// Источник: https://null-code.ru/scripts/10-fps-skript-upravleniiya-ot-pervogo-lica.html

public class PlayerControl : MonoBehaviour {
	
	public float speed = 1.5f;

	public Transform head;

	public float sensitivity = 5f; // чувствительность мыши
	public float headMinY = -40f; // ограничение угла для головы
	public float headMaxY = 40f;

	public KeyCode jumpButton = KeyCode.Space; // клавиша для прыжка
	public float jumpForce = 10; // сила прыжка
	public float jumpDistance = 1.2f; // расстояние от центра объекта, до поверхности

	private Vector3 direction;
	private float h, v;
	private int layerMask;
	private Rigidbody body;
	private float rotationY;
	
	void Start () 
	{
		body = GetComponent<Rigidbody>();
		body.freezeRotation = true;
		layerMask = 1 << gameObject.layer | 1 << 2;
		layerMask = ~layerMask;
	}
	
	void FixedUpdate()
	{
		body.AddForce(direction * speed, ForceMode.VelocityChange);
		
		// Ограничение скорости, иначе объект будет постоянно ускоряться
		if(Mathf.Abs(body.velocity.x) > speed)
		{
			body.velocity = new Vector3(Mathf.Sign(body.velocity.x) * speed, body.velocity.y, body.velocity.z);
		}
		if(Mathf.Abs(body.velocity.z) > speed)
		{
			body.velocity = new Vector3(body.velocity.x, body.velocity.y, Mathf.Sign(body.velocity.z) * speed);
		}
	}

	bool GetJump() // проверяем, есть ли коллайдер под ногами
	{
		RaycastHit hit;
		Ray ray = new Ray(transform.position, Vector3.down);
		if (Physics.Raycast(ray, out hit, jumpDistance, layerMask))
		{
			return true;
		}
		
		return false;
	}

	void Update () 
	{
		h = Input.GetAxis("Horizontal");
		v = Input.GetAxis("Vertical");

		// управление головой (камерой)
		float rotationX = head.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivity;
		rotationY += Input.GetAxis("Mouse Y") * sensitivity;
		rotationY = Mathf.Clamp (rotationY, headMinY, headMaxY);
		head.localEulerAngles = new Vector3(-rotationY, rotationX, 0);

		// вектор направления движения
		direction = new Vector3(h, 0, v);
		direction = head.TransformDirection(direction);
		direction = new Vector3(direction.x, 0, direction.z);
		
		if(Input.GetKeyDown(jumpButton) && GetJump())
		{
			body.velocity = new Vector2(0, jumpForce);
		}
	}

	void OnDrawGizmosSelected() // подсветка, для визуальной настройки jumpDistance
	{
		Gizmos.color = Color.red;
		Gizmos.DrawRay(transform.position, Vector3.down * jumpDistance);
	}
}