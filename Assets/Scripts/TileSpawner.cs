using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TileSpawner : MonoBehaviour, IPointerDownHandler
{

    [SerializeField] private Tile tile;

    void Start()
    {
        
    }


    void Update()
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (tile != null)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Tile newTile = Instantiate(tile, mousePosition, Quaternion.identity);
        }
    }
}
