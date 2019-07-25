using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using SplineMesh;

public class ItemPointer : MonoBehaviour, IDragHandler, IPointerEnterHandler, IPointerExitHandler,IPointerDownHandler
{
    public bool isSelect = false;
    Vector3 screenSpace;

    private void Start()
    {
        transform.GetComponent<Outline>().enabled = false;
    }
    public void OnDrag(PointerEventData eventData)
    {
        Vector3 currentPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z));
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
        try
        {
            screenSpace = Camera.main.WorldToScreenPoint(transform.position);
        }
        catch
        {
            return;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        foreach (var item in CameraTrailManager.instance.handlerGoup)
        {
            item.GetComponent<ItemPointer>().isSelect = false;
        }
        isSelect = true;
    }
}
