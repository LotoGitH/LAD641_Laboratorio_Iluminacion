using UnityEngine;

[RequireComponent(typeof(Light))]
public class ParpadeoAntorcha : MonoBehaviour
{
    [SerializeField] float intensidadBase = 3f;
    [SerializeField] float variacion = 0.6f;
    [SerializeField] float velocidad = 8f;
    Light luz;
    float semilla;

    void Awake() { luz = GetComponent<Light>(); semilla = Random.value * 100f; }

    void Update()
    {
        float ruido = Mathf.PerlinNoise(semilla, Time.time * velocidad);
        luz.intensity = intensidadBase + (ruido - 0.5f) * 2f * variacion;
    }
}
