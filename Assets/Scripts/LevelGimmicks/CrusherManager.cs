using System;
using PlayerLoopTimer;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class CrusherManager : MonoBehaviour {
    #region ---Fields---
    [Header("Tiles")]
    [SerializeField] private Tilemap crusherTilemap;
    [SerializeField] private Tilemap coverTilemap;
    [SerializeField] private RuleTile crusherTile;
    [SerializeField] private RuleTile coverTile;
    [SerializeField] private bool repaint = false;
    [Header("Size")]
    [SerializeField] private Vector2Int crusherSize;
    [FormerlySerializedAs("crusherUpSpeed")]
    [Header("Time")]
    [SerializeField] private float crusherDownSpeed;
    [SerializeField] private float crusherUpSpeed;
    [SerializeField] private float crusherDownWaitTime;
    [SerializeField] private float crusherUpWaitTime;
    [Header("Crusher Refrence")]
    [SerializeField] private CrusherScript crusherRefrence;
    private CountdownTimer _upWaitTimer;
    private CountdownTimer _downWaitTimer;
    #endregion

    private void Start() {
        GenerateCrusherTiles();
        
        _upWaitTimer = new CountdownTimer(crusherUpWaitTime);
        _downWaitTimer = new CountdownTimer(crusherDownWaitTime);
        
        _upWaitTimer.OnComplete += Down;
        _downWaitTimer.OnComplete += Up;

        crusherRefrence.upSpeed = crusherUpSpeed;
        crusherRefrence.downSpeed = crusherDownSpeed;
        
        _upWaitTimer.StartTimer();
    }

    private void Update() {
        if (crusherRefrence.isStill) {
            if (crusherRefrence.goingDown) {
                _downWaitTimer.StartTimer();
            }
            else {
                _upWaitTimer.StartTimer();
            }
        }
    }

    public void GenerateCrusherTiles() {
        crusherTilemap.transform.position = - new Vector3(crusherSize.x - 1, crusherSize.y, 0)/2 + transform.position;
        coverTilemap.transform.position = - new Vector3(crusherSize.x, crusherSize.y, 0)/2 + transform.position;

        if (repaint) {
            coverTilemap.ClearAllTiles();
            crusherTilemap.ClearAllTiles();
            for (int x = 0; x < crusherSize.x; x++) {
                for (int y = 0; y < crusherSize.y; y++) {
                    coverTilemap.SetTile(new Vector3Int(x, y, 0), coverTile);
                }
            }
            for (int x = 0; x < crusherSize.x - 1; x++) {
                for (int y = 0; y < crusherSize.y; y++) {
                    crusherTilemap.SetTile(new Vector3Int(x, y, 0), crusherTile);
                }
            }
        }

        crusherRefrence.startPos = crusherTilemap.transform.position;
        crusherRefrence.endPos = new Vector3(crusherTilemap.transform.position.x, transform.position.y - (crusherSize.y*1.5f), 0);
    }

    private void Down() {
        crusherRefrence.isStill = false;
        crusherRefrence.goingDown = true;
        crusherRefrence.triggerCollider2D.isTrigger = true;
    }

    private void Up() {
        crusherRefrence.isStill = false;
        crusherRefrence.goingDown = false;
    }
    
#if UNITY_EDITOR
    private void OnValidate() {
        if (_upWaitTimer != null) _upWaitTimer.Duration = crusherUpWaitTime;
        if (_downWaitTimer != null) _downWaitTimer.Duration = crusherDownWaitTime;
        if (crusherRefrence != null) {
            crusherRefrence.upSpeed = crusherUpSpeed;
            crusherRefrence.downSpeed = crusherDownSpeed;
        }
    }
#endif
}
