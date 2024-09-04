using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickListener : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public static ClickListener Get(GameObject obj)
    {
        ClickListener listener = obj.GetComponent<ClickListener>();
        if (listener == null)
        {
            listener = obj.AddComponent<ClickListener>();
        }
        return listener;
    }

    public static void Clear(GameObject obj)
    {
        ClickListener listener = obj.GetComponent<ClickListener>();
        if (listener != null)
        {
            Destroy(listener);
        }
    }

    Action<PointerEventData> m_ClickHandler = null;
    Action<PointerEventData> m_PointerUpHandler = null;
    Action<PointerEventData> m_PointerDownHandler = null;
    Action<PointerEventData> m_PointerEnterHandler = null;
    Action<PointerEventData> m_PointerExitHandler = null;
    Action<PointerEventData> m_DragHandler = null;

    private bool isPointerDown;
    private float timeDragStarted;
    public float durationThreshold = 1.0f;
    public float timeScale = 0f; //需要越按越快使用 
    private bool m_DragTriggered = false;
    private Vector2 timeDragStartedPos;
    private string mClickSound;
    public bool enableClickSound = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        m_ClickHandler?.Invoke(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        m_DragTriggered = false;
        timeDragStarted = Time.time;

        timeDragStartedPos = eventData.position;
        if (m_PointerDownHandler != null)
        {
            float timeDrag = Time.time - timeDragStarted;
            float magnitude = (timeDragStartedPos - eventData.position).magnitude;
            m_PointerDownHandler.Invoke(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
        m_DragTriggered = true;
        if (m_PointerUpHandler != null)
        {
            float timeDrag = Time.time - timeDragStarted;
            float magnitude = (timeDragStartedPos - eventData.position).magnitude;
            m_PointerUpHandler.Invoke(eventData);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_PointerEnterHandler?.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_PointerExitHandler?.Invoke(eventData);

        isPointerDown = false;
    }

    /// <summary>
    /// 设置事件 鼠标点击
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="clickSound"></param>
    public void SetClickHandler(Action<PointerEventData> handler, string clickSound = "")
    {
        m_ClickHandler = handler;
        mClickSound = clickSound;
    }

    /// <summary>
    /// 设置事件 鼠标长按
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="clickSound"></param>
    public void SetDragHandler(Action<PointerEventData> handler, string clickSound = "")
    {
        m_DragHandler = handler;
        mClickSound = clickSound;
    }

    /// <summary>
    /// 设置事件 鼠标按下
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="clickSound"></param>
    public void SetPointerDownHandler(Action<PointerEventData> handler, string clickSound = "")
    {
        m_PointerDownHandler = handler;
        mClickSound = clickSound;
    }

    /// <summary>
    /// 设置事件 鼠标抬起
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="clickSound"></param>
    public void SetPointerUpHandler(Action<PointerEventData> handler, string clickSound = "")
    {
        m_PointerUpHandler = handler;
        mClickSound = clickSound;
    }

    /// <summary>
    /// 设置事件 鼠标进入
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="clickSound"></param>
    public void SetPointerEnterHandler(Action<PointerEventData> handler, string clickSound = "")
    {
        m_PointerEnterHandler = handler;
        mClickSound = clickSound;
    }

    /// <summary>
    /// 设置事件 鼠标离开
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="clickSound"></param>
    public void SetPointerExitHandler(Action<PointerEventData> handler, string clickSound = "")
    {
        m_PointerExitHandler = handler;
        mClickSound = clickSound;
    }

    private void Update()
    {
        //鼠标长按 事件
        if (m_DragHandler != null && isPointerDown && !m_DragTriggered)
        {
            m_DragHandler?.Invoke(null);

            //if (Time.time - timeDragStarted > durationThreshold)
            //{
            //    durationThreshold -= timeScale;
            //    if (durationThreshold <= 0.08f)
            //    {
            //        durationThreshold = 0.08f;
            //    }
            //    timeDragStarted = Time.time;
            //    m_DragHandler?.Invoke(gameObject);
            //}
            return;
        }
        durationThreshold = 1.0f;
    }

    public static void PassEvent<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function, bool alwaysPass = false)
    where T : IEventSystemHandler
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);
        GameObject current = data.pointerCurrentRaycast.gameObject;
        for (int i = 0; i < results.Count; ++i)
        {
            if (current != results[i].gameObject)
            {
                ExecuteEvents.Execute(results[i].gameObject, data, function);
                // RaycastAll后ugui会自己排序，如果你只想响应透下去的最近的一个响应，
                // 这里ExecuteEvents.Execute后直接break就行。
                if (alwaysPass == false)
                {
                    break;
                }
            }
        }
    }
}
