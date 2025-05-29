using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class LoadSceneOnTrigger : MonoBehaviour
{
    [Tooltip("Name der Szene, die geladen werden soll")]
    public string sceneName = "SchoolExplore";

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        // Rigidbody für Trigger nötig (zum Fire OnTriggerEnter)
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[LoadSceneOnTrigger] Trigger Entered by: {other.name}");
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[LoadSceneOnTrigger] Loading scene: {sceneName}");
            if (Application.CanStreamedLevelBeLoaded(sceneName))
                SceneManager.LoadScene(sceneName);
            else
                Debug.LogError($"[LoadSceneOnTrigger] Szene '{sceneName}' nicht in Build Settings gefunden!");
        }
    }
}
