using System;
using System.Collections;
using UnityEngine;

public class ExplolsionScript : MonoBehaviour {
    
    private void Start() {
        gameObject.SetActive(true);
    }

    public void Destroy() {
        gameObject.SetActive(false);
    }
}
