using System;
using UnityEngine;

public class Despawner : MonoBehaviour {
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Despawnable"))
        {
            other.gameObject.SetActive(false);
        }
    }
}
