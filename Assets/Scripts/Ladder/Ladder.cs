using UnityEngine;
#if UNITY_EDITOR
using System;
using Attributes;
using UnityEngine.Tilemaps;
using ExtensionMethods;
using UnityEngine.Assertions;
#endif

public class Ladder : MonoBehaviour {
#if UNITY_EDITOR
    #region ---Fields---
    [SerializeField] private int height;
    
    [Header("LadderTilemaps")]
    [SerializeField] private TileMapInfo ladderTop;
    [SerializeField] private TileMapInfo ladderMiddle;
    [SerializeField] private TileMapInfo ladderBottom;
    #endregion

    [Serializable] private struct TileMapInfo {
        [Required] public Transform transform;
        [Required] public Tilemap tilemap;
        [Required] public Tile tile;
    }
    private void OnValidate() {
        height = Mathf.Clamp(height, 2, int.MaxValue);
    }

    [ContextMenu(nameof(UpdateLadderHeight))]
    public void UpdateLadderHeight() {
        Assert.IsTrue(height >= 2, "Height of this ladder must be greater than 2!");
        
        var effectiveHeight = height - 2;

        SetLadderTopTilemap(effectiveHeight);
        SetLadderMiddleTilemap(effectiveHeight);
        SetLadderBottomTilemap();
    }

    private void SetLadderTopTilemap(in int effectiveHeight) {
        ladderTop.tilemap.ClearAllTiles();
        
        ladderTop.tilemap.SetTile(new Vector3Int(0, 1, 0), ladderTop.tile);
        
        var ladderTopPosition = ladderTop.transform.position;
        ladderTop.transform.position = ladderTopPosition.With(y: effectiveHeight + transform.position.y);
    }

    private void SetLadderMiddleTilemap(in int effectiveHeight) {
        ladderMiddle.tilemap.ClearAllTiles();

        for (var i = 0; i < effectiveHeight; i++) {
            ladderMiddle.tilemap.SetTile(new Vector3Int(0, i, 0), ladderMiddle.tile);
        }
    }

    private void SetLadderBottomTilemap() {
        ladderBottom.tilemap.ClearAllTiles();
        
        ladderBottom.tilemap.SetTile(new Vector3Int(0, 0, 0), ladderBottom.tile);
    }
#endif
}