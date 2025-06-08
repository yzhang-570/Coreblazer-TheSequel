using NUnit.Framework;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] DialogueRunner dialogueRunner;
    [SerializeField] Camera mainCamera;
    [SerializeField] PlayerMovement playerController;
    //[SerializeField] Movement2 playerController;
    [SerializeField] IslandManager islandManagerScript;
    [SerializeField] ColorMaskController colorMaskController;
    PlayerInputActions playerInputActions;

    //wau using hashset smart, anne very big brain
    //public HashSet<string> completedQuests = new HashSet<string>();
    public HashSet<string> completedQuests = new HashSet<string>() { "Sapling" };

    public HashSet<string> killedNPCs = new HashSet<string>();
    public HashSet<string> savedNPCs = new HashSet<string>() { "a", "b", "c", "d", "r" };
    //public HashSet<string> savedNPCs = new HashSet<string>() {"Robot", "Sapling"};
    //public HashSet<string> savedNPCs = new HashSet<string>();

    // for completed interactions with environment objects
    HashSet<string> completedInteractions = new HashSet<string>();

    // private NPCFadeAndDisappear npcFadeScript;
    private NPCParticles npcFadeScript;

    void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.StartDialogue.Enable();
    }

    void Start()
    {
        dialogueRunner.AddCommandHandler<string>("start_fade", FadeNPC);
        dialogueRunner.AddCommandHandler<int>("remove_color", RemoveColor);
        dialogueRunner.AddCommandHandler<int>("show_color", ShowColor);
        dialogueRunner.AddCommandHandler<int>("half_show_color", HalfShowColor);
    }

    public void OnDisable()
    {
        playerInputActions.Player.StartDialogue.Disable();
    }

    void OnStartDialogue()
    {
        if (!dialogueRunner.IsDialogueRunning)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit objectHit;

            int ignoreLayerNumber = LayerMask.NameToLayer("IgnoreForDialogue");
            int ignoreLayerMask = ~(1 << ignoreLayerNumber);

            if (Physics.Raycast(ray, out objectHit, Mathf.Infinity, ignoreLayerMask))
            {
                string npcName = objectHit.collider.gameObject.name;
                npcName = RemoveWhitespace(npcName);
                if (objectHit.collider.gameObject.CompareTag("hasDialogue") && !completedQuests.Contains(npcName))
                {
                    //special case
                    if (objectHit.collider.gameObject.name == "LittleGirl")
                    {
                        if(completedQuests.Contains("BabyBear") && completedQuests.Contains("Adventurer"))
                        {
                            playerController.faceNPC(objectHit.collider.gameObject.transform);
                            npcFadeScript = objectHit.collider.gameObject.GetComponent<NPCParticles>();
                            dialogueRunner.StartDialogue($"{npcName}");
                        }
                    }
                    else //regular - always triggers dialogue
                    {
                        playerController.faceNPC(objectHit.collider.gameObject.transform);
                        npcFadeScript = objectHit.collider.gameObject.GetComponent<NPCParticles>();
                        dialogueRunner.StartDialogue($"{npcName}");
                    }
                }
                else if (objectHit.collider.gameObject.CompareTag("givesMemory"))
                {
                    string objName = objectHit.collider.gameObject.name;
                    if (!completedInteractions.Contains(objName))
                    {
                        completedInteractions.Add(objName);
                        objectHit.collider.gameObject.transform.parent.GetComponent<IndicatorShow>().MemoryUnavailable();
                        dialogueRunner.StartDialogue($"{objName}");
                    }
                }
            }
        }
    }

    public void RemoveColor(int index)
    {   
        colorMaskController.ToggleMask(index, 0f);
    }
    public void ShowColor(int index)
    {
        colorMaskController.ToggleMask(index, 1f);
    }
    public void HalfShowColor(int index)
    {
        colorMaskController.ToggleMask(index, .3f);
    }
    public void SetQuestComplete(string npcName)
    {
        npcName = RemoveWhitespace(npcName);
        completedQuests.Add(npcName);
        //Debug.Log($"{npcName} quest marked complete");

        islandManagerScript.CheckCurrentIslandStatus();
    }

    private static readonly Regex sWhitespace = new Regex(@"\s+");
    public static string RemoveWhitespace(string input)
    {
        return sWhitespace.Replace(input, "");
    }

    public void FadeNPC(string npc)
    {
        npc = RemoveWhitespace(npc);
        killedNPCs.Add(npc);

        //foreach(string npcName in killedNPCs)
        //{
        //    Debug.Log("killed " + npcName);
        //}
        //foreach (string npcName in savedNPCs)
        //{
        //    Debug.Log("saved " + npcName);
        //}

        GameObject obj = GameObject.Find(npc);
        if (obj != null)
        {
            obj.GetComponent<NPCParticles>().StartFade(npc);
        }
    }
}