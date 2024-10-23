using System;
using System.Collections;
using Attributes;
using Consensus;
using ExtensionMethods;
using Fmod;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using TetraCreations.Attributes;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities.DataBinding;

namespace Player {
    public class PlayerScript : MonoBehaviour, IDamageable, IBind<PlayerData> {
        #region ---Fields---
        #region ***---DataBindingVariables---***
        [Header("Data Binding")] public string Id { get; } = Guid.NewGuid().ToString();
        [field: SerializeField] public PlayerData GameData { get; set; }
        public PlayerData DataInGameData => SaveLoadSystemManager.Instance.gameData.playerData;
        #endregion

        [GetOrAddComponent, HideInInspector] public PlayerHealth health;
        [GetOrAddComponent] private GravityPlayer _playerMovement;
        [GetOrAddComponent] private Animator _animator;


        [Header("Player")] public int playerLives = 5;
        [SerializeField] private int damage;
        [SerializeField] private LayerMask whatIsInteractable;

        #region ***---Gems---***
        [Header("Gems")] public int gems;
        [SerializeField] private GemLostMethod gemLostMethod;

        [DrawIf(nameof(gemLostMethod), comparedValue: GemLostMethod.Subtraction)]
        [SerializeField, Tooltip("This number gets subtracted from the gem count")]
        private int gemsLostOnDeath;

        [DrawIf(nameof(gemLostMethod), comparedValue: GemLostMethod.Multiplication)]
        [SerializeField, Tooltip("The gem count gets multiplied by this number and cast to int")]
        private float gemMultiplierOnDeath = 0.9f;

        [Serializable] private enum GemLostMethod { Subtraction, Multiplication }
        #endregion

        [Header("Player Can Move")] [SerializeField, Attributes.Required]
        private AllInFavourConsensus playerCanMove;

        private bool playerMovementEnabled => playerCanMove.consensus;
        private readonly ScriptVotePair _isAliveVotePair = new();

        [Header("Debug Logger"), SerializeField]
        private DebugLoggerSO debugLogger;

        private bool _isDead;
        private bool _canAttack = true;
        public bool isAttacking;
        public Vector3 lastGroundedPosition;

        #region ***---AnimatorStringToHash---***
        private static readonly int AnimatorIsAttacking = Animator.StringToHash("IsAttacking");
        private static readonly int AnimatorTakeDamage = Animator.StringToHash("TakeDamage");
        private static readonly int Death = Animator.StringToHash("Death");
        private static readonly int RespawnTrigger = Animator.StringToHash("Respawn");
        private static readonly int IsDead = Animator.StringToHash("IsDead");
        #endregion

        private static FmodManager fmodManager => FmodManager.Instance;
        private static PauseMenu pauseMenu => PauseMenu.Instance;
        private static SaveLoadSystemManager saveLoadSystemManager => SaveLoadSystemManager.Instance;

        private ScriptVotePair _playerCanMoveVotePair;

        public event Action OnRespawn = delegate { };
        #endregion

        #region ---EventFunctions---
        private void Awake() {
            ServiceLocator.Register(this);
            debugLogger.messageOrigin = this;
        }

        private void Start() {
            // TODO: comment out
            // saveLoadSystemManager.Instance.Bind(this, DataInGameData);

            health.OnDeath += () => StartCoroutine(Die());
        }

        private IEnumerator Die() {
            _isDead = true;
            playerCanMove.ChangeVote(_isAliveVotePair, false);
            _animator.SetBool(IsDead, true);
            fmodManager.StopAllSound();
            fmodManager.PlaySfx("Player/Player_death");

            _animator.ResetTrigger(AnimatorTakeDamage);
            debugLogger.Log("Setting death trigger");
            _animator.SetTrigger(Death);

            yield return new WaitUntil(() => _animator.GetCurrentAnimationClip().name == "Death");
            yield return new WaitForSeconds(_animator.GetCurrentAnimationClip().length + 2f);

            playerLives--;

            if (playerLives <= 0) {
                debugLogger.Log("Ran out of lives");
                // TODO
            }
            else {
                Respawn();
            }
        }

        private void OnDestroy() {
            ServiceLocator.Unregister<PlayerScript>();
        }

        #region ***---InputActions---***
        public void OnPause(InputAction.CallbackContext context) {
            if (!context.performed) return;

            pauseMenu.PauseToggle();
        }

        public void OnInteract(InputAction.CallbackContext context) {
            if (!context.performed || !playerMovementEnabled) return;

            var interactable = InteractCheck();
            if (interactable == null) return;

            if (interactable.TryGetComponent<IInteractable>(out var interactableComponent)) {
                debugLogger.Log("Interact with: " + interactableComponent);
                interactableComponent.Interact(this);
            }
        }

        public void OnAttack(InputAction.CallbackContext context) {
            if (!context.performed || !_canAttack || !playerMovementEnabled) return;

            Attack();
        }
        #endregion
        #endregion

        public void Attack() {
            if (!_canAttack) return;
            debugLogger.Log("Attack");

            _animator.SetBool(AnimatorIsAttacking, true);
            fmodManager.PlaySfx("Player/Player_slash");
            _canAttack = false;
            isAttacking = true;
        }

        public void AttackFinished() {
            debugLogger.Log("Attack finished");
            _canAttack = true;
            _animator.SetBool(AnimatorIsAttacking, false);
            isAttacking = false;
        }

        public void TakeDamage(DamageInstance damageInstance) {
            if (_isDead) return;

            damageInstance.TriggerOnHitAction();
            health.TakeDamage(damageInstance.damage);

            debugLogger.Log($"Taking Damage: {damageInstance.damage}");
            _animator.SetTrigger(AnimatorTakeDamage);
        }

        private void Respawn() {
            debugLogger.Log("Respawning");

            playerCanMove.ChangeVote(_isAliveVotePair, true);

            _isDead = false;
            _animator.SetBool(IsDead, false);
            health.Health = health.maxHealth;

            LoseGems();

            OnRespawn.Invoke();

            _animator.ResetTrigger(Death);
            _animator.SetTrigger(RespawnTrigger);
        }

        private void LoseGems() {
            switch (gemLostMethod) {
                case GemLostMethod.Subtraction:
                    gems -= gemsLostOnDeath;
                    break;
                case GemLostMethod.Multiplication:
                    gems = (int)(gems * gemMultiplierOnDeath);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void GoToLastGroundedPosition() {
            transform.position = lastGroundedPosition;
            _playerMovement.ResetMovement(0);
            debugLogger.Log("Go To Last Grounded Position");
        }

        private Collider2D InteractCheck() {
            return Physics2D.OverlapBox(transform.position, transform.localScale, 0, whatIsInteractable);
        }

        #region ---DataBindingMethods---
        public void LoadBindData() {
            transform.position = GameData.position;
            gems = GameData.gems;
            health.Health = GameData.health;
            playerLives = GameData.lives;
        }

        public void SaveBindData() {
            GameData.position = transform.position;
            GameData.gems = gems;
            GameData.health = health.Health;
            GameData.lives = playerLives;
        }

        public void Bind(PlayerData data) {
            GameData = data;

            debugLogger.Log("Player data bind-ed to gameData");
        }

        public void UnBind(PlayerData data) {
            GameData = null;
        }
        #endregion
    }
}