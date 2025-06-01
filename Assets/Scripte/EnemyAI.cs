using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class enemyAI : MonoBehaviour
{
    [Header("Agent & Ziele")]
    public NavMeshAgent ai;
    public List<Transform> destinations;

    [Header("Animationen")]
    public Animator aiAnim;
    public float walkSpeed = 2f;
    public float chaseSpeed = 5f;

    [Header("Idle-Timing")]
    public float minIdleTime = 1f;
    public float maxIdleTime = 3f;
    private float idleTime;

    [Header("Spieler & Jagd")]
    public Transform player;
    public float catchDistance = 1.5f;
    public float fieldOfViewAngle = 110f;    // Sichtwinkel des Monsters
    public float sightRayLength = 15f;     // Raycast-Distanz
    public Vector3 rayCastOffset;            // Höhenversatz der Raycasts

    [Header("Sonstige")]
    public string deathScene;
    public GameObject hideText, stopHideText;

    [Header("Distanzausgleich")]
    [Tooltip("Wenn das Monster weiter als X Meter vom Spieler entfernt ist, wähle sofort ein Ziel in Spieler-Nähe.")]
    public float maxAllowedDistance = 150f;

    // interner Timer-Wert, um nicht dauernd neu zu wählen
    private float timeSinceForcedRepath = 0f;
    private float forcedRepathCooldown = 2f;
    // (damit wir nicht in jedem Frame neu zum nächsten Ziel springen, 
    //  sondern erst nach ein paar Sekunden erneut prüfen)

    public bool walking = true;
    public bool chasing = false;

    private Transform currentDest;
    private Vector3 dest;
    private int lastDestIndex = -1;
    private float aiDistance;

    void Start()
    {
        walking = true;
        chasing = false;

        // 1) Erster Zufallsziel-Index
        if (destinations.Count > 0)
        {
            int r = Random.Range(0, destinations.Count);
            currentDest = destinations[r];
            lastDestIndex = r;
        }

        // 2) Timer initialisieren
        timeSinceForcedRepath = forcedRepathCooldown;
    }

    void Update()
    {
        // Abstand zum Spieler berechnen
        aiDistance = Vector3.Distance(player.position, transform.position);

        Debug.Log($"[enemyAI] Distance to player: {aiDistance:F2} meters");

        // 1) Forced Repath: Wenn zu weit weg vom Spieler UND nicht gerade in Jagd
        if (!chasing)
        {
            timeSinceForcedRepath += Time.deltaTime;
            if (aiDistance > maxAllowedDistance && timeSinceForcedRepath >= forcedRepathCooldown)
            {
                // Wähle das Destination, das dem Spieler am nächsten ist
                ChooseNearestDestinationToPlayer();
                // Setze walking-Flag und Animation
                walking = true;
                ai.speed = walkSpeed;
                aiAnim.ResetTrigger("sprint");
                aiAnim.ResetTrigger("idle");
                aiAnim.SetTrigger("walk");

                // Timer zurücksetzen
                timeSinceForcedRepath = 0f;
            }
        }

        // 2) Sichtfeld‐Check: Wenn der Spieler im Sichtkegel erkannt wird, sofort jagd
        if (IsPlayerInSight() && !chasing)
        {
            chasing = true;
            walking = false;
            StopCoroutine("stayIdle");

            ai.speed = chaseSpeed;
            aiAnim.SetTrigger("sprint");
            aiAnim.ResetTrigger("walk");
            aiAnim.ResetTrigger("idle");
        }

        // 3) Jagdlogik
        if (chasing)
        {
            ai.destination = player.position;
            ai.speed = chaseSpeed;
            aiAnim.ResetTrigger("walk");
            aiAnim.ResetTrigger("idle");
            aiAnim.SetTrigger("sprint");

            if (aiDistance <= catchDistance)
            {
                player.gameObject.SetActive(false);
                aiAnim.ResetTrigger("walk");
                aiAnim.ResetTrigger("idle");
                aiAnim.ResetTrigger("sprint");
                aiAnim.SetTrigger("jumpscare");
                StartCoroutine(deathRoutine());
                chasing = false;
            }
        }
        // 4) Wandern/Idle, wenn nicht in Jagd
        else if (walking)
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

    // Wählt das Ziel (aus destinations), das zum Spieler den geringsten Abstand hat
    private void ChooseNearestDestinationToPlayer()
    {
        if (destinations.Count == 0) return;

        float bestDist = float.MaxValue;
        int bestIndex = lastDestIndex;

        for (int i = 0; i < destinations.Count; i++)
        {
            float d = Vector3.Distance(player.position, destinations[i].position);
            if (d < bestDist)
            {
                bestDist = d;
                bestIndex = i;
            }
        }

        currentDest = destinations[bestIndex];
        lastDestIndex = bestIndex;
    }

    // Idle‐Routine: Nach zufälliger Wartezeit neues Zufallsziel wählen
    IEnumerator stayIdle()
    {
        idleTime = Random.Range(minIdleTime, maxIdleTime);
        yield return new WaitForSeconds(idleTime);

        walking = true;

        // Zufallsziel, das sich vom zuletzt gewählten unterscheidet (falls möglich)
        int newIndex = lastDestIndex;
        if (destinations.Count > 1)
        {
            while (newIndex == lastDestIndex)
            {
                newIndex = Random.Range(0, destinations.Count);
            }
        }
        lastDestIndex = newIndex;
        currentDest = destinations[newIndex];
    }

    // Prüft, ob der Spieler innerhalb des Sichtkegels (Field of View) ist
    bool IsPlayerInSight()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        Debug.DrawRay(transform.position + rayCastOffset, dirToPlayer * sightRayLength, Color.green);

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        // Debug.Log("Angle: " + angle);
        if (angle < fieldOfViewAngle / 2f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + rayCastOffset, dirToPlayer, out hit, sightRayLength))
            {
                Debug.DrawRay(transform.position + rayCastOffset, dirToPlayer * hit.distance, Color.red);
                if (hit.collider.CompareTag("Player"))
                {
                    // Debug.Log("Player detected!");
                    return true;
                }
                // else Debug.Log("Raycast hit something else: " + hit.collider.name);
            }
        }
        return false;
    }

    public void stopChase()
    {
        walking = true;
        chasing = false;
        StopCoroutine("chaseRoutine");

        // Direkt eine neue Zufallsdestination (nicht zuletzt gewählte)
        int newIndex = lastDestIndex;
        if (destinations.Count > 1)
        {
            while (newIndex == lastDestIndex)
            {
                newIndex = Random.Range(0, destinations.Count);
            }
        }
        lastDestIndex = newIndex;
        currentDest = destinations[newIndex];
    }

    IEnumerator deathRoutine()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(deathScene);
    }
}
