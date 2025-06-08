using UnityEngine;
using Yarn.Unity;

public class TriggerFinalCutscene : MonoBehaviour
{
    [SerializeField] DialogueManager dialogueManager;
    [SerializeField] DialogueRunner dialogueRunner;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("entered trigger");
        if(other.gameObject.name == "Player")
        {
            if (dialogueManager.savedNPCs.Count < 5)
            {
                Debug.Log("running bad end");
                dialogueRunner.StartDialogue("Bad");
            }
            else
            {
                Debug.Log("running good end");
                dialogueRunner.StartDialogue("Good");
            }
            gameObject.SetActive(false);
        }
    }
}
