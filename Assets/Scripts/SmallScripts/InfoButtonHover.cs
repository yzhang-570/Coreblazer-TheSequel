using UnityEngine;
using UnityEngine.EventSystems;

public class InfoButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GameObject infoPopup;
    public void OnPointerEnter(PointerEventData mouseData)
    {
        Debug.Log("mouse hover enter");
        infoPopup.SetActive(true);
    }

    public void OnPointerExit(PointerEventData mouseData)
    {
        Debug.Log("mouse hover exit");
        infoPopup.SetActive(false);
    }
}
