using System;
using System.Collections.Generic;
using System.Threading;
using PlayerLoopTimer;
using TetraCreations.Attributes;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

public class ActivatingBook : MonoBehaviour, IDamageable {
    private static readonly int Hit = Animator.StringToHash("Hit");

    #region ---Fields---
    [SerializeField] private List<GameObject> bookList = new List<GameObject>();
    [SerializeField] private float activationTime;
    private CountdownTimer _activationTimer;
    [SerializeField] private GameObject book;
    [SerializeField] private Animator animator;
    #endregion

    private void Start() {
        _activationTimer = new CountdownTimer(activationTime);
        _activationTimer.OnComplete += CloseBooks;
    }

    public void TakeDamage(DamageInstance damageInstance) {
        animator.SetTrigger(Hit);
        foreach (GameObject books in bookList) {
            books.GetComponent<BookScript>().OpenThisBook();
        }

        if (_activationTimer.IsRunning) {
            _activationTimer.RestartTimer();
        }
        else {
            _activationTimer.StartTimer();
        }
    }

    private void CloseBooks() {
        foreach (GameObject books in bookList) {
            books.GetComponent<BookScript>().CloseThisBook();
        }
    }
    
    [ContextMenu(nameof(AddBook)), Button(nameof(AddBook))]
    public void AddBook() {
        var obj = Instantiate(book, transform.position, Quaternion.identity, transform);
        bookList.Add(obj);
    }

    [ContextMenu(nameof(RemoveBook)), Button(nameof(RemoveBook))]
    public void RemoveBook() {
        if (bookList.Count < 1) return;
        var obj = bookList[^1];
        DestroyImmediate(obj);
        bookList.Remove(obj);
    }
    
#if UNITY_EDITOR
    private void OnValidate() {
        if (_activationTimer != null) _activationTimer.Duration = activationTime;
    }
#endif
}
