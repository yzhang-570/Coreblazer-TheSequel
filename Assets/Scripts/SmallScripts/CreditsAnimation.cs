using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Yarn.Unity;

public class CreditsAnimation : MonoBehaviour
{
    [SerializeField] GameObject creditUI;
    [SerializeField] DialogueRunner dialogueRunner;
    private RectTransform canvasTransform;
    private float animationDuration;

    /*
     * TO USE:
     * call rollCredits() to play the animation
     */

    void Start()
    {
        creditUI.SetActive(false);
        canvasTransform = creditUI.GetComponent<RectTransform>();
        animationDuration = 15f;

        dialogueRunner.AddCommandHandler("roll_credits", rollCredits);
        //rollCredits();
    }

    public void rollCredits()
    {
        creditUI.SetActive(true);
        StartCoroutine(playCreditsAnimation());
    }

    private IEnumerator playCreditsAnimation()
    {
        float startTime = Time.time;
        Vector2 startPosition = new Vector2(0, -1200);
        Vector2 endPosition = new Vector2(0, 2000);

        while(Time.time - startTime < animationDuration)
        {
            canvasTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, (Time.time - startTime)/animationDuration);
            yield return null;
        }
        yield break;
    }
}
