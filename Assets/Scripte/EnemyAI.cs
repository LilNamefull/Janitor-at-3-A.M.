using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class enemyAI : MonoBehaviour
{
    public NavMeshAgent ai;
    public List<Transform> destinations;
    public Animator aiAnim;
    public float walkSpeed, chaseSpeed, minIdleTime, maxIdleTime, idleTime, detectionDistance, catchDistance, sightRayLength;
    public bool walking, chasing;
    public Transform player;
    Transform currentDest;
    Vector3 dest;
    public Vector3 rayCastOffset;
    public string deathScene;
    public float aiDistance;
    public GameObject hideText, stopHideText;

    public float fieldOfViewAngle = 110f; // Der Sichtwinkel des Monsters
    public float maxDetectionDistance = 15f; // Maximale Entdeckungsdistanz

    void Start()
    {
        walking = true;
        currentDest = destinations[Random.Range(0, destinations.Count)];
    }

    void Update()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        aiDistance = Vector3.Distance(player.position, this.transform.position);

        // Überprüfen, ob der Spieler im Sichtfeld des Monsters ist
        if (IsPlayerInSight())
        {
            // Wenn der Spieler im Sichtbereich ist, sofort in den Jagdmodus (chasing) übergehen
            if (!chasing)
            {
                chasing = true;
                walking = false; // Stoppt das Laufen zu einem Zielpunkt
                StopCoroutine("stayIdle"); // Stopp die Idle-Routine
                ai.speed = chaseSpeed; // Setzt die Geschwindigkeit auf Jagdgeschwindigkeit
                aiAnim.SetTrigger("sprint"); // Aktiviert die Sprint-Animation
                aiAnim.ResetTrigger("walk");
                aiAnim.ResetTrigger("idle");
            }

            // Setze das Ziel auf den Spieler
            ai.destination = player.position;
        }

        // Wenn das Monster den Spieler verfolgt
        if (chasing)
        {
            dest = player.position;
            ai.destination = dest;  // Setzt das Ziel auf den Spieler
            ai.speed = chaseSpeed;  // Jagdgeschwindigkeit wird verwendet
            aiAnim.ResetTrigger("walk");
            aiAnim.ResetTrigger("idle");
            aiAnim.SetTrigger("sprint");

            if (aiDistance <= catchDistance)
            {
                player.gameObject.SetActive(false); // Spielergesicht deaktivieren (Player stirbt)
                aiAnim.ResetTrigger("walk");
                aiAnim.ResetTrigger("idle");
                aiAnim.ResetTrigger("sprint");
                aiAnim.SetTrigger("jumpscare");
                StartCoroutine(deathRoutine());
                chasing = false;
            }
        }

        // Wenn das Monster geht (Idle-Phase) und nicht jagt
        if (walking)
        {
            dest = currentDest.position;
            ai.destination = dest;
            ai.speed = walkSpeed;
            aiAnim.ResetTrigger("sprint");
            aiAnim.ResetTrigger("idle");
            aiAnim.SetTrigger("walk");
            if (ai.remainingDistance <= ai.stoppingDistance)
            {
                aiAnim.ResetTrigger("sprint");
                aiAnim.ResetTrigger("walk");
                aiAnim.ResetTrigger("idle");
                aiAnim.SetTrigger("idle");
                ai.speed = 0;
                StopCoroutine("stayIdle");
                StartCoroutine("stayIdle");
                walking = false;
            }
        }
    }

    // Überprüft, ob der Spieler im Sichtbereich des Monsters ist (Kegel)
    bool IsPlayerInSight()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // Debugging: Zeige den Raycast im Editor
        Debug.DrawRay(transform.position + rayCastOffset, directionToPlayer * sightRayLength, Color.green);

        // Überprüfe, ob der Spieler innerhalb des Sichtwinkels des Monsters ist
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        Debug.Log("Angle: " + angle); // Debugging: Überprüfen des Winkels

        // Wenn der Spieler innerhalb des Sichtwinkels ist
        if (angle < fieldOfViewAngle / 2)
        {
            RaycastHit hit;
            // Starte den Raycast von der Position des Monsters mit einem Offset, in die Richtung des Spielers
            if (Physics.Raycast(transform.position + rayCastOffset, directionToPlayer, out hit, sightRayLength))
            {
                // Debugging: Zeige den Treffer des Raycasts im Editor
                Debug.DrawRay(transform.position + rayCastOffset, directionToPlayer * hit.distance, Color.red);

                if (hit.collider.gameObject.tag == "Player")
                {
                    // Debugging: Spieler erkannt
                    Debug.Log("Player detected!");
                    return true; // Spieler wird erkannt
                }
                else
                {
                    // Debugging: Raycast hat etwas anderes getroffen
                    Debug.Log("Raycast hit something else: " + hit.collider.gameObject.name);
                }
            }
        }
        // Spieler ist nicht im Sichtbereich
        return false;
    }

    public void stopChase()
    {
        walking = true;
        chasing = false;
        StopCoroutine("chaseRoutine");
        currentDest = destinations[Random.Range(0, destinations.Count)];
    }

    IEnumerator stayIdle()
    {
        idleTime = Random.Range(minIdleTime, maxIdleTime);
        yield return new WaitForSeconds(idleTime);
        walking = true;
        currentDest = destinations[Random.Range(0, destinations.Count)];
    }

    IEnumerator deathRoutine()
    {
        yield return new WaitForSeconds(2f);  // Warte für einen Moment, bevor der Todesscreen geladen wird
        SceneManager.LoadScene(deathScene);
    }
}
