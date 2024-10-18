using AsyncTimers;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class BatSpawner : MonoBehaviour {
    [SerializeField] private SplineContainer batPath;
    [SerializeField] private GameObject batPrefab;
    [SerializeField] private float timeBetweenBats;
    [SerializeField] private int maxBats;
    
    private readonly RuntimeSet<GameObject> _batRuntimeSet = new();
    private TimerRepeating _timer;

    private void Awake() {
        batPath = GetComponent<SplineContainer>();
        
        SpawnBat();
        _timer = new TimerRepeating(timeBetweenBats);
        _timer.OnComplete += () => {
            if (_batRuntimeSet.Count < maxBats) {
                SpawnBat();
            }
        };
    }

    private void OnDestroy() {
        _timer?.CancelTimer();
    }

    private void SpawnBat() {
        var bat = Instantiate(batPrefab, new Vector3(-100,-100,0), quaternion.identity);
        
        var batScript = bat.GetComponentInChildren<Bat>();
        batScript.batRuntimeSet = _batRuntimeSet;
        batScript.parentGameObject = bat;
        
        var batSplineAnimator = bat.GetComponent<SplineAnimate>();
        batSplineAnimator.Container = batPath;
        batSplineAnimator.Play();
    }
}