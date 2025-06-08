using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Yarn.Unity;

public class UIInputHandler : MonoBehaviour
{
    [SerializeField] DialogueRunner dialogueRunner;
    [SerializeField] DialogueManager dialogueManagerScript;
    [SerializeField] GameObject canvas;
    [SerializeField] GameObject memoryMenu;
    [SerializeField] GameObject incompatibleMessage;
    [SerializeField] GameObject secondMemoryMessage;
    [SerializeField] MemoryDisplayManager memoryDisplayManager;
    [SerializeField] GameObject memoryGainedUI;
    [SerializeField] GameObject newSpawnPointUI;
    [SerializeField] GameObject dialogueUI;
    [SerializeField] GameObject blockPuzzle;
    [SerializeField] GameObject controlsUI;
    [SerializeField] GameObject memoriesToggleUI;
    [SerializeField] GameObject questProgressUI;
    GraphicRaycaster UI_raycaster;

    PointerEventData click_data;
    List<RaycastResult> click_results;

    private float memoryTime = 1f;
    private float timer = 0f;
    bool disableMemoryMenuToggle = false;

    bool blockPuzzleFinished = false;

    void Awake()
    {
        click_data = new PointerEventData(EventSystem.current);
        UI_raycaster = canvas.GetComponent<GraphicRaycaster>();

        click_results = new List<RaycastResult>();
    }

    private void Start()
    {
        dialogueRunner.AddCommandHandler<string, string, string>("prompt_memory_selection", PromptMemorySelection);
        dialogueRunner.AddCommandHandler("disable_UI", disableUI);
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            click_results.Clear();
            click_data.position = Mouse.current.position.ReadValue();

            UI_raycaster.Raycast(click_data, click_results);

            foreach (RaycastResult result in click_results)
            {
                GameObject UI_element = result.gameObject;
                //Debug.Log(UI_element.name);

                // toggle menu
                if (UI_element.name == "MemoryMenuButton" && !disableMemoryMenuToggle)
                {
                    memoryMenu.SetActive(!memoryMenu.activeSelf);
                }
                if (UI_element.name == "MemoryGainedBackground" && timer <= 0)
                {
                    memoryGainedUI.SetActive(false);
                    dialogueUI.SetActive(true);
                }
            }
        }
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
    }

    public void ShowMemoryGainedUI()
    {
        timer = memoryTime;
        memoryGainedUI.SetActive(true);
        dialogueUI.SetActive(false);
        AudioManager.Instance.PlaySFX("echoding");
    }

    public void ShowNewSpawnPointUI()
    {
        //Debug.Log("new spawn ui shown");
        StartCoroutine(ActivateMessage(newSpawnPointUI));
    }

    public IEnumerator PromptMemorySelection(string npcName, string targetMemoryType1, string targetMemoryType2 = "None")
    {
        memoryMenu.SetActive(true);
        dialogueManagerScript.savedNPCs.Add(npcName);

        disableMemoryMenuToggle = true;
        blockPuzzleFinished = false;

        List<string> TypesToSelect = new List<string>();
        bool selectMultipleMemories = false; //used to determine if popup to select a memory should be shown

        TypesToSelect.Add(targetMemoryType1);
        if (targetMemoryType2 != "None")
        {
            TypesToSelect.Add(targetMemoryType2);
            selectMultipleMemories = true;
        }

        //Repeat until correct UI element is clicked
        while (true)
        {
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                click_results.Clear();
                click_data.position = Mouse.current.position.ReadValue();

                UI_raycaster.Raycast(click_data, click_results);

                foreach (RaycastResult result in click_results)
                {
                    GameObject UI_element = result.gameObject;

                    if (MemoryData.IsValidMemory(UI_element.name))
                    {
                        //Debug.Log($"\n MemoryType: {MemoryData.GetMemoryType(UI_element.name)}");

                        string memoryType = MemoryData.GetMemoryType(UI_element.name);

                        //Debug.Log($"{UI_element.name}'s type: {memoryType}");
                        //Debug.Log("Wanted types");
                        //foreach (string type in TypesToSelect)
                        //{
                        //    Debug.Log(type);
                        //}
                        if (TypesToSelect.Contains(memoryType))
                        {
                            AudioManager.Instance.PlaySFX("memorygained");
                            TypesToSelect.Remove(memoryType);
                            memoryDisplayManager.GiveMemory(npcName, UI_element.name);

                            //success message after selecting 1
                            if (selectMultipleMemories = true && TypesToSelect.Count == 1) StartCoroutine(ActivateMessage(secondMemoryMessage));
                            else if (TypesToSelect.Count == 0)
                            {
                                memoryMenu.SetActive(false);

                                blockPuzzle.SetActive(true);
                                Debug.Log("Start block puzzle for " + npcName);
                                blockPuzzle.GetComponent<BlockPuzzleManager>().loadLevel(npcName);
                            }
                        }
                        else //otherwise, show this memory is not compatible message
                        {
                            AudioManager.Instance.PlaySFX("negative");
                            StartCoroutine(ActivateMessage(incompatibleMessage));
                        }
                    }
                }
            }
            if (blockPuzzleFinished)
            {
                disableMemoryMenuToggle = false;
                dialogueManagerScript.SetQuestComplete(npcName); //mark completed
                yield break;
            }
            yield return null; //don't return until next frame
        }
    }

    public void EndBlockPuzzle()
    {
        blockPuzzle.SetActive(false);
        blockPuzzleFinished = true;
    }

    public IEnumerator ActivateMessage(GameObject messageWindow)
    {
        messageWindow.SetActive(true);
        //Debug.Log(messageWindow.name + " shown");
        yield return new WaitForSeconds(2f);
        messageWindow.SetActive(false);
        //Debug.Log(messageWindow.name + " hidden");
    }

    public void disableUI()
    {
        controlsUI.SetActive(false);
        memoriesToggleUI.SetActive(false);
        questProgressUI.SetActive(false);
    }

    public void enableUI()
    {
        controlsUI.SetActive(true);
        memoriesToggleUI.SetActive(true);
        questProgressUI.SetActive(true);
    }
















    //public void ShowMemoryGained(string memoryName)
    //{
    //    Debug.Log("show memory gained called");
    //    memoryGainedUI.SetActive(true);

    //    StartCoroutine(waitForClick("MemoryGainedBackground"));
    //}

    //public IEnumerator waitForClick(string UIName)
    //{
    //    while (true)
    //    {
    //        if (Mouse.current.leftButton.wasReleasedThisFrame)
    //        {
    //            click_results.Clear();
    //            click_data.position = Mouse.current.position.ReadValue();

    //            UI_raycaster.Raycast(click_data, click_results);

    //            foreach (RaycastResult result in click_results)
    //            {
    //                GameObject UI_element = result.gameObject;
    //                if (UI_element.name == UIName)
    //                {
    //                    memoryGainedUI.SetActive(false);
    //                    yield break;
    //                }
    //            }
    //        }
    //        yield return null;
    //    }
    //}
}


