using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ui : MonoBehaviour
{
    public static ui instance { get; private set; }
    public uiSettings settings { get; private set; }
    //public List<uiObjective> 
    //    uiObjectives;
    public bool 
        uiFadeToBlack;
    //[SerializeField] GameObject 
    //    uiObjectivePrefab,
    //    uiObjectivesParent;
    [SerializeField] TextMeshProUGUI 
        uiSpeedometer;
    [SerializeField] Image 
        uiFade;
    [SerializeField] float 
        uiFadeInSpeed,
        uiFadeOutSpeed;
    public float uiFadeAlpha;
    const string
        uiLevelNumText = "Level ",
        uiSectionNumText = "Section ",
        uiLevelNumDashText = "-";
    void Awake()
    {
        instance = this;
        settings = GetComponentInChildren<uiSettings>();
        settings.gameObject.SetActive(false);
        //LevelLoader.instance.levelLoaded.AddListener(Refresh);
    }
    void Start()
    {
        //InvokeRepeating(nameof(uiObjectivesRefresh), 1f, 1f);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { settings.gameObject.SetActive(!settings.gameObject.activeSelf); }
        uiFadeUpdate();
        uiSpeedometerUpdate();
    }
    void Pause()
    {

    }
    /// <summary>
    /// Updates the color of the ui fade element on screen used to hide the screen, uiFadeToBlack controls the direction of the fade
    /// </summary>
    void uiFadeUpdate()
    {
        uiFade.color = new Color(0, 0, 0, System.Math.Clamp(uiFade.color.a + (uiFadeToBlack ? Time.deltaTime * uiFadeInSpeed : -Time.deltaTime * uiFadeOutSpeed), 0f, 1f));
        uiFadeAlpha = uiFade.color.a;
    }
    void uiSpeedometerUpdate()
    {
        uiSpeedometer.text = (Player.instance.playerSpeed > Player.instance.movementSpeedReadOnly ? "<color=green>" : "") +
            "<line-height=40%>" + Player.instance.playerSpeed + "\n" + "<size=50%>m/s";
    }
    public void ToggleSpeedometer()
    {
        uiSpeedometer.gameObject.SetActive(!uiSpeedometer.gameObject.activeSelf);
    }
}