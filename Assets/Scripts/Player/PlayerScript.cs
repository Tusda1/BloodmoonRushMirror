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
        [GetOrAddComponent] private PlayerMovement _playerMovement;
        
        public int playerLives = 5;
        private bool _isDead;

        public int gems;

        [SerializeField] private ActionEventChannel skipEventChannel;

        [SerializeField] private AllInFavourConsensus playerCanMove;
        private readonly ScriptVotePair _isNotAttackingOnGround = new();
        private readonly ScriptVotePair _isAliveVotePair = new();

        [SerializeField] private GameObject hitbox;
        [SerializeField] private PlayerHitbox playerHitbox;
        [SerializeField] private int damage;

        [SerializeField] private LayerMask whatIsInteractable;

        [Header("Debug Logger"), SerializeField]
        private DebugLoggerSO debugLogger;

        private bool _canAttack = true;
        public bool isAttacking;
        public Vector3 lastGroundedPosition;

        [Header("Knockback")] [SerializeField] private float knockbackForce;
        [SerializeField] private float knockbackDuration;
        [SerializeField] private AnimationCurve knockbackCurve;
        private CountdownTimer _knockbackTimer;
        private CountdownTimer _attackTimer;
        private Vector2 _knockbackDirection;

        private static readonly int AnimatorIsAttacking = Animator.StringToHash("IsAttacking");
        private static readonly int AnimatorTakeDamage = Animator.StringToHash("TakeDamage");
        private static readonly int Death = Animator.StringToHash("Death");
        private static readonly int Respawn = Animator.StringToHash("Respawn");
        private static readonly int IsDead = Animator.StringToHash("IsDead");
        
        private FmodManager fmodManager => FmodManager.Instance;
        private PauseMenu pauseMenu => PauseMenu.Instance;

        public event Action OnRespawn;

        private ScriptVotePair _playerCanMoveVotePair;

        private void Awake() {
            ServiceLocator.Register(this);
            debugLogger.messageOrigin = this;
            hitbox.SetActive(false);

            _playerMovement = GetComponent<PlayerMovement>();

            InitializeKnockbackTimer();
            InitializeAttackTimer();
            
            health.OnDeath += () => {
                playerLives--;
                fmodManager.StopAllSound();
                fmodManager.PlaySfx("Player/Player_death");
                StartCoroutine(Die());
            };
        }

        private IEnumerator Die() {
            _isDead = true;

            _playerMovement.playerCanMove.ChangeVote(_isAliveVotePair, false);

            _playerMovement.animator.SetBool(IsDead, true);

            _playerMovement.animator.ResetTrigger(AnimatorTakeDamage);
            print("Setting death trigger");
            _playerMovement.animator.SetTrigger(Death);

            yield return new WaitUntil(() => _playerMovement.animator.GetCurrentAnimatorStateInfo(0).IsName("Death"));

            yield return new WaitForSeconds(_playerMovement.animator.GetCurrentAnimatorStateInfo(0).length + 2f);

            _playerMovement.playerCanMove.ChangeVote(_isAliveVotePair, true);
            _playerMovement.animator.SetBool(IsDead, false);

            _isDead = false;
            health.Health = health.maxHealth;
            gems /= 2;
            OnRespawn?.Invoke();

            _playerMovement.animator.ResetTrigger(Death);
            _playerMovement.animator.SetTrigger(Respawn);

            if (playerLives <= 0) {
                // TODO
                // SceneManager.LoadScene("Death");
            }
        }

        private void OnDestroy() {
            ServiceLocator.Unregister<PlayerScript>();
            _playerMovement.playerCanMove.DeleteVote(_playerCanMoveVotePair);
            _knockbackTimer.StopTimer();
            playerCanMove.DeleteVote(_isNotAttackingOnGround);
            _playerMovement.playerCanMove.DeleteVote(_isAliveVotePair);
        }

        private void InitializeKnockbackTimer() {
            _knockbackTimer = new CountdownTimer(knockbackDuration);
            _playerCanMoveVotePair = new ScriptVotePair { vote = false };
            _knockbackTimer.OnBegin += () => _playerMovement.playerCanMove.ChangeVote(_playerCanMoveVotePair, false);
            _knockbackTimer.OnUpdate += () => _playerMovement.playerRigidbody.linearVelocity = _knockbackDirection *
                (knockbackForce * knockbackCurve.Evaluate(_knockbackTimer.CurrentProgress));
            _knockbackTimer.OnComplete += () => _playerMovement.playerCanMove.ChangeVote(_playerCanMoveVotePair, true);
        }

        private void InitializeAttackTimer() {
            playerHitbox.OnHitAction += () =>  _playerMovement.killCharges++;
            
            _playerMovement.animator.SetBool(AnimatorIsAttacking, false);

            _attackTimer = new CountdownTimer(0.25f);

            _attackTimer.OnBegin += () => {
                debugLogger.Log("Attack" + _attackTimer.CurrentProgress);
                
                fmodManager.PlaySfx("Player/Player_slash");

                hitbox.SetActive(true);
                playerHitbox.damage = damage;
                _canAttack = false;
                isAttacking = true;
                _playerMovement.animator.SetBool(AnimatorIsAttacking, true);
                playerCanMove.ChangeVote(_isNotAttackingOnGround, false);
            };
            _attackTimer.OnStop += OnAttackTimerOnOnComplete;
        }

        private void OnAttackTimerOnOnComplete() {
            debugLogger.Log("Attack done" + _attackTimer.CurrentProgress);
            hitbox.SetActive(false);
            _canAttack = true;
            _playerMovement.animator.SetBool(AnimatorIsAttacking, false);
            playerCanMove.ChangeVote(_isNotAttackingOnGround, true);
            isAttacking = false;
        }

        #region ---InputActions---

        public void OnPause(InputAction.CallbackContext context) {
            if (!context.performed) return;

            pauseMenu.PauseToggle();
        }

        public void OnInteract(InputAction.CallbackContext context) {
            if (!context.performed || !_playerMovement.playerInputEnabled) return;

            var interactable = InteractCheck();
            if (interactable == null) return;

            if (interactable.TryGetComponent<IInteractable>(out var interactableComponent)) {
                debugLogger.Log("Interact with: " + interactableComponent);
                interactableComponent.Interact(this);
            }
        }

        public void OnAttack(InputAction.CallbackContext context) {
            if (!context.performed || !_canAttack || !_playerMovement.playerInputEnabled ||
                !_playerMovement.isGrounded) return;

            Attack();
        }

        public void OnSkipAction(InputAction.CallbackContext context) {
            if (!context.performed) return;
            skipEventChannel.SendEventMessage();
        }

        #endregion

        public void Attack() {
            if (!_canAttack) return;
            
            _attackTimer.StartTimer();
        }


        public bool TakeDamage(int damageAmount, Vector3 attackDirection = default) {
            if (_isDead) {
                return false;
            }

            health.TakeDamage(damageAmount);

            debugLogger.Log($"Taking Damage: {damageAmount}");
            _playerMovement.animator.SetTrigger(AnimatorTakeDamage);

            if (attackDirection != default) {
                _knockbackDirection = attackDirection;
                _knockbackTimer.StartTimer();
                debugLogger.Log($"Applying Knockback Force: {knockbackForce} in direction: {attackDirection}");
            }

            return false;
        }

        public void GoToLastGroundedPosition() {
            transform.position = lastGroundedPosition;
            _playerMovement.playerRigidbody.linearVelocity = Vector2.zero;
            debugLogger.Log("Go To Last Grounded Position");
        }

        private Collider2D InteractCheck() {
            return Physics2D.OverlapBox(transform.position, transform.localScale, 0, whatIsInteractable);
        }
    }
}