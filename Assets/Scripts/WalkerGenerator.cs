using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WalkerGenerator : MonoBehaviour
{
    [Header("Prefabs & Tiles")]
    public GameObject dashablePrefab;
    public float dashableSpawnChance = 0.05f;
    public RuleTile Wall;
    public TileBase Background;

    [Header("Tilemaps")]
    public Tilemap tilemapWalls;
    public Tilemap tilemapBackground;

    [Header("Map Settings")]
    public int MapWidth = 10;
    public int MapHeight = 60;
    public int MaximumWalkers = 3;
    public float FillPercentage = 0.20f;

    [Header("Infinite Generation")]
    public Transform playerTransform;
    public float GenerateAheadThreshold = 0.7f;

    private int nextChunkYOffset = 0;
    private List<int> lastChunkTopOpenings = new List<int>();

    public enum Grid { EMPTY, WALL }

    void Start()
    {
        GenerateChunk(0);
        GenerateChunk(MapHeight);
        nextChunkYOffset = MapHeight * 2;

        playerTransform.position = new Vector3(MapWidth / 2f + 0.5f, 2.5f, 0);
        Camera.main.transform.position = new Vector3(
            MapWidth / 2f, 2f,
            Camera.main.transform.position.z);
    }

    void Update()
    {
        float generateTriggerY = nextChunkYOffset - MapHeight * (1f - GenerateAheadThreshold);

        if (playerTransform.position.y > generateTriggerY)
        {
            ClearChunkBelow(nextChunkYOffset - MapHeight * 2);
            GenerateChunk(nextChunkYOffset);
            nextChunkYOffset += MapHeight;
        }
    }

    // ─── Génération d'un chunk ────────────────────────────────────────────────

    void GenerateChunk(int yOffset)
    {
        Grid[,] grid = new Grid[MapWidth, MapHeight];

        for (int x = 0; x < MapWidth; x++)
            for (int y = 0; y < MapHeight; y++)
                grid[x, y] = Grid.WALL;

        var walkers = new List<WalkerObject>();

        if (lastChunkTopOpenings.Count > 0)
        {
            foreach (int openX in lastChunkTopOpenings)
            {
                grid[openX, 0] = Grid.EMPTY;
                grid[openX, 1] = Grid.EMPTY;
                grid[openX, 2] = Grid.EMPTY;
                walkers.Add(new WalkerObject(
                    new Vector2(openX, 2), GetDirection(), 0.5f));
            }
        }
        else
        {
            Vector2 startPos = new Vector2(MapWidth / 2, 2);
            grid[(int)startPos.x, (int)startPos.y] = Grid.EMPTY;
            walkers.Add(new WalkerObject(startPos, GetDirection(), 0.5f));
        }

        int tileCount = CountEmpty(grid);
        CreatePaths(grid, walkers, tileCount);

        // Force la jonction après génération
        if (lastChunkTopOpenings.Count > 0)
        {
            foreach (int openX in lastChunkTopOpenings)
            {
                grid[openX, 0] = Grid.EMPTY;
                grid[openX, 1] = Grid.EMPTY;
                grid[openX, 2] = Grid.EMPTY;

                // Perce la dernière ligne du chunk précédent sur la Tilemap
                int prevChunkTopY = yOffset - 1;
                if (prevChunkTopY >= 0)
                {
                    tilemapWalls.SetTile(new Vector3Int(openX, prevChunkTopY, 0), null);

                    if (tilemapBackground != null)
                        tilemapBackground.SetTile(new Vector3Int(openX, prevChunkTopY, 0), Background);
                }
            }
        }

        // Mémorise les ouvertures en haut pour le prochain chunk
        lastChunkTopOpenings.Clear();
        for (int x = 0; x < MapWidth; x++)
            if (grid[x, MapHeight - 1] == Grid.EMPTY || grid[x, MapHeight - 2] == Grid.EMPTY)
                lastChunkTopOpenings.Add(x);

        // Garantit au moins une ouverture en haut
        if (lastChunkTopOpenings.Count == 0)
        {
            int centerX = MapWidth / 2;
            grid[centerX, MapHeight - 1] = Grid.EMPTY;
            grid[centerX, MapHeight - 2] = Grid.EMPTY;
            grid[centerX, MapHeight - 3] = Grid.EMPTY;
            lastChunkTopOpenings.Add(centerX);
        }

        RenderChunk(grid, yOffset);
        SpawnDashables(grid, yOffset);
    }

    int CountEmpty(Grid[,] grid)
    {
        int count = 0;
        for (int x = 0; x < MapWidth; x++)
            for (int y = 0; y < MapHeight; y++)
                if (grid[x, y] == Grid.EMPTY) count++;
        return count;
    }

    void CreatePaths(Grid[,] grid, List<WalkerObject> walkers, int tileCount)
    {
        int maxIterations = MapWidth * MapHeight * 10;
        int iterations = 0;
        int totalCells = grid.Length;

        while ((float)tileCount / totalCells < FillPercentage)
        {
            if (++iterations > maxIterations)
            {
                Debug.LogWarning("Génération stoppée : trop d'itérations");
                break;
            }

            foreach (var w in walkers)
            {
                int x = (int)w.Position.x;
                int y = (int)w.Position.y;
                if (grid[x, y] != Grid.EMPTY)
                {
                    grid[x, y] = Grid.EMPTY;
                    tileCount++;
                }
            }

            ChanceToRemove(walkers);
            ChanceToRedirect(walkers);
            ChanceToCreate(walkers);
            UpdatePosition(walkers, grid);
        }
    }

    // ─── Rendu ───────────────────────────────────────────────────────────────

    void RenderChunk(Grid[,] grid, int yOffset)
    {
        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                Vector3Int pos = new Vector3Int(x, y + yOffset, 0);

                if (grid[x, y] == Grid.WALL)
                {
                    tilemapWalls.SetTile(pos, Wall);
                }
                else if (Background != null && tilemapBackground != null)
                {
                    tilemapBackground.SetTile(pos, Background);
                }
            }
        }
        Physics2D.SyncTransforms();
    }

    void ClearChunkBelow(int belowY)
    {
        if (belowY <= 0) return;

        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < belowY; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                tilemapWalls.SetTile(pos, null);
                if (tilemapBackground != null)
                    tilemapBackground.SetTile(pos, null);
            }
        }
    }

    // ─── Walker helpers ───────────────────────────────────────────────────────

    Vector2 GetDirection()
    {
        float rand = UnityEngine.Random.value;
        if (rand < 0.60f) return Vector2.up;
        if (rand < 0.80f) return Vector2.left;
        return Vector2.right;
    }

    void ChanceToRemove(List<WalkerObject> walkers)
    {
        for (int i = 0; i < walkers.Count; i++)
        {
            if (UnityEngine.Random.value < walkers[i].ChanceToChange && walkers.Count > 1)
            {
                walkers.RemoveAt(i);
                break;
            }
        }
    }

    void ChanceToRedirect(List<WalkerObject> walkers)
    {
        for (int i = 0; i < walkers.Count; i++)
        {
            if (UnityEngine.Random.value < walkers[i].ChanceToChange)
            {
                var w = walkers[i];
                w.Direction = GetDirection();
                walkers[i] = w;
            }
        }
    }

    void ChanceToCreate(List<WalkerObject> walkers)
    {
        int count = walkers.Count;
        for (int i = 0; i < count; i++)
        {
            if (UnityEngine.Random.value < walkers[i].ChanceToChange
                && walkers.Count < MaximumWalkers)
            {
                walkers.Add(new WalkerObject(
                    walkers[i].Position, GetDirection(), 0.5f));
            }
        }
    }

    void UpdatePosition(List<WalkerObject> walkers, Grid[,] grid)
    {
        for (int i = 0; i < walkers.Count; i++)
        {
            var w = walkers[i];
            w.Position += w.Direction;
            w.Position.x = Mathf.Clamp(w.Position.x, 2, grid.GetLength(0) - 3);
            w.Position.y = Mathf.Clamp(w.Position.y, 1, grid.GetLength(1) - 2);
            walkers[i] = w;
        }
    }

    void SpawnDashables(Grid[,] grid, int yOffset)
    {
        for (int x = 1; x < MapWidth - 1; x++)
        {
            for (int y = 1; y < MapHeight - 1; y++)
            {
                if (grid[x, y] != Grid.EMPTY) continue;

                bool hasWall =
                    grid[x + 1, y] == Grid.WALL || grid[x - 1, y] == Grid.WALL ||
                    grid[x, y + 1] == Grid.WALL || grid[x, y - 1] == Grid.WALL;

                if (!hasWall || y < 5) continue;

                if (UnityEngine.Random.value < dashableSpawnChance)
                    Instantiate(dashablePrefab,
                        new Vector3(x + 0.5f, y + yOffset + 0.5f, 0),
                        Quaternion.identity);
            }
        }
    }
}