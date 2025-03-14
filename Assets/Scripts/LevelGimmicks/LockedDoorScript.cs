using System;
using System.Collections;
using Player;
using UnityEngine;

public class LockedDoorScript : MonoBehaviour, IPlayerInteractable {
    private static readonly int IsOpened = Animator.StringToHash("isOpened");
    private static readonly int IsAlreadyOpened = Animator.StringToHash("isAlreadyOpened");

    #region ---Fields---
    [SerializeField] private BoxCollider2D openCollider;
    [SerializeField] private BoxCollider2D closedCollider;
    [SerializeField] private int openKeyAmount;
    [SerializeField] private float destructionTime;
    [SerializeField] private Animator animator;
    private bool _isOpened;
    #endregion

    private void OnEnable() {
        if (_isOpened) {
            animator.SetBool(IsAlreadyOpened, true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (_isOpened) return;
        if (other.CompareTag("Player"))
            if (other.TryGetComponent<PlayerScript>(out var playerScript)) {
                Interact(playerScript);
            }
    }

    public void Interact(PlayerScript playerScript) {
        if (playerScript.keyObjects.Count < openKeyAmount) return;
        playerScript.UseKey(transform, openKeyAmount, destructionTime);
                
        StartCoroutine(OpenDoor());
    }

    IEnumerator OpenDoor() {
        yield return new WaitForSeconds(destructionTime);
        _isOpened = true;
        animator.SetBool(IsOpened, true);
        closedCollider.enabled = false;
    }
}
