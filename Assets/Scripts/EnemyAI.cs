using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyAI : NetworkBehaviour
{
    [SerializeField] private int State; 
    // 1 = roaming 2 = chase 3 = track sound 4 = angy
    [SerializeField] private int WPC; 
    //WayPoint Choice
    [SerializeField] private int[] Path;
    [SerializeField] private NavMeshAgent NMA;
    [SerializeField] private Transform[] Waypoints;
    [SerializeField] private Transform Player;
    [SerializeField] private Transform LSP; 

    [SerializeField] private float radius;
    [Range(0,360)]
    [SerializeField] private float angle;
    [SerializeField] private GameObject PRef; // Player reference
    [SerializeField] private LayerMask Target;
    [SerializeField] private LayerMask Obstacle;
    [SerializeField] private bool PlayerIsVisible;

    private float pingTime = 5;
    [SerializeField] private float pingTimer = 5;
    
    //Last Seen Position
    




    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
        }
        
        State = 1;
        WPC = Random.Range(0,Waypoints.Length);
        NMA = GetComponent<NavMeshAgent>();
        Debug.Log(WPC);
        Debug.Log(Waypoints.Length);
        Path = new int[Waypoints.Length];
        for(int i = 0; i<Waypoints.Length; i++)
        {
            Path[i] = Random.Range(0,Waypoints.Length);
        }
        PRef = GameObject.FindGameObjectWithTag("Player");
        //StartCoroutine(FovRoutine());
        
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        pingTimer -= Time.fixedDeltaTime;
        if (pingTimer <= 0)
        {
            pingTimer = pingTime;
            FOVC();
        }
        
        
        switch(State)
        {
            case 1:
                Roaming(WPC);
                break;
            case 2:
                Chasing();
                break;
            case 3:
                Tracking();
                break;
            case 4:

                break;
        }
    }

    private void Roaming(int Dest)
    {
        NMA.destination = Waypoints[Dest].position;

        if(NMA.transform.position.x == Waypoints[Dest].position.x && NMA.transform.position.z == Waypoints[Dest].position.z)
        {
            WPC = Random.Range(0,Waypoints.Length);
            Debug.Log(WPC);
        }
        //Debug.Log("NMV");
    }

    private void Chasing()
    {
        NMA.destination = Player.position;
        Debug.Log("the hunt is on");
    }
    private void Tracking()
    {
        NMA.destination = LSP.position;
        if(NMA.transform.position.x == LSP.position.x && NMA.transform.position.z == LSP.position.z)
        {
            State = 1;
        }
    }

        private IEnumerator FovRoutine()
    {

        WaitForSeconds wait = new WaitForSeconds(0.2f);
        while(true)
        {
            yield return wait;
            FOVC();
        }
    }
    private void FOVC()
    {
        Collider[] RangeChecks = Physics.OverlapSphere(transform.position,radius,Target);
        print("Scanning through " + RangeChecks.Length + " targets");
        float shortestDist = 0;

        if(RangeChecks.Length != 0)
        {
            Transform Target = RangeChecks[0].transform;
            Vector3 TargetDirection = (Target.position - transform.position).normalized;

            if(Vector3.Angle(transform.forward,TargetDirection) < angle / 2)
            {
                float distancetotarget = Vector3.Distance(transform.position, Target.position);
                if(!Physics.Raycast(transform.position,TargetDirection,distancetotarget,Obstacle) && Target.tag == "Player")
                {
                    float dist = Vector3.Distance(transform.position, Target.position);
                    
                    if (shortestDist != 0)
                    {
                        if (shortestDist > dist)
                        {
                            shortestDist = dist;
                            Player = Target;
                        }
                    }
                    else
                    {
                        shortestDist = dist;
                        Player = Target;
                    }
                    
                    
                    PlayerIsVisible = true;
                    State = 2;
                    Debug.Log("me angy");
                }
                else
                {
                    PlayerIsVisible = false;  
                    State = 1;
                    Debug.Log("me passive");
                }
            }
            else
            {
                PlayerIsVisible = false;
                State = 1;
                Debug.Log("me confused");
            }
        } 
        else if(PlayerIsVisible)
        {
            PlayerIsVisible = false;
            State = 1;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && IsServer)
        {
            other.GetComponent<PlayerCharacterControls>().die();
        }
    }
}
