using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    public static Enemy instance { get ; private set; }
    NavMeshAgent 
        agent;
    [SerializeField] float 
        minSpeed,
        maxSpeed;
    void Awake()
    {
        instance = this;
        agent = GetComponent<NavMeshAgent>();
    }
    void OnCollisionEnter(Collision collision)
    { 
        if (collision.gameObject.TryGetComponent(out Player player)) 
        { 
            uiDebugConsole.instance.InternalCommandCall("kill");
        }
    }
    void Update()
    {
        if (!MazeNavMesh.instance.initialBakeComplete) { return; }
        agent.destination = Player.instance.transform.position;
        agent.speed = Mathf.Lerp(minSpeed, maxSpeed, Game.instance.papersCollected / 8f);
    }
}
