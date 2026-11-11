using UnityEngine;
using UnityEngine.InputSystem;

// Controlador sencillo para recorrer la escena en Play (alternativa a Starter Assets).
// WASD: mover · Q/E: bajar/subir · Shift: rápido · Clic derecho sostenido: mirar.
public class CamaraLibre : MonoBehaviour
{
    [SerializeField] float velocidad = 4f;
    [SerializeField] float sensibilidad = 0.15f;
    float yaw, pitch;

    void Start()
    {
        var e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x > 180f ? e.x - 360f : e.x;
    }

    void Update()
    {
        var k = Keyboard.current;
        var m = Mouse.current;

        if (m != null && m.rightButton.isPressed)
        {
            Vector2 d = m.delta.ReadValue();
            yaw += d.x * sensibilidad;
            pitch = Mathf.Clamp(pitch - d.y * sensibilidad, -85f, 85f);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        if (k == null) return;
        Vector3 dir = Vector3.zero;
        if (k.wKey.isPressed) dir += Vector3.forward;
        if (k.sKey.isPressed) dir += Vector3.back;
        if (k.aKey.isPressed) dir += Vector3.left;
        if (k.dKey.isPressed) dir += Vector3.right;
        if (k.eKey.isPressed) dir += Vector3.up;
        if (k.qKey.isPressed) dir += Vector3.down;
        float v = velocidad * (k.leftShiftKey.isPressed ? 3f : 1f);
        transform.Translate(dir.normalized * v * Time.deltaTime, Space.Self);
    }
}
