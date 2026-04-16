using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WalkerGenerator : MonoBehaviour
{
    public enum Grid
    {
        FLOOR,
        WALL,
        EMPTY
    }

    // Variables
    public Grid[,] gridHandler;
    public List<WalkerObject> Walkers;
    public Tilemap tileMap;
    public Tile Floor;
    public Tile Wall;
    public int MapWidth = 10;
    public int MapHeight = 60;
    public int MaximumWalkers = 3;
    public int TileCount = default;
    public float FillPercentage = 0.20f;

    void Start()
    {
        InitializeGrid();
    }

    // Initialise la grille, place le premier walker en bas au centre,
    // génère tout instantanément puis rend la map
    void InitializeGrid()
    {
        gridHandler = new Grid[MapWidth, MapHeight];

        // Tout est mur par défaut — le walker va creuser dedans
        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                gridHandler[x, y] = Grid.WALL;
            }
        }

        Walkers = new List<WalkerObject>();

        // Départ en bas au centre, le walker monte vers le haut
        Vector2 startPos = new Vector2(MapWidth / 2, 2);

        WalkerObject curWalker = new WalkerObject(startPos, GetDirection(), 0.5f);
        gridHandler[(int)startPos.x, (int)startPos.y] = Grid.FLOOR;
        TileCount++;
        Walkers.Add(curWalker);

        // Génération instantanée (synchrone)
        CreateFloors();

        // Rendu de toute la map en une passe
        RenderMap();

        // Centrer la caméra sur le point de départ du joueur
        Camera.main.transform.position = new Vector3(
            startPos.x,
            startPos.y,
            Camera.main.transform.position.z
        );
    }

    // Retourne une direction aléatoire biaisée vers le haut (60%)
    // pour simuler un puits qu'on creuse verticalement
    Vector2 GetDirection()
    {
        float rand = UnityEngine.Random.value;

        if (rand < 0.60f) return Vector2.up;    // 60% vers le haut
        if (rand < 0.80f) return Vector2.left;  // 20% gauche
        if (rand < 1.00f) return Vector2.right; // 20% droite
        return Vector2.up;
    }

    // Boucle principale de génération (synchrone) : fait marcher les walkers
    // jusqu'à atteindre le pourcentage de remplissage souhaité
    void CreateFloors()
    {
        int maxIterations = MapWidth * MapHeight * 10; // sécurité anti-boucle infinie
        int iterations = 0;

        while ((float)TileCount / (float)gridHandler.Length < FillPercentage)
        {
            iterations++;
            if (iterations > maxIterations)
            {
                Debug.LogWarning("Génération stoppée : trop d'itérations");
                break;
            }

            foreach (WalkerObject curWalker in Walkers)
            {
                Vector3Int curPos = new Vector3Int(
                    (int)curWalker.Position.x,
                    (int)curWalker.Position.y,
                    0
                );

                if (gridHandler[curPos.x, curPos.y] != Grid.FLOOR)
                {
                    gridHandler[curPos.x, curPos.y] = Grid.FLOOR;
                    TileCount++;
                }
            }

            ChanceToRemove();
            ChanceToRedirect();
            ChanceToCreate();
            UpdatePosition();
        }
    }

    // Place toutes les tuiles en une seule passe après la génération
    void RenderMap()
    {
        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (gridHandler[x, y] == Grid.FLOOR)
                    tileMap.SetTile(pos, Floor);
                else
                    tileMap.SetTile(pos, Wall);
            }
        }
    }

    // Supprime aléatoirement un walker s'il y en a plusieurs,
    // pour éviter que la map soit trop ouverte
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

    // Change aléatoirement la direction d'un walker
    // pour créer des couloirs qui ne vont pas tous dans la même direction
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

    // Crée aléatoirement un nouveau walker à la position d'un walker existant,
    // dans la limite de MaximumWalkers, pour élargir certaines zones
    void ChanceToCreate()
    {
        int updatedCount = Walkers.Count;
        for (int i = 0; i < updatedCount; i++)
        {
            if (UnityEngine.Random.value < Walkers[i].ChanceToChange && Walkers.Count < MaximumWalkers)
            {
                Vector2 newDirection = GetDirection();
                Vector2 newPosition = Walkers[i].Position;
                WalkerObject newWalker = new WalkerObject(newPosition, newDirection, 0.5f);
                Walkers.Add(newWalker);
            }
        }
    }

    // Déplace chaque walker d'une case dans sa direction,
    // en le maintenant dans les limites de la grille avec une marge
    // de 2 tuiles sur les côtés (pour garantir des murs latéraux)
    void UpdatePosition()
    {
        for (int i = 0; i < Walkers.Count; i++)
        {
            WalkerObject FoundWalker = Walkers[i];
            FoundWalker.Position += FoundWalker.Direction;

            // 2 tuiles de marge sur les côtés gauche/droit = murs latéraux garantis
            FoundWalker.Position.x = Mathf.Clamp(FoundWalker.Position.x, 2, gridHandler.GetLength(0) - 3);
            // 1 tuile de marge en haut/bas
            FoundWalker.Position.y = Mathf.Clamp(FoundWalker.Position.y, 1, gridHandler.GetLength(1) - 2);

            Walkers[i] = FoundWalker;
        }
    }
}