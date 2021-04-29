using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    Transform playerTransform;
    Transform parentTransform;
    [SerializeField] float clampRangeX = 8f;
    [SerializeField] float clampRangeZ = 8f;
    [SerializeField] float clampRangeXNeg = -8f;
    [SerializeField] float clampRangeZNeg = -8f;

    [SerializeField] float movePerScroll = 2.5f;
    [SerializeField] float fovClampMin = 50;
    [SerializeField] float fovClampMax = 80;

    private Vector3 cameraOffset;

    [Range(0.01f, 1.0f)]
    [SerializeField] public float smoothFactor;

    [SerializeField] float shakeDuration;
    [SerializeField] float shakeIntensity;

    void Start()
    {
        parentTransform = transform.parent;
        playerTransform = FindObjectOfType<Player>().transform;
        cameraOffset = parentTransform.position - playerTransform.position;
    }

    void LateUpdate()
    {
        if (playerTransform)
        {
            Vector3 newPos = playerTransform.position + cameraOffset;
            Vector3 coords;
            coords = Vector3.Slerp(transform.position, newPos, smoothFactor);
            coords.x = Mathf.Clamp(coords.x, clampRangeXNeg, clampRangeX);
            coords.y = Mathf.Clamp(coords.y, 14.1f, 14.1f);
            coords.z = Mathf.Clamp(coords.z, clampRangeZNeg, clampRangeZ);
            parentTransform.position = coords;
        }
    }

    public void Shake()
    {
        StartCoroutine(ShakeCamera(shakeDuration, shakeIntensity));
    }

    IEnumerator ShakeCamera(float duration, float intensity)
    {
        Vector3 originalPos = transform.localPosition;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;

            transform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPos;
    }
}
