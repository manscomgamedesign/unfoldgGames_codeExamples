using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragableItemUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private Transform parentBeforeDrag;
    [SerializeField] private Transform parentAfterDrag;

    /// <summary>
    /// represent the index of this drag item from the parent
    /// </summary>
    private int dragIndex;

    public Action OnDragBeginAction;
    public Action OnDragEndAction;

    public Transform ParentBeforeDrag { get { return parentBeforeDrag; } set { parentBeforeDrag = value; } }
    public Transform ParentAfterDrag { get { return parentAfterDrag; } set { parentAfterDrag = value; } }
    public int DragIndex { get { return dragIndex; } }

    /// <summary>
    /// if the item has direct drop target then just drop the item to this target without other actions
    /// thats beacuse in the game design item can also be directly drop to an area
    /// </summary>
    public bool directDropTarget = false; // deafult is true until the the draggable item is drag from the non-restricted area

    private void Awake()
    {
        if (canvas == null) canvas = transform.root.GetComponent<Canvas>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        rectTransform = GetComponent<RectTransform>();
        if (GetComponent<Image>() == null)
        {
            Image image = gameObject.AddComponent<Image>();
            image.raycastTarget = true;
            image.color = new Color(0, 0, 0, 0);
        }
        else
        {
            Image image = gameObject.GetComponent<Image>();
            image.raycastTarget = true;
            image.color = new Color(0, 0, 0, 0);
        }
    }

    private void OnEnable()
    {
        canvasGroup.blocksRaycasts = true; // make sure can be drag whenever this component turn on
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;

        // depend on the UI strcture need to call the parent
        dragIndex = transform.parent.GetSiblingIndex();

        parentBeforeDrag = transform.parent;
        parentAfterDrag = transform.parent; // item will be drop back to the origin if found 
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        if (OnDragBeginAction != null)
            OnDragBeginAction.Invoke();
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform,
            eventData.position,
            canvas.worldCamera,
            out position);
        transform.position = canvas.transform.TransformPoint(position);

        //rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // Check if can drop
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        //DropableMargeMercenaryUI canDrop = null;
        //foreach (var result in results)
        //{
        //    canDrop = result.gameObject.GetComponent<DropableMargeMercenaryUI>();

        //    if (canDrop)
        //    {
        //        transform.SetParent(parentAfterDrag);
        //        // set the new parent 
        //        return;
        //    }
        //}

        //if (canDrop == null)
        //{
            transform.position = parentAfterDrag.position;
            transform.SetParent(parentAfterDrag);
        //}

        if (OnDragEndAction != null)
            OnDragEndAction.Invoke();     
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {

    }

    public bool RectOverlaps(RectTransform rect1, RectTransform rect2)
    {
        return WorldRect(rect1).Overlaps(WorldRect(rect2), true);
    }

    private Rect WorldRect(RectTransform rt)
    {
        Vector2 sizeDelta = rt.sizeDelta;
        float rectTransformWidth = sizeDelta.x * rectTransform.lossyScale.x;
        float rectTransformHeight = sizeDelta.y * rectTransform.lossyScale.y;
        Vector3 position = rt.position;
        return new Rect(position.x - rectTransformWidth / 2f, position.y - rectTransformHeight / 2, rectTransformWidth, rectTransformHeight);
    }
}
