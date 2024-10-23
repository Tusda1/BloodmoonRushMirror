using System;
using System.Collections;
using Attributes;
using Consensus;
using Fmod;
using PlayerLoopTimer;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player {
    public class PlayerScript : MonoBehaviour, IDamageable {
        [GetOrAddComponent, HideInInspector] public PlayerHealth health;
        [GetOrAddComponent] private GravityPlayer _playerMovement;
        [GetOrAddComponent] private Animator _animator;
        
        public int playerLives = 5;
        public int gems;

        [Header("Player Can Move")]
        [SerializeField, Required] private AllInFavourConsensus playerCanMove;
        private bool playerMovementEnabled => playerCanMove.consensus;
        private readonly ScriptVotePair _isNotAttackingOnGround = new();
        private readonly ScriptVotePair _isAliveVotePair = new();

        [SerializeField] private GameObject hitbox;
        [SerializeField] private PlayerHitbox playerHitbox;
        [SerializeField] private int damage;

        [SerializeField] private LayerMask whatIsInteractable;

        [Header("Debug Logger"), SerializeField]
        private DebugLoggerSO debugLogger;

        private bool _isDead;
        private bool _canAttack = true;
        public bool isAttacking;
        public Vector3 lastGroundedPosition;
        
        private CountdownTimer _attackTimer;

        private static readonly int AnimatorIsAttacking = Animator.StringToHash("IsAttacking");
        private static readonly int AnimatorTakeDamage = Animator.StringToHash("TakeDamage");
        private static readonly int Death = Animator.StringToHash("Death");
        private static readonly int Respawn = Animator.StringToHash("Respawn");
        private static readonly int IsDead = Animator.StringToHash("IsDead");
        
        private static FmodManager fmodManager => FmodManager.Instance;
        private static PauseMenu pauseMenu => PauseMenu.Instance;

        public event Action OnRespawn;

        private ScriptVotePair _playerCanMoveVotePair;

        private void Awake() {
            ServiceLocator.Register(this);
            debugLogger.messageOrigin = this;
            hitbox.SetActive(false);

            InitializeAttackTimer();
            
            health.OnDeath += () => {
                StartCoroutine(Die());
            };
        }

        private IEnumerator Die() {
            _isDead = true;
            playerCanMove.ChangeVote(_isAliveVotePair, false);
            _animator.SetBool(IsDead, true);
            fmodManager.StopAllSound();
            fmodManager.PlaySfx("Player/Player_death");

            _animator.ResetTrigger(AnimatorTakeDamage);
            print("Setting death trigger");
            _animator.SetTrigger(Death);

            yield return new WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsName("Death"));

            yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length + 2f);

            playerCanMove.ChangeVote(_isAliveVotePair, true);
            _animator.SetBool(IsDead, false);

            _isDead = false;
            health.Health = health.maxHealth;
            gems /= 2;
            playerLives--;
            OnRespawn?.Invoke();

            _animator.ResetTrigger(Death);
            _animator.SetTrigger(Respawn);

            if (playerLives <= 0) {
                // TODO
                // SceneManager.LoadScene("Death");
            }
        }

        private void OnDestroy() {
            ServiceLocator.Unregister<PlayerScript>();
            playerCanMove.DeleteVote(_playerCanMoveVotePair);
            playerCanMove.DeleteVote(_isNotAttackingOnGround);
            playerCanMove.DeleteVote(_isAliveVotePair);
        }

        private void InitializeAttackTimer() {
            // playerHitbox.OnKillAction += () => _playerMovement.killCharges++;
            
            _animator.SetBool(AnimatorIsAttacking, false);

            _attackTimer = new CountdownTimer(0.25f);

            _attackTimer.OnBegin += () => {
                debugLogger.Log("Attack" + _attackTimer.CurrentProgress);
                
                fmodManager.PlaySfx("Player/Player_slash");

                hitbox.SetActive(true);
                _canAttack = false;
                isAttacking = true;
                _animator.SetBool(AnimatorIsAttacking, true);
                playerCanMove.ChangeVote(_isNotAttackingOnGround, false);
            };
            _attackTimer.OnStop += OnAttackTimerOnOnComplete;
        }

        private void OnAttackTimerOnOnComplete() {
            debugLogger.Log("Attack done" + _attackTimer.CurrentProgress);
            hitbox.SetActive(false);
            _canAttack = true;
            _animator.SetBool(AnimatorIsAttacking, false);
            playerCanMove.ChangeVote(_isNotAttackingOnGround, true);
            isAttacking = false;
        }

        #region ---InputActions---

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

        public void Attack() {
            if (!_canAttack) return;
            
            _attackTimer.StartTimer();
        }


        public void TakeDamage(DamageInstance damageInstance) {
            if (_isDead) return;

            damageInstance.TriggerOnHitAction();
            health.TakeDamage(damageInstance.damage);

            debugLogger.Log($"Taking Damage: {damageInstance.damage}");
            _animator.SetTrigger(AnimatorTakeDamage);
        }

        public void GoToLastGroundedPosition() {
            transform.position = lastGroundedPosition;
            _playerMovement.ResetMovement(0);
            debugLogger.Log("Go To Last Grounded Position");
        }

        private Collider2D InteractCheck() {
            return Physics2D.OverlapBox(transform.position, transform.localScale, 0, whatIsInteractable);
        }
    }
}