using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using Unity.VisualScripting;

public class Tile : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    private Canvas canvas;
    private Image imageComponent;
    [SerializeField] private bool instantiateVisual = true;
    private VisualTileHandler visualHandler;
    private Vector3 offset;

    [Header("Movement")]
    [SerializeField] private float moveSpeedLimit = 50;

    [Header("Selection")]
    public bool selected;
    public float selectionOffset = 50;
    private float pointerDownTime;
    private float pointerUpTime;

    [Header("Visual")]
    [SerializeField] private GameObject tileVisualPrefab;
    [HideInInspector] public TileVisual tileVisual;

    [Header("States")]
    public bool isHovering;
    public bool isDragging;
    [HideInInspector] public bool wasDragged;

    [Header("Events")]
    [HideInInspector] public UnityEvent<Tile> PointerEnterEvent;
    [HideInInspector] public UnityEvent<Tile> PointerExitEvent;
    [HideInInspector] public UnityEvent<Tile, bool> PointerUpEvent;
    [HideInInspector] public UnityEvent<Tile> PointerDownEvent;
    [HideInInspector] public UnityEvent<Tile> BeginDragEvent;
    [HideInInspector] public UnityEvent<Tile> EndDragEvent;
    [HideInInspector] public UnityEvent<Tile, bool> SelectEvent;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        imageComponent = GetComponent<Image>();

        if (!instantiateVisual)
            return;

        visualHandler = FindFirstObjectByType<VisualTileHandler>();
        tileVisual = Instantiate(tileVisualPrefab, visualHandler ? visualHandler.transform : canvas.transform).GetComponent<TileVisual>();
        tileVisual.Initialize(this);
    }

    // Update is called once per frame
    void Update()
    {

        if (isDragging)
        {
            SwapHolder();
            Vector2 targetPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - offset;
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            Vector2 velocity = direction * Mathf.Min(moveSpeedLimit, Vector2.Distance(transform.position, targetPosition) / Time.deltaTime);
            transform.Translate(velocity * Time.deltaTime);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        BeginDragEvent.Invoke(this);
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        offset = mousePosition - (Vector2)transform.position;
        isDragging = true;
        canvas.GetComponent<GraphicRaycaster>().enabled = false;
        imageComponent.raycastTarget = false;

        wasDragged = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EndDragEvent.Invoke(this);
        isDragging = false;
        canvas.GetComponent<GraphicRaycaster>().enabled = true;
        imageComponent.raycastTarget = true;
        
        StartCoroutine(FrameWait());
        
        IEnumerator FrameWait()
        {
            yield return new WaitForEndOfFrame();
            wasDragged = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnterEvent.Invoke(this);
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExitEvent.Invoke(this);
        isHovering = false;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        PointerDownEvent.Invoke(this);
        pointerDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        pointerUpTime = Time.time;

        PointerUpEvent.Invoke(this, pointerUpTime - pointerDownTime > .13f);

        if (pointerUpTime - pointerDownTime <= .13f)
        {
            selected = !selected;
            SelectEvent.Invoke(this, selected);
        }

        //if (wasDragged)
        //    return;


        transform.localPosition = Vector3.zero;
        if (selected)
            transform.localPosition += (this.transform.up * selectionOffset);
    }

    public int SiblingAmount()
    {
        return transform.parent.CompareTag("Slot") ? transform.parent.parent.childCount - 1 : 0;
    }

    public int ParentIndex()
    {
        return transform.parent.CompareTag("Slot") ? transform.parent.GetSiblingIndex() : 0;
    }

    public void SwapHolder()
    {
        if (!isDragging) { return; }
        var curHolder = GetComponentInParent<TileHolder>();
        var holders = FindObjectsByType<TileHolder>(FindObjectsSortMode.None);
        foreach (var holder in holders)
        {
            if (IsOverlapping(this, holder))
            {
                curHolder.tiles.Remove(this);
                transform.parent.SetParent(holder.transform, true);
                holder.tiles.Add(this);
                break;
            }
        }

    }

    public bool IsOverlapping(Tile tile, TileHolder holder)
    {
        var tileRect = tile.GetComponent<RectTransform>();
        var holderRect = holder.GetComponent<RectTransform>();
        Vector3 worldCenter = tileRect.TransformPoint(tileRect.rect.center);
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldCenter);

        return RectTransformUtility.RectangleContainsScreenPoint(holderRect, screenPoint, null);
    }
}
