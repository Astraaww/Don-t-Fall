using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WalkerGenerator : MonoBehaviour
{
    public enum Grid
    {
        EMPTY,  // chemin vide — le joueur peut passer
        WALL,   // plateforme solide
    }

    public Grid[,] gridHandler;
    public List<WalkerObject> Walkers;

    public Tilemap tilemapWalls;       // Tilemap des murs (avec Collider)
    public Tilemap tilemapBackground;  // Tilemap de fond (sans Collider, optionnel)

    public RuleTile Wall;              // Ton Rule Tile avec autotiling
    public TileBase Background;        // Tile de fond (simple Tile, peut être null)

    public int MapWidth = 10;
    public int MapHeight = 60;
    public int MaximumWalkers = 3;
    public int TileCount = default;
    public float FillPercentage = 0.20f; // % de cases EMPTY (chemins)

    public Transform playerTransform;

    void Start()
    {
        InitializeGrid();
    }

    void InitializeGrid()
    {
        gridHandler = new Grid[MapWidth, MapHeight];

        for (int x = 0; x < MapWidth; x++)
            for (int y = 0; y < MapHeight; y++)
                gridHandler[x, y] = Grid.WALL;

        Walkers = new List<WalkerObject>();

        Vector2 startPos = new Vector2(MapWidth / 2, 2);

        WalkerObject curWalker = new WalkerObject(startPos, GetDirection(), 0.5f);
        gridHandler[(int)startPos.x, (int)startPos.y] = Grid.EMPTY;
        TileCount++;
        Walkers.Add(curWalker);

        CreatePaths();
        RenderMap();

        // Spawn du joueur sur la première case EMPTY
        Vector2 spawnPos = FindSpawnPoint();
        playerTransform.position = new Vector3(spawnPos.x + 0.5f, spawnPos.y + 0.5f, 0);

        // Caméra sur le point de spawn
        Camera.main.transform.position = new Vector3(
            spawnPos.x,
            spawnPos.y,
            Camera.main.transform.position.z
        );
    }

    // Cherche la première case EMPTY en partant du bas au centre
    Vector2 FindSpawnPoint()
    {
        int centerX = MapWidth / 2;
        for (int y = 0; y < MapHeight; y++)
        {
            if (gridHandler[centerX, y] == Grid.EMPTY)
                return new Vector2(centerX, y);
        }
        return new Vector2(centerX, 2); // fallback
    }

    // Retourne une direction biaisée vers le haut (60%)
    Vector2 GetDirection()
    {
        float rand = UnityEngine.Random.value;
        if (rand < 0.60f) return Vector2.up;
        if (rand < 0.80f) return Vector2.left;
        return Vector2.right;
    }

    // Les walkers creusent des chemins EMPTY dans la grille de WALL
    void CreatePaths()
    {
        int maxIterations = MapWidth * MapHeight * 10;
        int iterations = 0;

        while ((float)TileCount / gridHandler.Length < FillPercentage)
        {
            if (++iterations > maxIterations)
            {
                Debug.LogWarning("Génération stoppée : trop d'itérations");
                break;
            }

            foreach (WalkerObject curWalker in Walkers)
            {
                int x = (int)curWalker.Position.x;
                int y = (int)curWalker.Position.y;

                if (gridHandler[x, y] != Grid.EMPTY)
                {
                    gridHandler[x, y] = Grid.EMPTY;
                    TileCount++;
                }
            }

            ChanceToRemove();
            ChanceToRedirect();
            ChanceToCreate();
            UpdatePosition();
        }
    }

    // Rendu : place les murs là où c'est WALL, rien là où c'est EMPTY
    void RenderMap()
    {
        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (gridHandler[x, y] == Grid.WALL)
                {
                    tilemapWalls.SetTile(pos, Wall);
                }
                else
                {
                    // Tile de fond optionnelle sur les cases vides
                    if (Background != null && tilemapBackground != null)
                        tilemapBackground.SetTile(pos, Background);
                }
            }
        }

        // Force le recalcul du Composite Collider
        Physics2D.SyncTransforms();
    }

    void ChanceToRemove()
    {
        int updatedCount = Walkers.Count;
        for (int i = 0; i < updatedCount; i++)
        {
            if (UnityEngine.Random.value < Walkers[i].ChanceToChange && Walkers.Count > 1)
            {
                Walkers.RemoveAt(i);
                break;
            }
        }
    }

    void ChanceToRedirect()
    {
        for (int i = 0; i < Walkers.Count; i++)
        {
            if (UnityEngine.Random.value < Walkers[i].ChanceToChange)
            {
                WalkerObject curWalker = Walkers[i];
                curWalker.Direction = GetDirection();
                Walkers[i] = curWalker;
            }
        }
    }

    void ChanceToCreate()
    {
        int updatedCount = Walkers.Count;
        for (int i = 0; i < updatedCount; i++)
        {
            if (UnityEngine.Random.value < Walkers[i].ChanceToChange && Walkers.Count < MaximumWalkers)
            {
                WalkerObject newWalker = new WalkerObject(
                    Walkers[i].Position,
                    GetDirection(),
                    0.5f
                );
                Walkers.Add(newWalker);
            }
        }
    }

    void UpdatePosition()
    {
        for (int i = 0; i < Walkers.Count; i++)
        {
            WalkerObject w = Walkers[i];
            w.Position += w.Direction;

            // 2 tuiles de marge sur les côtés = murs latéraux garantis
            w.Position.x = Mathf.Clamp(w.Position.x, 2, gridHandler.GetLength(0) - 3);
            w.Position.y = Mathf.Clamp(w.Position.y, 1, gridHandler.GetLength(1) - 2);

            Walkers[i] = w;
        }
    }
}