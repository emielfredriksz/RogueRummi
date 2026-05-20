using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

public class TileHolder : MonoBehaviour
{

    [SerializeField] private Tile selectedTile;

    [SerializeField] private GameObject slotPrefab;

    [Header("Spawn settings")]
    [SerializeField] private int tilesToSpawn = 7;
    public List<Tile> tiles;

    void Start()
    {

        for (int i = 0; i < tilesToSpawn; i++)
        {
            Instantiate(slotPrefab, transform);
        }

        tiles = GetComponentsInChildren<Tile>().ToList();

        int tileCount = 0;
        foreach (Tile tile in tiles)
        {
            tile.name = tileCount.ToString();
            tile.BeginDragEvent.AddListener(BeginDrag);
            tile.EndDragEvent.AddListener(EndDrag);
            tileCount++;
        }
    }

    private void BeginDrag(Tile tile)
    {
        selectedTile = tile;
    }


    void EndDrag(Tile tile)
    {
        if (selectedTile == null)
            return;

        selectedTile = null;

    }

    void Update()
    {

        if (selectedTile == null)
            return;

        for (int i = 0; i < tiles.Count; i++)
        {
            if (selectedTile.transform.position.x > tiles[i].transform.position.x)
            {
                if (selectedTile.ParentIndex() < tiles[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }

            if (selectedTile.transform.position.x < tiles[i].transform.position.x)
            {
                if (selectedTile.ParentIndex() > tiles[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }
        }
    }

    void Swap(int index)
    {

        Transform focusedParent = selectedTile.transform.parent;
        Transform crossedParent = tiles[index].transform.parent;

        tiles[index].transform.SetParent(focusedParent);
        tiles[index].transform.localPosition = tiles[index].selected ? new Vector3(0, tiles[index].selectionOffset, 0) : Vector3.zero;
        selectedTile.transform.SetParent(crossedParent);

        //bool swapIsRight = tiles[index].ParentIndex() > selectedTile.ParentIndex();
        //tiles[index].tileVisual.Swap(swapIsRight ? -1 : 1);

        //Updated Visual Indexes
        foreach (Tile tile in tiles)
        {
            tile.tileVisual.UpdateIndex(transform.childCount);
        }
    }


    //  TODO Emiel: when swapping holders cards get swapped too fast or something so they get swapped to the other holder or something oops
}
