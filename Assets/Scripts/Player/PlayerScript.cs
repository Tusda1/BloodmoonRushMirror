using System;
using System.Collections;
using Attributes;
using Consensus;
using ExtensionMethods;
using FMODScripts;
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
        #region ***---LocalComponentRefrences---***
        [GetOrAddComponent, HideInInspector] public PlayerHealth healthComponent;
        [GetOrAddComponent, HideInInspector] public Animator animator;
        [GetOrAddComponent, HideInInspector] public SpriteRenderer spriteRenderer;
        [GetOrAddComponent, HideInInspector] public GravityObjectPlayerMovement playerMovement;
        #endregion
        
        [Header("Player")] 
        public int playerLives = 5;
        [SerializeField] private LayerMask whatIsInteractable;
        public bool isAttacking;
        public Vector3 lastGroundedPosition;
        [SerializeField] private PlayerHitbox hitbox;
        [SerializeField] private float alphaWhenTakingDamage = 0.2f;

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
        private AllInFavourConsensusSO playerCanMove;

        private bool PlayerMovementEnabled => playerCanMove.consensus;
        private readonly ScriptVotePair _isAliveVotePair = new();

        [Header("Debug Logger"), SerializeField, Attributes.Required]
        private DebugLoggerSO debugLogger;

        #region ***---AnimatorStringToHash---***
        private static readonly int AnimatorIsAttackingTrigger = Animator.StringToHash("IsAttacking");
        private static readonly int AnimatorDuckAttackTrigger = Animator.StringToHash("DuckAttack");
        private static readonly int AnimatorTakeDamageTrigger = Animator.StringToHash("TakeDamage");
        private static readonly int AnimatorIsDeadBool = Animator.StringToHash("IsDead");
        private static readonly int AnimatorRespawnTrigger = Animator.StringToHash("Respawn");
        private static readonly int AnimatorDeathTrigger = Animator.StringToHash("Death");
        
        #endregion

        #region ***---GameManagers---***
        private static FMODManager FMODManager => FMODManager.Instance;
        private static PauseMenu PauseMenu => PauseMenu.Instance;
        private static SaveLoadSystemManager SaveLoadSystemManager => SaveLoadSystemManager.Instance;
        #endregion

        private ScriptVotePair _playerCanMoveVotePair;

        public event Action OnRespawn = delegate { };

        #region ***---DataBindingVariables---***
        [field: Header("Data Binding")]
        [field: SerializeField]
        public string ID { get; set; } = Guid.NewGuid().ToString();

        [field: SerializeField] public PlayerData SaveSystemData { get; set; }
        public PlayerData DataInGameData => SaveLoadSystemManager.Instance.gameData.playerData;
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
            
            healthComponent.InvulnerabilityTimer.OnBegin += () => spriteRenderer.color = new Color(1,1,1,alphaWhenTakingDamage);
            healthComponent.InvulnerabilityTimer.OnComplete += () => spriteRenderer.color = new Color(1,1,1,1);

            SetStateBehavioursReferences();
        }

        private void OnDestroy() {
            ServiceLocator.Unregister<PlayerScript>();
        }

        #region ***---InputActions---***
        public void OnPause(InputValue inputValue) {
            if (!inputValue.isPressed) return;
            
            PauseMenu.PauseToggle();
        }

        public void OnInteract(InputValue inputValue) {
            if (!inputValue.isPressed || !PlayerMovementEnabled) return;

            var interactable = InteractCheck();
            if (interactable == null) return;

            if (interactable.TryGetComponent<IInteractable>(out var interactableComponent)) {
                debugLogger.Log("Interact with: " + interactableComponent);
                interactableComponent.Interact(this);
            }
        }

        public void OnAttack(InputValue inputValue) {
            if (!inputValue.isPressed) return;
            debugLogger.Log($"canAttack: {_canAttack} | canMove: {PlayerMovementEnabled}");
            if (!_canAttack || !PlayerMovementEnabled) return;

            if (playerMovement.isDucking) {
                DuckAttack();
            }
            else {
                Attack();
            }
        }
        #endregion
        #endregion

        #region ---Combat---
        private void Attack() {
            if (!_canAttack) return;
            debugLogger.Log("Attack");

            animator.SetTrigger(AnimatorIsAttackingTrigger);
            FMODManager.PlaySfx("Player/Player_slash");
            _canAttack = false;
            isAttacking = true;
        }

        private void DuckAttack() {
            if (!_canAttack) return;
            debugLogger.Log("DuckAttack");

            animator.SetTrigger(AnimatorDuckAttackTrigger);
            FMODManager.PlaySfx("Player/Player_slash");
            _canAttack = false;
            isAttacking = true;
        }

        public void AttackFinished() {
            debugLogger.Log("Attack finished");
            _canAttack = true;
            isAttacking = false;
        }

        public void TakeDamage(DamageInstance damageInstance) {
            if (_isDead) return;

            damageInstance.TriggerOnHitAction();
            healthComponent.TakeDamage(damageInstance.damage);
            playerMovement.Knockback(damageInstance.knockback);
            debugLogger.Log($"Taking Damage: {damageInstance.damage}");
            //_animator.SetTrigger(AnimatorTakeDamageTrigger);
        }

        #region ***---Death---***
        private IEnumerator Die() {
            _isDead = true;
            playerCanMove.ChangeVote(_isAliveVotePair, false);
            animator.SetBool(AnimatorIsDeadBool, true);
            FMODManager.StopAllSound();
            FMODManager.PlaySfx("Player/Player_death");

            animator.ResetTrigger(AnimatorTakeDamageTrigger);
            debugLogger.Log("Setting death trigger");
            animator.SetTrigger(AnimatorDeathTrigger);

            yield return new WaitUntil(() => animator.GetCurrentAnimationClip().name == "Death");
            yield return new WaitForSeconds(animator.GetCurrentAnimationClip().length + 2f);

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
            animator.SetBool(AnimatorIsDeadBool, false);
            healthComponent.Health = healthComponent.maxHealth;

            LoseGems();

            OnRespawn.Invoke();

            animator.ResetTrigger(AnimatorDeathTrigger);
            animator.SetTrigger(AnimatorRespawnTrigger);
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

        public void HitSpike() {
            playerMovement.SpikeReset();
        }

        private Collider2D InteractCheck() {
            return Physics2D.OverlapBox(transform.position, transform.localScale, 0, whatIsInteractable);
        }

        private void SetStateBehavioursReferences() {
            var playerAttackBehaviours = animator.GetBehaviours<PlayerAttackBehaviour>();
            foreach (var playerAttackBehaviour in playerAttackBehaviours) {
                playerAttackBehaviour.hitbox = hitbox;
            }
        }

        #region ---DataBindingMethods---
        public void Bind() {
            SaveLoadSystemManager.Bind(this, DataInGameData);
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (gameObject.activeInHierarchy && string.IsNullOrEmpty(ID)) {
                ID = Guid.NewGuid().ToString();
                debugLogger.Log($"Setting new guid for {nameof(PlayerScript)}");
            }
        }
#endif

        [ContextMenu(nameof(LoadBindData))]
        public void LoadBindData() {
            healthComponent.Health = SaveSystemData.health;
            playerLives = SaveSystemData.lives;
            gems = SaveSystemData.gems;
        }

        [ContextMenu(nameof(SaveBindData))]
        public void SaveBindData() {
            SaveSystemData.health = healthComponent.Health;
            SaveSystemData.lives = playerLives;
            SaveSystemData.gems = gems;
        }
        #endregion
    }
}