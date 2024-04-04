using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
//using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Rendering;
//using UnityEngine.ResourceManagement.AsyncOperations;
//using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

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
    public double clock { get; private set; }
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
}