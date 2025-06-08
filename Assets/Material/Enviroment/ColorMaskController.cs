using UnityEngine;
using System.Collections;

public class ColorMaskController : MonoBehaviour
{
    public GameObject[] targets;
    public float revealAmount = 1.0f;
    public Material material;
    public Camera cam;

    const int MAX_CENTERS = 32;
    private Vector4[] centers = new Vector4[MAX_CENTERS];
    public float[] radii = new float[MAX_CENTERS];
    private float[] displayRadii = new float[MAX_CENTERS];
    private Coroutine[] animCoroutines = new Coroutine[MAX_CENTERS];

    private bool[] inCamera = new bool[MAX_CENTERS];
    public float maxRadius = 0.33f;
    public float animationDuration = 0.3f;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void Update()
    {
        if (cam == null || material == null) return;

        material.SetFloat("_RevealAmount", revealAmount);

        int count = Mathf.Min(targets.Length, MAX_CENTERS);
        for (int i = 0; i < count; i++)
        {
            if (targets[i] != null)
            {
                Vector3 viewportPos = cam.WorldToViewportPoint(targets[i].transform.position);
                centers[i] = new Vector4(viewportPos.x, viewportPos.y, 0, 0);
                if (viewportPos.x < -maxRadius || viewportPos.x > 2 * maxRadius || viewportPos.y < -maxRadius || viewportPos.y > 2 * maxRadius)
                {
                    inCamera[i] = false;
                }
                else
                {
                    inCamera[i] = true;
                }
            }
            else
            {
                centers[i] = Vector4.zero;
            }
        }
        for (int i = 0; i < MAX_CENTERS; i++)
        {
            displayRadii[i] = inCamera[i] ? radii[i] : 0f;
            //Debug.Log($"Target {i} radius: {displayRadii[i]}");
        }
        material.SetInt("_CenterCount", count);
        material.SetVectorArray("_Centers", centers);
        material.SetFloatArray("_Radii", displayRadii);
        material.SetVector("_CameraWorldPos", cam.transform.position);


    }

    public void ToggleMask(int index, float radius)
    {
        if (index < 0 || index >= MAX_CENTERS) return;
        Debug.Log($"Toggling mask for index {index} to radius {radius}");
        if (animCoroutines[index] != null)
            StopCoroutine(animCoroutines[index]);

        animCoroutines[index] = StartCoroutine(AnimateRadius(index, radius));
    }

    private IEnumerator AnimateRadius(int index, float target)
    {
        float start = radii[index];
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            radii[index] = Mathf.Lerp(start, target, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        radii[index] = target;
    }
}