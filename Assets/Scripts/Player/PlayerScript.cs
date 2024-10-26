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
        [GetOrAddComponent, HideInInspector] public PlayerHealth healthComponent;
        [GetOrAddComponent] private GravityObjectPlayerMovement _playerMovement;
        [GetOrAddComponent] private Animator _animator;

        [Header("Player")] public int playerLives = 5;
        [SerializeField] private LayerMask whatIsInteractable;
        public bool isAttacking;
        public Vector3 lastGroundedPosition;

        private bool _isDead;
        private bool _canAttack = true;

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

        [Header("Debug Logger"), SerializeField, Attributes.Required]
        private DebugLoggerSO debugLogger;

        #region ***---AnimatorStringToHash---***
        private static readonly int AnimatorIsAttackingBool = Animator.StringToHash("IsAttacking");
        private static readonly int AnimatorIsCrouchAttackingBool = Animator.StringToHash("IsCrouchAttacking");
        private static readonly int AnimatorTakeDamageTrigger = Animator.StringToHash("TakeDamage");
        private static readonly int AnimatorDeathTrigger = Animator.StringToHash("Death");
        private static readonly int AnimatorRespawnTrigger = Animator.StringToHash("Respawn");
        private static readonly int AnimatorIsDeadBool = Animator.StringToHash("IsDead");
        #endregion

        #region ***---GameManagers---***
        private static FmodManager fmodManager => FmodManager.Instance;
        private static PauseMenu pauseMenu => PauseMenu.Instance;
        private static SaveLoadSystemManager saveLoadSystemManager => SaveLoadSystemManager.Instance;
        #endregion

        private ScriptVotePair _playerCanMoveVotePair;

        public event Action OnRespawn = delegate { };

        #region ***---DataBindingVariables---***
        [field: Header("Data Binding")]
        [field: SerializeField] public string id { get; set; } = Guid.NewGuid().ToString();

        [field: SerializeField] public PlayerData saveSystemData { get; set; }
        public PlayerData dataInGameData => SaveLoadSystemManager.Instance.gameData.playerData;
        #endregion
        #endregion

        #region ---EventFunctions---
        private void Awake() {
            ServiceLocator.Register(this);
            debugLogger.messageOrigin = this;
        }

        private void Start() {
            Bind();

            healthComponent.OnDeath += () => StartCoroutine(Die());
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

            if (_playerMovement.isDucking) {
                CrouchAttack();
            }
            else {
                Attack();
            }
        }
        #endregion
        #endregion

        #region ---Combat---
        public void Attack() {
            if (!_canAttack) return;
            debugLogger.Log("Attack");

            _animator.SetBool(AnimatorIsAttackingBool, true);
            fmodManager.PlaySfx("Player/Player_slash");
            _canAttack = false;
            isAttacking = true;
        }

        private void CrouchAttack() {
            if (!_canAttack) return;
            debugLogger.Log("CrouchAttack");

            _animator.SetBool(AnimatorIsCrouchAttackingBool, true);
            fmodManager.PlaySfx("Player/Player_slash");
            _canAttack = false;
            isAttacking = true;
        }

        public void AttackFinished() {
            debugLogger.Log("Attack finished");
            _canAttack = true;
            _animator.SetBool(AnimatorIsAttackingBool, false);
            isAttacking = false;
        }

        public void TakeDamage(DamageInstance damageInstance) {
            if (_isDead) return;

            damageInstance.TriggerOnHitAction();
            healthComponent.TakeDamage(damageInstance.damage);

            debugLogger.Log($"Taking Damage: {damageInstance.damage}");
            //_animator.SetTrigger(AnimatorTakeDamageTrigger);
        }

        #region ***---Death---***
        private IEnumerator Die() {
            _isDead = true;
            playerCanMove.ChangeVote(_isAliveVotePair, false);
            _animator.SetBool(AnimatorIsDeadBool, true);
            fmodManager.StopAllSound();
            fmodManager.PlaySfx("Player/Player_death");

            _animator.ResetTrigger(AnimatorTakeDamageTrigger);
            debugLogger.Log("Setting death trigger");
            _animator.SetTrigger(AnimatorDeathTrigger);

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

        private void Respawn() {
            debugLogger.Log("Respawning");

            playerCanMove.ChangeVote(_isAliveVotePair, true);

            _isDead = false;
            _animator.SetBool(AnimatorIsDeadBool, false);
            healthComponent.Health = healthComponent.maxHealth;

            LoseGems();

            OnRespawn.Invoke();

            _animator.ResetTrigger(AnimatorDeathTrigger);
            _animator.SetTrigger(AnimatorRespawnTrigger);
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
        #endregion
        #endregion

        public void GoToLastGroundedPosition() {
            transform.position = lastGroundedPosition;
            _playerMovement.ResetMovement(0);
            debugLogger.Log("Go To Last Grounded Position");
        }

        private Collider2D InteractCheck() {
            return Physics2D.OverlapBox(transform.position, transform.localScale, 0, whatIsInteractable);
        }

        #region ---DataBindingMethods---
        public void Bind() {
            saveLoadSystemManager.Bind(this, dataInGameData);
        }
        
        public void LoadBindData() {
            healthComponent.Health = saveSystemData.health;
            playerLives = saveSystemData.lives;
            gems = saveSystemData.gems;
        }

        public void SaveBindData() {
            saveSystemData.health = healthComponent.Health;
            saveSystemData.lives = playerLives;
            saveSystemData.gems = gems;
        }
        #endregion
    }
}