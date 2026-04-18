using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class TilePrefabPair
{
	public TileBase tile;
	public GameObject prefab;
}

public class TileMapToGameObjects : MonoBehaviour
{
	public Tilemap tilemap;

	[Header("Tile → Prefab Mapping")]
	public List<TilePrefabPair> tileMappings = new List<TilePrefabPair>();

	[Header("Options")]
	public bool clearTiles = true;
	public Transform parentContainer;

	private Dictionary<TileBase, GameObject> lookup;

	void Awake()
	{
		lookup = new Dictionary<TileBase, GameObject>();
	}

	[ContextMenu("Convert Tilemap To GameObjects")]
	public void Convert()
	{

        lookup = new Dictionary<TileBase, GameObject>();

        foreach (var pair in tileMappings)
        {
            if (pair.tile != null && pair.prefab != null)
            {
                Debug.Log(pair.tile);
                Debug.Log(pair.prefab);

				if (!lookup.ContainsKey(pair.tile))
					lookup.Add(pair.tile, pair.prefab);
            }
        }
        
		if (tilemap == null)
		{
			Debug.LogError("Tilemap not assigned!");
			return;
		}

		if (lookup == null || lookup.Count == 0)
		{
			Debug.LogError("No tile mappings set!");
			return;
		}

		BoundsInt bounds = tilemap.cellBounds;

		int count = 0;

		foreach (Vector3Int pos in bounds.allPositionsWithin)
		{
			TileBase tile = tilemap.GetTile(pos);

			if (tile == null) continue;

			if (!lookup.ContainsKey(tile)) continue;

			GameObject prefab = lookup[tile];

			Vector3 worldPos = tilemap.GetCellCenterWorld(pos);

			GameObject obj = Instantiate(prefab, worldPos, Quaternion.identity,
				parentContainer != null ? parentContainer : transform);

			obj.name = prefab.name + $"_{pos.x}_{pos.y}";

			count++;

			if (clearTiles)
			{
				tilemap.SetTile(pos, null);
			}
		}

		Debug.Log($"Converted {count} tiles to GameObjects!");
	}
}
