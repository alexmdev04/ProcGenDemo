using System;
using TMPro;
using UnityEngine;

public class uiGameOver : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI 
        papersCollectedText,
        timeAliveText;
    bool skippedOnEnable = false;

    void OnEnable()
    {
        if (!skippedOnEnable) { skippedOnEnable = true; return; }
        Game.instance.Pause(true);
        papersCollectedText.text = "Papers Collected: " + Game.instance.papersCollected + "/8";
        timeAliveText.text = "Time Alive: " + TimeSpan.FromSeconds(Game.instance.clock).TotalSeconds.ToString() + "s";
    }
    void OnDisable()
    {
        Game.instance.Pause(false);
    }
    public void Quit()
    {
        uiDebugConsole.instance.InternalCommandCall("exit");
    }
    public void Reset()
    {
        uiDebugConsole.instance.InternalCommandCall("reset");
        Game.instance.clock = 0;
        gameObject.SetActive(false);
    }
}
