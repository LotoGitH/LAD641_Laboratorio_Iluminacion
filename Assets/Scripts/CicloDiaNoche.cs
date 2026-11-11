using UnityEngine;

[RequireComponent(typeof(Light))]
public class CicloDiaNoche : MonoBehaviour
{
    [SerializeField] float segundosPorDia = 60f;
    [SerializeField] Gradient colorDelSol;           // amanecer → mediodía → atardecer
    [SerializeField] AnimationCurve intensidad = AnimationCurve.EaseInOut(0, 0, 1, 1.2f);
    Light sol;
    float t;   // 0 a 1 = un día completo

    void Awake() => sol = GetComponent<Light>();

    void Update()
    {
        t = (t + Time.deltaTime / segundosPorDia) % 1f;
        transform.rotation = Quaternion.Euler(t * 360f, -30f, 0f);   // 0 = amanecer, 0.25 = mediodía, 0.5 = atardecer
        float altura = Mathf.Clamp01(Mathf.Sin(t * Mathf.PI * 2f));   // 0 de noche
        sol.intensity = intensidad.Evaluate(altura);
        if (colorDelSol != null) sol.color = colorDelSol.Evaluate(altura);
    }
}
