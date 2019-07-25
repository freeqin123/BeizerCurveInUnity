using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TextMove : MonoBehaviour,IDragHandler,IPointerEnterHandler,IPointerExitHandler
{
    private Vector3 screenSpace;
    private Vector3 offset;

    private void Start()
    {
        transform.GetComponent<Outline>().enabled = false;
    }
    public void OnDrag(PointerEventData eventData)
    {
        Vector3 currentPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,screenSpace.z));
        transform.position = currentPos;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        transform.GetComponent<Outline>().enabled = true;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        transform.GetComponent<Outline>().enabled = false;
    }

    private void Update()
    {
        screenSpace = Camera.main.WorldToScreenPoint(transform.position);
    }

}
