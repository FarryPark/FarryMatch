using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public static BoardController instance;

    private int xSize, ySize;
    private List<Sprite> tileSprite = new List<Sprite>();
    private Tile[,] tileArray;

    private Tile oldSelectTile;
    private Vector2[] dirRay = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

    private bool isFoundMatch = false;
    private bool isShifted = false;
    private bool isSearchEmptyTile = false;

    public void SetValue(Tile[,] tileArray, int xSize, int ySize, List<Sprite> tileSprite)
    {
        this.tileArray = tileArray;
        this.xSize = xSize;
        this.ySize = ySize;
        this.tileSprite = tileSprite;
    }

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (isSearchEmptyTile)
        {
            SearchEmptyTile();
        }

        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit2D ray = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
            if(ray != false)
            {
                CheckSelectTile(ray.collider.gameObject.GetComponent<Tile>());
            }
        }
        
    }

    #region(Выделить тайл, снять с него выделение, управление выделением)
    private void SelectTile(Tile tile)
    {
        tile.isSelected = true;
        tile.spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f);
        oldSelectTile = tile;
    }
    private void DeselectTile(Tile tile)
    {
        tile.isSelected = false;
        tile.spriteRenderer.color = new Color(1, 1, 1);
        oldSelectTile = null;
    }
    private void CheckSelectTile(Tile tile)
    {
        if (tile.isEmpty || isShifted) 
        {
            return;
        }
        if (tile.isSelected)
        {
            DeselectTile(tile);
        }
        else
        {
            // Первое выделение тайла
            if (!tile.isSelected && oldSelectTile == null)
            {
                SelectTile(tile);
            }
            // Попытка выбрать другой тайл
            else
            {
                // Если 2-ой выбранный тайл - сосед предыдущего тайла
                if (AdjacentTiles().Contains(tile))
                {
                    SwapTwoTiles(tile);
                    FindAllMatch(tile);
                    DeselectTile(oldSelectTile);
                }
                // Выделение нового тайла, забываем старый
                else
                {
                    DeselectTile(oldSelectTile);
                    SelectTile(tile);
                }
            }
        }
    }
    #endregion
    #region(Поиск совпадения, удаление спрайтов, поиск всех совпадений)

    private List<Tile> FindMatch(Tile tile, Vector2 dir)
    {
        List<Tile> cashFindTiles = new List<Tile>();
        RaycastHit2D hit = Physics2D.Raycast(tile.transform.position, dir);
        while(hit.collider != null && 
            hit.collider.gameObject.GetComponent<Tile>().spriteRenderer.sprite == tile.spriteRenderer.sprite)
        {
            cashFindTiles.Add(hit.collider.gameObject.GetComponent<Tile>());
            hit = Physics2D.Raycast(hit.collider.gameObject.transform.position, dir);
        }
        return cashFindTiles;
    }

    private void DeleteSprite(Tile tile, Vector2[] dirArray)
    {
        List<Tile> cashFindSprite = new List<Tile>();
        for (int i = 0; i < dirArray.Length; i++)
        {
            cashFindSprite.AddRange(FindMatch(tile, dirArray[i]));
        }
        if (cashFindSprite.Count >= 2)
        {
            for (int i = 0; i < cashFindSprite.Count; i++)
            {
                cashFindSprite[i].spriteRenderer.sprite = null;
            }
            isFoundMatch = true;
        }
    }

    private void FindAllMatch(Tile tile)
    {
        if (tile.isEmpty)
        {
            return;
        }
        DeleteSprite(tile, new Vector2[2] { Vector2.up, Vector2.down });
        DeleteSprite(tile, new Vector2[2] { Vector2.left, Vector2.right });
        if (isFoundMatch)
        {
            isFoundMatch = false;
            tile.spriteRenderer.sprite = null;
            isSearchEmptyTile = true;
        }
    }
    #endregion

    #region(Смена двух тайлов, соседние тайлы)
    private void SwapTwoTiles(Tile tile)
    {
        if (oldSelectTile.spriteRenderer.sprite == tile.spriteRenderer.sprite)
        {
            return;
        }
        Sprite cashSprite = oldSelectTile.spriteRenderer.sprite;
        oldSelectTile.spriteRenderer.sprite = tile.spriteRenderer.sprite;
        tile.spriteRenderer.sprite = cashSprite;
        UI.instance.Moves(1);
    }
    private List<Tile> AdjacentTiles()
    {
        List<Tile> cashTiles = new List<Tile>();
        for (int i = 0; i < dirRay.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(oldSelectTile.transform.position, dirRay[i]);
            if (hit.collider != null)
            {
                cashTiles.Add(hit.collider.gameObject.GetComponent<Tile>());
            }
        }
        return cashTiles;
    }
    #endregion
    #region(Поиск пустого тайла, сдвиг тайла вниз, установка нового изображения, выбрать новое изображение)

    private void SearchEmptyTile()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                if (tileArray[x, y].isEmpty)
                {
                    ShiftTileDown(x, y);
                    break;
                }
                if (x == xSize && y == ySize - 1)
                {
                    isSearchEmptyTile = false;
                }
            }
        }
    }
    private void ShiftTileDown(int xPosition, int yPosition)
    {
        isShifted = true;
        List<SpriteRenderer> cashRenderer = new List<SpriteRenderer>();
        for (int y = yPosition; y < ySize; y++)
        {
            Tile tile = tileArray[xPosition, y];
//            if (tile.isEmpty)
//            {
//                count++;
//            }
            cashRenderer.Add(tile.spriteRenderer);
        }
        //        for (int i = 0; i < count; i++)
        //        {
        //            SetNewSprite(xPosition, cashRenderer);
        //        }
        UI.instance.Score(50);
        SetNewSprite(xPosition, cashRenderer);
        isShifted = false;
    }
    private void SetNewSprite(int xPosition, List<SpriteRenderer> renderer)
    {
        for (int y = 0; y < renderer.Count - 1; y++)
        {
            renderer[y].sprite = renderer[y + 1].sprite;
            renderer[y + 1].sprite = GetNewSprite(xPosition, ySize - 1);
        }
    }
    private Sprite GetNewSprite (int xPosition, int yPosition)
    {
        List<Sprite> cashSprite = new List<Sprite>();
        cashSprite.AddRange(tileSprite);
        if (xPosition > 0)
        {
            cashSprite.Remove(tileArray[xPosition - 1, yPosition].spriteRenderer.sprite);
        }
        if (xPosition < xSize - 1)
        {
            cashSprite.Remove(tileArray[xPosition + 1, yPosition].spriteRenderer.sprite);
        }
        if (yPosition > 0)
        {
            cashSprite.Remove(tileArray[xPosition, yPosition - 1].spriteRenderer.sprite);
        }
        return cashSprite[Random.Range(0, cashSprite.Count)];
    }
    #endregion
}
