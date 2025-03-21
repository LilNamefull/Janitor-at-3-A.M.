using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    private bool isOpen = false;
    public float openAngle = 90f;
    public float speed = 3f;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
    }

    public void ToggleDoor()
    {
        StopAllCoroutines();
        StartCoroutine(RotateDoor(isOpen ? closedRotation : openRotation));
        isOpen = !isOpen;
    }

    private IEnumerator RotateDoor(Quaternion targetRotation)
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
            yield return null;
        }
        transform.rotation = targetRotation;
    }
}
