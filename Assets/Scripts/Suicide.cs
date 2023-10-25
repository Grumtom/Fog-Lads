using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Suicide : NetworkBehaviour
{
    [SerializeField] private bool collisionDeath = false;
    [SerializeField] private GameObject collisionSpawn;
    [SerializeField] private float deathTime = 1;
    [SerializeField] private float deathTimer = 0;
    [SerializeField] private float speed;

    private float spawnTime = 1;
    private float spawnTimer = 0.3f;
    
    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }
        deathTimer = deathTime;
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            spawnTimer -= Time.fixedDeltaTime;
            deathTimer -= Time.fixedDeltaTime;
            if (deathTimer <= 0 && deathTime > 0)
            {
                GetComponent<NetworkObject>().Despawn(true);
            }
            
            if (collisionDeath)
            {
                transform.position += transform.forward * speed;
                
                if (spawnTimer <= 0)
                {
                    GetComponent<Collider>().enabled = true;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        print("Col");
        if (IsServer && collisionDeath)
        {
            GameObject spawn = Instantiate(collisionSpawn, transform.position, transform.rotation);
            spawn.GetComponent<NetworkObject>().Spawn();
            if(IsSpawned){GetComponent<NetworkObject>().Despawn(true);}
        }
    }

}
