using UnityEngine;

public class Rotador : MonoBehaviour
{
    [SerializeField] float velocidadGiro = 45f;   // grados por segundo
    [SerializeField] float amplitud = 0.5f;       // altura de flotación
    Vector3 inicio;

    void Start() => inicio = transform.position;

    void Update()
    {
        transform.Rotate(0f, velocidadGiro * Time.deltaTime, 0f);
        transform.position = inicio + Vector3.up * Mathf.Sin(Time.time) * amplitud;
    }
}
