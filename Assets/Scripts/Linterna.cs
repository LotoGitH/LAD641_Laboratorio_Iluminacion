using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Light))]
public class Linterna : MonoBehaviour
{
    Light luz;
    void Awake() => luz = GetComponent<Light>();

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            luz.enabled = !luz.enabled;
    }
}
