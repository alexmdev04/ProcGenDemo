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
        maxSpeed,
        damagePushIntensity = 2f;
    void Awake()
    {
        instance = this;
        agent = GetComponent<NavMeshAgent>();
    }
    void Update()
    {
        if (!MazeNavMesh.instance.initialBakeComplete) { return; }
        agent.destination = Player.instance.transform.position;
        agent.speed = Mathf.Lerp(minSpeed, maxSpeed, Game.instance.papersCollected / 8f);
    }
    void OnCollisionEnter(Collision collision)
    { 
        if (collision.gameObject.TryGetComponent(out Player player)) 
        { 
            player.rb.AddForce(transform.forward * damagePushIntensity, ForceMode.Impulse);
            Game.instance.AttackPlayer();
        }
    }
    public void RandomPosition()
    {
        transform.position = 
            MazeRenderer.instance.loadedMazePieces[Game.instance.random.Next(MazeRenderer.instance.loadedMazePieces.Count)]
            .mazePiece.gridIndex.GridIndexToWorldPosition(2) + new Vector3(5, 0, 5);
    }
}