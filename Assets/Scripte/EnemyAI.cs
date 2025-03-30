using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent ai;
    public List<Transform> destinations;
    public Animator aiAnim;
    public float walkSpeed, chaseSpeed, minIdleTime, maxIdleTime, idleTime, sightDistance, catchDistance, chaseTime, minChaseTime, maxChaseTime, jumpscareTime;
    public bool walking, chasing;
    public Transform player;
    Transform currentDest;
    Vector3 dest;
    int randNum;
    public int destinationAmount;
    public Vector3 rayCastOffset;
    public string deathScene;

    void Start()
    {
        walking = true;
        randNum = Random.Range(0, destinations.Count);
        currentDest = destinations[randNum];
        Debug.Log("Enemy started. First destination: " + currentDest.name);
    }

    void Update()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        RaycastHit hit;

        //  Zeichnet den Raycast in der Scene-Ansicht
        Debug.DrawRay(transform.position + rayCastOffset, direction * sightDistance, Color.red);

        if (Physics.Raycast(transform.position + rayCastOffset, direction, out hit, sightDistance))
        {
            Debug.Log("Raycast hit: " + hit.collider.gameObject.name); // Zeigt an, welches Objekt getroffen wurde

            if (hit.collider.gameObject.tag == "Player")
            {
                Debug.Log("Player detected! Starting chase."); // Debug, falls der Spieler erkannt wird
                walking = false;
                StopCoroutine("stayIdle");
                StopCoroutine("chaseRoutine");
                StartCoroutine("chaseRoutine");
                chasing = true;
            }
        }

        if (chasing == true)
        {
            Debug.Log("Chasing player...");
            dest = player.position;
            ai.destination = dest;
            ai.speed = chaseSpeed;
            aiAnim.ResetTrigger("walk");
            aiAnim.ResetTrigger("idle");
            aiAnim.SetTrigger("sprint");

            float distance = Vector3.Distance(player.position, ai.transform.position);
            Debug.Log("Distance to player: " + distance);

            if (distance <= catchDistance)
            {
                Debug.Log("Player caught! Triggering jumpscare...");
                player.gameObject.SetActive(false);
                aiAnim.ResetTrigger("walk");
                aiAnim.ResetTrigger("idle");
                aiAnim.ResetTrigger("sprint");
                aiAnim.SetTrigger("jumpscare");
                StartCoroutine(deathRoutine());
                chasing = false;
            }
        }

        if (walking == true)
        {
            Debug.Log("Walking to destination: " + currentDest.name);
            dest = currentDest.position;
            ai.destination = dest;
            ai.speed = walkSpeed;
            aiAnim.ResetTrigger("sprint");
            aiAnim.ResetTrigger("idle");
            aiAnim.SetTrigger("walk");

            if (ai.remainingDistance <= ai.stoppingDistance)
            {
                Debug.Log("Reached destination, idling...");
                aiAnim.ResetTrigger("sprint");
                aiAnim.ResetTrigger("walk");
                aiAnim.SetTrigger("idle");
                ai.speed = 0;
                StopCoroutine("stayIdle");
                StartCoroutine("stayIdle");
                walking = false;
            }
        }
    }

    IEnumerator stayIdle()
    {
        idleTime = Random.Range(minIdleTime, maxIdleTime);
        Debug.Log("Idling for " + idleTime + " seconds...");
        yield return new WaitForSeconds(idleTime);
        walking = true;
        randNum = Random.Range(0, destinations.Count);
        currentDest = destinations[randNum];
        Debug.Log("New destination chosen: " + currentDest.name);
    }

    IEnumerator chaseRoutine()
    {
        chaseTime = Random.Range(minChaseTime, maxChaseTime);
        Debug.Log("Chasing player for " + chaseTime + " seconds...");
        yield return new WaitForSeconds(chaseTime);
        walking = true;
        chasing = false;
        randNum = Random.Range(0, destinations.Count);
        currentDest = destinations[randNum];
        Debug.Log("Chase ended, returning to random destination: " + currentDest.name);
    }

    IEnumerator deathRoutine()
    {
        Debug.Log("Jumpscare triggered, loading death scene in " + jumpscareTime + " seconds...");
        yield return new WaitForSeconds(jumpscareTime);
        SceneManager.LoadScene(deathScene);
    }
}
