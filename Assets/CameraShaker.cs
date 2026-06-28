using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance { get; private set; }
    private Vector3 originalPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        if(Instance == null)Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        originalPos = transform.position;
    }

    public void Shake(float duration = 0.08f,float magnitude = 0.15f) 
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }
    private IEnumerator ShakeRoutine(float duration,float magnitude) 
    {
        float elapsed = 0.0f;
        while (elapsed < duration) 
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.position = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;

        }
        transform.position = originalPos;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
