using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using VContainer;

public class CollapsingFloor : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private Tilemap tilesToExplode;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private Vector2Int explosionTileSize;
    [SerializeField] private GameObject explosionCarrier;
    [SerializeField] private float delayTime;
    public IAudioManager AudioManager;
    [Inject]
    public void Construct(IObjectResolver container) {
        AudioManager = container.Resolve<IAudioManager>();
    }
    #endregion

    private void Start() {
        SetExplodingTiles();
    }

    public void SetExplodingTiles() {
        tilesToExplode.ClearAllTiles();
        for (var x = 0; x < explosionTileSize.x; x++) {
            for (var y = 0; y < explosionTileSize.y; y++) {
                tilesToExplode.SetTile(new Vector3Int(x, y, 0), floorTile);
            }
        }
    }

    public void Explode() {
        StartCoroutine(ExplodingCoroutine());
    }

    private IEnumerator ExplodingCoroutine() {
        int middle = explosionTileSize.x / 2;
        bool isOdd;
        if (explosionTileSize.x % 2 == 0) {
            isOdd = false;
            print("even");
        }
        else {
            ExplodeRow(middle);
            isOdd = true;
            print("odd");
        }
        
        if (isOdd)
            for (int i = 1; i <= middle; i++) {
                yield return new WaitForSeconds(delayTime);
                ExplodeRow(middle + i);
                ExplodeRow(middle - i);
            }
        else {
            for (int i = 1; i <= middle; i++) {
                yield return new WaitForSeconds(delayTime);
                ExplodeRow(middle + i - 1);
                ExplodeRow(middle - i);
            }
        }
        yield return null;
    }

    private void ExplodeRow(int whichRow) {
        for (var i = 0; i < explosionTileSize.y; i++) {
            tilesToExplode.SetTile(new Vector3Int(whichRow, i, 0), null);
            AudioManager.PlayOneShot("Misc_Floor_Explosion");
             Instantiate(explosionCarrier, new Vector3(whichRow + 0.5f, i + 0.5f, 0) + transform.position, Quaternion.identity, transform);
        }
    }
}
