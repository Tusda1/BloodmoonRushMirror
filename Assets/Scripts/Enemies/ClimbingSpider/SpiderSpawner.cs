using System;
using System.Collections;
using UnityEngine;

public class SpiderSpawner : MonoBehaviour {
    [SerializeField] private GameObject spiderPrefab;
    [SerializeField] private float spawnDelay = 3f;
    [SerializeField] private GameObject topPosition;
    [SerializeField] private GameObject bottomPosition;
    
    private ClimbingSpider _spider;

    private void Start()
    {
        StartCoroutine(SpawnSpider());
    }

    private IEnumerator SpawnSpider()
    {
        yield return new WaitForSeconds(spawnDelay);
        _spider = Instantiate(spiderPrefab, transform.position, Quaternion.identity).GetComponent<ClimbingSpider>();
        _spider.topPosition = topPosition.transform.position;
        _spider.bottomPosition = bottomPosition.transform.position;
        _spider.OnSpiderDestroyed += () => StartCoroutine(SpawnSpider());
    }
}
