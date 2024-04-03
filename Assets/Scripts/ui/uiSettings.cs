using TMPro;
using UnityEngine;

public class uiSettings : MonoBehaviour
{
    [SerializeField] TMP_InputField sensitivityInputField;
    void OnEnable()
    {
        InputHandler.instance.SetActive(false);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        sensitivityInputField.text = Player.instance.lookSensitivity.y.ToString();
    }
    void OnDisable()
    {
        InputHandler.instance.SetActive(true);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void Quit()
    {
        uiDebugConsole.instance.InternalCommandCall("exit");
    }
    public void Resume()
    {
        gameObject.SetActive(false);
    
    }
    public void Menu()
    {
        uiDebugConsole.instance.InternalCommandCall("menu");
        Resume();
    }
    public void SetSensitivity()
    {
        if (float.TryParse(sensitivityInputField.text, out float sensitivity))
        {
            sensitivity = System.Math.Clamp(sensitivity, 0.0001f, 100000f);
            Player.instance.lookSensitivity = new(sensitivity, sensitivity);
        }
    }
}
