using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase groundTile;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap grassTilemap;
    [SerializeField] private float scale = 0.2f;
    [SerializeField] private float threshold = 0.6f;
    [SerializeField] private GameObject TreePrefab;
    [SerializeField] private float treeScale = 0.05f;
    [SerializeField] private float treethreshold = 0.1f;
    [SerializeField] private GameObject BushPrefab;
    [SerializeField] private int bushesPerTile = 3;
    [SerializeField] private float bushScale = 0.15f;
    [SerializeField] private float bushthreshold = 1f;

    private HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();

    void Awake()
    {
        Generate(128, 128);
    }

    public void Generate(int width, int height)
    {
        float offsetX = Random.Range(0f, 9999f);
        float offsetY = Random.Range(0f, 9999f);
        float offsetX2 = Random.Range(0f, 9999f);
        float offsetY2 = Random.Range(0f, 9999f);
        float offsetX3 = Random.Range(0f, 9999f);
        float offsetY3 = Random.Range(0f, 9999f);

        int halfWidth = width / 2;
        int halfHeight = height / 2;

        occupiedPositions.Clear();

        for (int x = -(halfWidth + 2); x < halfWidth + 2; x++)
        {
            for (int y = -(halfHeight + 2); y < halfHeight + 2; y++)
            {
                Vector3Int pos = new(x, y, 0);
                groundTilemap.SetTile(pos, groundTile);

                bool inMainArea = x >= -halfWidth && x < halfWidth && y >= -halfHeight && y < halfHeight;

                if (inMainArea)
                {
                    float noise = Mathf.PerlinNoise(
                        (x + halfWidth + offsetX) * scale,
                        (y + halfHeight + offsetY) * scale
                    );
                    if (noise > threshold)
                        grassTilemap.SetTile(new(x, y, 0), grassTile);
                }
                else
                {
                    grassTilemap.SetTile(new(x, y, 0), grassTile);
                }
            }
        }

        for (int x = -halfWidth; x < halfWidth; x++)
        {
            for (int y = -halfHeight; y < halfHeight; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (grassTilemap.GetTile(pos) != null)
                {
                    if (CountNeighbors(x, y, grassTilemap) < 4)
                        grassTilemap.SetTile(pos, null);
                }
            }
        }

        for (int x = -halfWidth; x < halfWidth; x++)
        {
            for (int y = -halfHeight; y < halfHeight; y++)
            {
                float noise = Mathf.PerlinNoise(
                    (x + halfWidth + offsetX2) * treeScale,
                    (y + halfHeight + offsetY2) * treeScale
                );

                Vector3 worldPos = grassTilemap.CellToWorld(new Vector3Int(x, y, 0));
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (grassTilemap.GetTile(pos) != null && noise > treethreshold)
                {
                    Instantiate(TreePrefab, worldPos, Quaternion.identity, transform);
                    occupiedPositions.Add(new Vector2Int(x, y));
                }
            }
        }

        for (int x = -halfWidth; x < halfWidth; x++)
        {
            for (int y = -halfHeight; y < halfHeight; y++)
            {
                float noise = Mathf.PerlinNoise(
                    (x + halfWidth + offsetX3) * bushScale,
                    (y + halfHeight + offsetY3) * bushScale
                );

                Vector3 worldPos = grassTilemap.CellToWorld(new Vector3Int(x, y, 0));
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (grassTilemap.GetTile(pos) != null
                    && !occupiedPositions.Contains(new Vector2Int(x, y))
                    && noise > bushthreshold)
                {
                    for (int i = 0; i < bushesPerTile; i++)
                    {
                        Vector3 spawnPos = worldPos + new Vector3(
                            Random.Range(-0.4f, 0.4f),
                            Random.Range(-0.4f, 0.4f),
                            0
                        );
                        Instantiate(BushPrefab, spawnPos, Quaternion.identity, transform);
                    }
                }
            }
        }
    }

    int CountNeighbors(int x, int y, Tilemap tilemap)
    {
        int count = 0;
        for (int nx = -1; nx <= 1; nx++)
        {
            for (int ny = -1; ny <= 1; ny++)
            {
                if (nx == 0 && ny == 0) continue;

                Vector3Int neighborPos = new Vector3Int(x + nx, y + ny, 0);
                if (tilemap.GetTile(neighborPos) != null)
                    count++;
            }
        }
        return count;
    }
}
