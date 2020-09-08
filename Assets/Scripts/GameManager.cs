using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoardSetting
{
    public int xSize, ySize;
    public Tile tileGO;
    public List<Sprite> tileSprite;
}

public class GameManager : MonoBehaviour
{
    [Header ("Параметры игровой доски")]
    public BoardSetting boardSetting;
    // Start is called before the first frame update
    void Start()
    {
        BoardController.instance.SetValue(Board.instance.SetValue(boardSetting.xSize, boardSetting.ySize, boardSetting.tileGO, boardSetting.tileSprite),
            boardSetting.xSize, boardSetting.ySize,
            boardSetting.tileSprite);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
