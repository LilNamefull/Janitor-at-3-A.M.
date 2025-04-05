using UnityEngine;

public class MonsterDoorTrigger : MonoBehaviour
{
    public Transform doorPivot;         // Hier ziehst du dein Tür-Pivot (z. B. das Scharnier)
    public float openAngle = 90f;       // Um wie viel Grad soll die Tür aufgehen
    public float closeAngle = 0f;       // Die geschlossene Position
    public float openSpeed = 2f;        // Wie schnell sich die Tür öffnet
    public string monsterTag = "Monster";

    private bool monsterInRange = false;

    void Update()
    {
        float targetAngle = monsterInRange ? openAngle : closeAngle;

        Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

        doorPivot.localRotation = Quaternion.Lerp(
            doorPivot.localRotation,
            targetRotation,
            Time.deltaTime * openSpeed
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(monsterTag))
        {
            Debug.Log("Monster hat Trigger betreten");
            monsterInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(monsterTag))
        {
            Debug.Log("Monster hat Trigger verlassen");
            monsterInRange = false;
        }
    }
}
