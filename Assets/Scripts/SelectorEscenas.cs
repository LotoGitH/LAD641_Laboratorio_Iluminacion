using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SelectorEscenas : MonoBehaviour
{
    void Update()
    {
        var k = Keyboard.current;
        if (k == null) return;
        if (k.digit1Key.wasPressedThisFrame) SceneManager.LoadScene(0);
        if (k.digit2Key.wasPressedThisFrame) SceneManager.LoadScene(1);
        if (k.digit3Key.wasPressedThisFrame) SceneManager.LoadScene(2);
        if (k.digit4Key.wasPressedThisFrame) SceneManager.LoadScene(3);
        if (k.digit5Key.wasPressedThisFrame) SceneManager.LoadScene(4);
        if (k.digit6Key.wasPressedThisFrame) SceneManager.LoadScene(5);
    }
}
