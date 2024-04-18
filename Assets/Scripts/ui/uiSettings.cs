using TMPro;
using UnityEngine;

public class uiSettings : MonoBehaviour
{
    [SerializeField] TMP_InputField 
        sensitivityInputField,
        renderDistanceInputField,
        mazeSizeXInputField,
        mazeSizeZInputField;
    public GameObject 
        resetMessage;
    bool skipOnEnable = false;

    void OnEnable()
    {
        if (!skipOnEnable) { skipOnEnable = true; return; }
        Game.instance.Pause(true);
        sensitivityInputField.text = Player.instance.lookSensitivity.y.ToString();
        renderDistanceInputField.text = MazeRenderer.instance.renderDistance.ToString();
        mazeSizeXInputField.text = MazeGen.instance.mazeSizeX.ToString();
        mazeSizeZInputField.text = MazeGen.instance.mazeSizeZ.ToString();
    }
    void OnDisable()
    {
        Game.instance.Pause(false);
    }
    public void Quit()
    {
        uiDebugConsole.instance.InternalCommandCall("exit");
    }
    public void Resume()
    {
        gameObject.SetActive(false);
    }
    public void Reset()
    {
        uiDebugConsole.instance.InternalCommandCall("reset");
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
    public void SetRenderDistance()
    {
        if (int.TryParse(renderDistanceInputField.text, out int renderDistance))
        {
            renderDistance = System.Math.Clamp(renderDistance, 1, 25);
            MazeRenderer.instance.SetRenderDistance(renderDistance);
        }
    }
    public void SetMazeSizeX()
    {
        if (int.TryParse(mazeSizeXInputField.text, out int mazeSizeX))
        {
            mazeSizeX = System.Math.Clamp(mazeSizeX, 2, 10000);
            MazeGen.instance.mazeSizeX = mazeSizeX;
            resetMessage.SetActive(true);
        }
    }
    public void SetMazeSizeZ()
    {
        if (int.TryParse(mazeSizeZInputField.text, out int mazeSizeZ))
        {
            mazeSizeZ = System.Math.Clamp(mazeSizeZ, 2, 10000);
            MazeGen.instance.mazeSizeZ = mazeSizeZ;
            resetMessage.SetActive(true);
        }
    }
}
