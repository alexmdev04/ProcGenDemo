using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
//using UnityEngine.ResourceManagement.AsyncOperations;
//using UnityEngine.ResourceManagement.ResourceProviders;

public class Game : MonoBehaviour
{
    public enum Directions
    {
        north,
        northEast,
        east,
        southEast,
        south,
        southWest,
        west,
        northWest,
    }
    public LayerMask defaultLayer;
    public static Game instance { get; private set; }
    public System.Random random = new();
    public double clock;
    public bool inGame;
    public int 
        papersCollected,
        playerLives = 3;
    public Volume globalVolume;
    void Awake()
    {
        instance = this;
        globalVolume = GetComponent<Volume>();
        //Addressables.InitializeAsync();
    #if !UNITY_EDITOR
        SceneManager.LoadScene("ui", LoadSceneMode.Additive);
    #endif
    }
    void Update()
    {
        clock += Time.deltaTime;
    }
    public void Pause(bool state = false)
    {
        Time.timeScale = state ? 0 : 1;
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
        InputHandler.instance.SetActive(!state);
    }
    public void collectPage()
    {
        papersCollected++;
        uiMessage.instance.New("Papers Collected: " + papersCollected, "Game");
    }
    public void Kill()
    {
        uiMessage.instance.New("You Died!", "Game");
        if (playerLives > 3)
        {
            uiMessage.instance.New("You have " + playerLives + " lives remaining", "Game");
            uiMessage.instance.SetTimer(5);
        }
        else // GAME OVER
        {
            ui.instance.gameOver.gameObject.SetActive(true);
        }
    }
}