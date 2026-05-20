using UnityEngine;
using Unity.Collections;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class TileVisual : MonoBehaviour
{

    private bool initalize = false;

    [Header("Tile")]
    public Tile parentTile;
    private Transform tileTransform;
    //private Vector3 rotationDelta;
    //private int savedIndex;
    //Vector3 movementDelta;
    private Canvas canvas;

    [SerializeField] private Transform shakeParent;
    [SerializeField] private Transform tiltParent;

    [Header("Follow Parameters")]
    [SerializeField] private float followSpeed = 30;

    [Header("Scale Parameters")]
    [SerializeField] private float scaleHover = 1.1f;
    [SerializeField] private float scaleSelected = 1.15f;

    private float curveYOffset;

    void Start()
    {

    }

    public void Initialize(Tile target, int index = 0)
    {
        //Declarations
        parentTile = target;
        tileTransform = target.transform;
        canvas = GetComponent<Canvas>();
        //shadowCanvas = visualShadow.GetComponent<Canvas>();

        //Event Listening
        parentTile.PointerEnterEvent.AddListener(PointerEnter);
        parentTile.PointerExitEvent.AddListener(PointerExit);
        //parentTile.BeginDragEvent.AddListener(BeginDrag);
        //parentTile.EndDragEvent.AddListener(EndDrag);
        parentTile.PointerDownEvent.AddListener(PointerDown);
        parentTile.PointerUpEvent.AddListener(PointerUp);
        parentTile.SelectEvent.AddListener(Select);

        //Initialization
        initalize = true;
    }

    public void UpdateIndex(int length)
    {
        transform.SetSiblingIndex(parentTile.transform.parent.GetSiblingIndex());
    }

    // Update is called once per frame
    void Update()
    {
        if (!initalize || parentTile == null) return;

        SmoothFollow();
    }

    private void SmoothFollow()
    {
        Vector3 verticalOffset = (Vector3.up * (parentTile.isDragging ? 0 : curveYOffset));
        transform.position = Vector3.Lerp(transform.position, tileTransform.position + verticalOffset, followSpeed * Time.deltaTime);
    }

    private void Select(Tile tile, bool state)
    {
        if (state)
        {
            UpdateScale(scaleHover, true);
        }
        else
        {
            UpdateScale(scaleHover, false);
        }
    }

    private void PointerDown(Tile tile)
    {
        UpdateScale(scaleHover, false);
        UpdateScale(scaleSelected, true);
    }

    private void PointerUp(Tile tile, bool longPress)
    {
        UpdateScale(scaleSelected, false);
        UpdateScale(scaleHover, true);
    }

    private void PointerEnter(Tile tile)
    {
        UpdateScale(scaleHover, true);
    }

    private void PointerExit(Tile tile)
    {
        UpdateScale(scaleHover, false);
    }

    private void UpdateScale(float scale, bool scaleUp)
    {
        if (scaleUp)
        {
            transform.localScale = new Vector3(transform.localScale.x * scale, transform.localScale.y * scale, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x / scale, transform.localScale.y / scale, transform.localScale.z);
        }
    }
}
