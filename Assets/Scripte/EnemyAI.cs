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
    public float fieldOfViewAngle = 110f;    // Sichtwinkel
    public float sightRayLength = 15f;     // Raycast-Distanz
    public Vector3 rayCastOffset;             // Raycast-Höhenoffset

    [Header("Sonstige")]
    public string deathScene;
    public GameObject hideText, stopHideText;

    [Header("Distanzausgleich")]
    [Tooltip("Abstand (Meter), ab dem das Monster zum Spieler-nächsten Ziel wechselt")]
    public float maxAllowedDistance = 100f;

    // Kleiner Cooldown, damit Forced Repath nicht in jedem Frame erneut aktiviert wird
    private float timeSinceForcedRepath = 0f;
    private float forcedRepathCooldown = 2f;

    public bool walking = true;
    public bool chasing = false;

    private Transform currentDest;
    private Vector3 dest;
    private int lastDestIndex = -1;
    private float aiDistance;

    // Wenn true, läuft das Monster gerade auf das erzwungene Ziel in Spieler-Nähe
    private bool forcedTargetActive = false;

    // Öffentliche Eigenschaft, um von außen (z. B. hidingPlace) zu prüfen, ob gejagt wird
    public bool IsChasing
    {
        get { return chasing; }
    }

    void Start()
    {
        walking = true;
        chasing = false;
        forcedTargetActive = false;

        // Erstes zufälliges Ziel auswählen
        if (destinations.Count > 0)
        {
            int r = Random.Range(0, destinations.Count);
            currentDest = destinations[r];
            lastDestIndex = r;
        }
        timeSinceForcedRepath = forcedRepathCooldown;
    }

    void Update()
    {
        // Abstand zum Spieler berechnen
        aiDistance = Vector3.Distance(player.position, transform.position);

        //  –– DEBUG: Abstand ausgeben ––
        Debug.Log($"[enemyAI] Distance to player: {aiDistance:F2} m");

        // 1) Forced Repath (nur wenn nicht jagen und kein erzwungenes Ziel aktiv)
        if (!chasing && !forcedTargetActive)
        {
            timeSinceForcedRepath += Time.deltaTime;
            if (aiDistance > maxAllowedDistance && timeSinceForcedRepath >= forcedRepathCooldown)
            {
                // Wähle das Ziel, das dem Spieler am nächsten ist
                ChooseNearestDestinationToPlayer();
                forcedTargetActive = true;   // Merke: jetzt ist ein erzwungenes Ziel aktiv

                // Monster in Wander‐Modus versetzen (Animation & Speed)
                walking = true;
                ai.speed = walkSpeed;
                aiAnim.ResetTrigger("sprint");
                aiAnim.ResetTrigger("idle");
                aiAnim.SetTrigger("walk");

                timeSinceForcedRepath = 0f;

                // SOFORTRÜCKKEHR, damit wir nicht im selben Frame in die Idle-Logik gelangen
                return;
            }
        }

        // 2) Sichtfeld‐Check: Wird der Spieler im Kegel erkannt?
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

        // 3) Jagd‐Logik
        if (chasing)
        {
            ai.destination = player.position;
            ai.speed = chaseSpeed;
            aiAnim.ResetTrigger("walk");
            aiAnim.ResetTrigger("idle");
            aiAnim.SetTrigger("sprint");

            if (aiDistance <= catchDistance)
            {
                // Spieler „packen“
                player.gameObject.SetActive(false);
                aiAnim.ResetTrigger("walk");
                aiAnim.ResetTrigger("idle");
                aiAnim.ResetTrigger("sprint");
                aiAnim.SetTrigger("jumpscare");
                StartCoroutine(deathRoutine());
                chasing = false;
            }
        }
        // 4) Wander/Idle, wenn nicht in Jagd
        else if (walking)
        {
            dest = currentDest.position;
            ai.destination = dest;
            ai.speed = walkSpeed;
            aiAnim.ResetTrigger("sprint");
            aiAnim.ResetTrigger("idle");
            aiAnim.SetTrigger("walk");

            // Sobald wir am Ziel sind:
            if (ai.remainingDistance <= ai.stoppingDistance)
            {
                // Wenn das Ziel durch Forced Repath gewählt wurde:
                if (forcedTargetActive)
                {
                    // Erzwungenes Ziel ist erreicht – abgeschlossene Forced Repath
                    forcedTargetActive = false;
                }

                // Animation auf Idle
                aiAnim.ResetTrigger("sprint");
                aiAnim.ResetTrigger("walk");
                aiAnim.ResetTrigger("idle");
                aiAnim.SetTrigger("idle");
                ai.speed = 0;

                // Starte die zufällige Idle‐Phase (neues Zufallsziel abseits Forced Repath)
                StopCoroutine("stayIdle");
                StartCoroutine("stayIdle");
                walking = false;
            }
        }
    }

    /// <summary>
    /// Wählt aus `destinations` jenes Transform aus, das aktuell den kürzesten Abstand zum Spieler besitzt.
    /// </summary>
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

    /// <summary>
    /// Idle‐Routine: Das Monster wartet eine zufällige Zeit und wählt dann ein neues,
    /// zufälliges Ziel (kein Forced Repath).
    /// </summary>
    IEnumerator stayIdle()
    {
        idleTime = Random.Range(minIdleTime, maxIdleTime);
        yield return new WaitForSeconds(idleTime);

        walking = true;
        // Neues, anderes Zufallsziel (wenn möglich)
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

    /// <summary>
    /// Prüft, ob der Spieler innerhalb des Sichtkegels liegt und nicht durch Hindernisse verdeckt ist.
    /// </summary>
    bool IsPlayerInSight()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        Debug.DrawRay(transform.position + rayCastOffset, dirToPlayer * sightRayLength, Color.green);

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle < fieldOfViewAngle / 2f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + rayCastOffset, dirToPlayer, out hit, sightRayLength))
            {
                Debug.DrawRay(transform.position + rayCastOffset, dirToPlayer * hit.distance, Color.red);
                if (hit.collider.CompareTag("Player"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Stoppt die aktuelle Verfolgung (wenn nötig) und wählt ein neues Zufallsziel (kein Forced).
    /// </summary>
    public void stopChase()
    {
        walking = true;
        chasing = false;
        StopCoroutine("chaseRoutine");

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
