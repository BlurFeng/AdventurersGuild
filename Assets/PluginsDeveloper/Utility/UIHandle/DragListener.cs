using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragListener : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    /// <summary>
    /// <pointerEnter, position>
    /// </summary>
    Action<GameObject, Vector2> m_OnBeginDrag;
    /// <summary>
    /// <pointerEnter, position, delta>
    /// </summary>
    Action<GameObject, Vector2, Vector2> m_OnDrag;
    /// <summary>
    /// <pointerEnter, position>
    /// </summary>
    Action<GameObject, Vector2> m_OnEndDrag;

    public void SetBeginDragHandler(Action<GameObject, Vector2> handler)
    {
        m_OnBeginDrag = handler;
    }

    public void SetDragHandler(Action<GameObject, Vector2, Vector2> handler)
    {
        m_OnDrag = handler;
    }

    public void SetEndDragHandler(Action<GameObject, Vector2> handler)
    {
        m_OnEndDrag = handler;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log(string.Format("OnBeginDrag: pointerEnter={0} ",
        //    eventData.pointerEnter));
        m_OnBeginDrag?.Invoke(eventData.pointerEnter, eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log(string.Format("OnDrag: pointerEnter={0}, position={1}, delta={2}",
        //    eventData.pointerEnter, eventData.position, eventData.delta));
        m_OnDrag?.Invoke(eventData.pointerEnter, eventData.position, eventData.delta);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log(string.Format("OnEndDrag: pointerEnter={0}, ",
        //    eventData.pointerEnter));
        m_OnEndDrag?.Invoke(eventData.pointerEnter, eventData.position);
    }

    //public override void OnPointerDown(PointerEventData eventData)
    //{
    //    Debug.Log(string.Format("OnPointerDown: pointerPress={0}, pointerDrag={1}, pointerEnter={2}, delta={3}",
    //        eventData.pointerPress, eventData.pointerDrag, eventData.pointerEnter, eventData.delta));
    //}

}
