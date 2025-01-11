using System;
using System.Collections;
using System.Collections.Generic;
using Attributes;
using Consensus;
using FMODScripts;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using SceneCollections;
using TetraCreations.Attributes;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities.DataBinding;

namespace Player {
    public class PlayerScript : MonoBehaviour, IDamageable, IBind<PlayerData> {
        #region ---Fields---
        #region ***---GetOrAddComponents---***
        [GetOrAddComponent, HideInInspector] public PlayerHealth healthComponent;
        [GetOrAddComponent, HideInInspector] public SpriteRenderer spriteRenderer;
        [GetOrAddComponent, HideInInspector] public GravityObjectPlayerMovement playerMovement;
        [GetOrAddComponent, HideInInspector] public PlayerInput playerInput;
        #endregion

        #region ***---SerializedFields---***
        [SerializeField] public MultipleAnimatorControllers animator;
        [SerializeField] private SceneCollectionSO gameOverScene;

        [Header("Player")] 
        public int playerLives = 5;
        [SerializeField] private LayerMask whatIsInteractable;
        public bool isAttacking;
        [SerializeField] private PlayerHitbox hitbox;
        [SerializeField] private ParticleSystem bloodSplatter;

        [Header("Taking Damage")] 
        [SerializeField] private float alphaWhenTakingDamage = 0.2f;

        [SerializeField] private float takeDamageScreenShakeIntensity = 0.2f;
        [SerializeField] private float takeDamageScreenShakeDuration = 0.2f;
        [SerializeField] private float takeDamageTimeSlowDuration = 0.5f;

        [Header("Keys")] 
        public int keyAmount;
        public List<GameObject> keyObjects = new();

        [Header("Player Can Move")] 
        [SerializeField, Attributes.Required] private AllInFavourConsensusSO playerCanMove;

        [Header("Debug Logger")] 
        [SerializeField, Attributes.Required] public DebugLoggerSO debugLogger;
        #endregion

        private bool _isDead;
        private bool _canAttack = true;

        private bool PlayerMovementEnabled => playerCanMove.consensus;
        private readonly ScriptVotePair _isAliveVotePair = new();

        public event Action OnRespawn = delegate { };

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

        #region ***---AnimatorStringToHash---***
        private static readonly int AnimatorIsAttackingBool = Animator.StringToHash("IsAttacking");
        private static readonly int AnimatorDuckAttackTrigger = Animator.StringToHash("DuckAttack");
        private static readonly int AnimatorTakeDamageTrigger = Animator.StringToHash("TakeDamage");
        private static readonly int AnimatorIsDeadBool = Animator.StringToHash("IsDead");
        private static readonly int AnimatorRespawnTrigger = Animator.StringToHash("Respawn");
        private static readonly int AnimatorDeathTrigger = Animator.StringToHash("Death");
        #endregion

        #region ***---GameManagers---***
        private static PauseMenu PauseMenu => PauseMenu.Instance;
        private static SaveLoadSystemManager SaveLoadSystemManager => SaveLoadSystemManager.Instance;
        #endregion

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
            if (DataInGameData != null) {
                Bind();
            }

            healthComponent.OnDeath += () => StartCoroutine(Die());

            healthComponent.InvulnerabilityTimer.OnBegin +=
                () => spriteRenderer.color = new Color(1, 1, 1, alphaWhenTakingDamage);
            healthComponent.InvulnerabilityTimer.OnComplete += () => spriteRenderer.color = new Color(1, 1, 1, 1);

            SetStateBehavioursReferences();
            
            Gamepad.current?.SetMotorSpeeds(0.123f, 0.234f);
            InputSystem.PauseHaptics();
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            StartCoroutine(ResetPlayerInput());
        }

        private void OnDestroy() {
            ServiceLocator.Unregister<PlayerScript>();
            playerCanMove.Clear();
            InputSystem.PauseHaptics();
        }

        #region ***---InputActions---***
        public void OnPause(InputValue inputValue) {
            if (!inputValue.isPressed) return;

            PauseMenu.PauseToggle();
        }

        public void OnSkipDialogue(InputValue inputValue) {
            if (!inputValue.isPressed) return;
            DialogueUI.Instance?.TriggerOnSkipDialogue();
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
            if (playerMovement.isDashing) return;
            debugLogger.Log($"canAttack: {_canAttack} | canMove: {PlayerMovementEnabled}");
            if (!_canAttack || !PlayerMovementEnabled) return;

            if (playerMovement.isClimbing) {
                var u = playerMovement.inputVector.x >= 0 ? 1 : -1;
                transform.localScale = new Vector3(u, 1, 1);
            }
            else {
                transform.localScale = playerMovement.isFacingRight ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
            }
            
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
            
            FMODManager.RaiseSFXEvent("Player/Player_slash");
            _canAttack = false;
            isAttacking = true;
            animator.SetBool(AnimatorIsAttackingBool, true);
        }

        private void DuckAttack() {
            if (!_canAttack) return;
            debugLogger.Log("DuckAttack");

            animator.SetTrigger(AnimatorDuckAttackTrigger);
            FMODManager.RaiseSFXEvent("Player/Player_slash");
            _canAttack = false;
            isAttacking = true;
            animator.SetBool(AnimatorIsAttackingBool, true);
        }

        public void AttackFinished() {
            debugLogger.Log("Attack finished");
            _canAttack = true;
            isAttacking = false;
            animator.SetBool(AnimatorIsAttackingBool, false);
        }

        public void TakeDamage(DamageInstance damageInstance) {
            if (healthComponent.HasInvincibilityFrames && !damageInstance.ignoreInvincibility) return;
            if (_isDead) return;

            debugLogger.Log($"Taking Damage: {damageInstance.damage}");

            damageInstance.TriggerOnHitAction();
            playerMovement.DashTimer.StopTimer();
            playerMovement.Knockback(damageInstance.knockback);
            animator.SetTrigger(AnimatorTakeDamageTrigger);
            healthComponent.TakeDamage(damageInstance.damage, damageInstance.ignoreInvincibility);
            var cameraEffects = ServiceLocator.Get<CameraEffects>();

            _ = cameraEffects.ShakeCamera(takeDamageScreenShakeIntensity, takeDamageScreenShakeDuration);
            _ = cameraEffects.TimeSlow(takeDamageTimeSlowDuration);
            
            playerMovement.lameParticles.Play();

            PlayBloodSplatter();

            StartCoroutine(ControllerVibration());
        }

        private static IEnumerator ControllerVibration() {
            InputSystem.ResumeHaptics();
            yield return new WaitForSeconds(0.2f);
            InputSystem.PauseHaptics();
        }

        private void PlayBloodSplatter() {
            bloodSplatter.transform.localScale = transform.localScale;
            bloodSplatter.Play();
        }

        #region ***---Death---***
        private IEnumerator Die() {
            var cameraEffects = ServiceLocator.Get<CameraEffects>();
            
            _isDead = true;
            playerCanMove.ChangeVote(_isAliveVotePair, false);
            animator.SetBool(AnimatorIsDeadBool, true);
            FMODManager.RaiseStopSoundEvent(StopSoundType.All);
            FMODManager.RaiseSFXEvent("Player/Player_death");

            animator.ResetTrigger(AnimatorTakeDamageTrigger);
            debugLogger.Log("Setting death trigger");
            animator.SetTrigger(AnimatorDeathTrigger);
            
            cameraEffects.CircleFadeOut(2);

            yield return new WaitUntil(() => animator.GetCurrentAnimationClip().name == "[DVD]Death");
            yield return new WaitForSeconds(animator.GetCurrentAnimationClip().length + 2f);

            playerLives--;

            if (playerLives < 0) {
                debugLogger.Log("Ran out of lives");
                _ = SceneCollectionManager.LoadSceneCollectionAsync(gameOverScene, 
                    () => cameraEffects.CircleFadeIn(0.5f));
            }
            else {
                Respawn();
            
                yield return new WaitForSeconds(0.5f);
                cameraEffects.CircleFadeIn(0.5f);
            }
        }

        private void Respawn() {
            debugLogger.Log("Respawning");
            
            OnRespawn.Invoke();
            
            playerCanMove.ChangeVote(_isAliveVotePair, true);

            _isDead = false;
            
            animator.SetBool(AnimatorIsDeadBool, false);
            
            healthComponent.Health = healthComponent.maxHealth;
            
            LoseGems();
            
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

        private Collider2D InteractCheck() {
            return Physics2D.OverlapBox(transform.position, transform.localScale, 0, whatIsInteractable);
        }

        public void GetKey(GameObject key) {
            if (key.GetComponent<KeyScript>().isCollected) return;
            key.GetComponent<KeyScript>().isCollected = true;
            keyAmount++;
            keyObjects.Add(key);
            if (keyObjects.Count == 1) {
                key.GetComponent<KeyScript>().SetFollowTransform(transform, Vector2.up);
                print("collected key");
            }
            else {
                key.GetComponent<KeyScript>().SetFollowTransform(keyObjects[^2].transform);
                print("collected another key");
            }
        }

        public void UseKey(Transform doorTransform, int usedKeys, float destructionTime) {
            for (var i = 0; i < usedKeys; i++) {
                var keyScript = keyObjects[^1].GetComponent<KeyScript>();
                keyScript.SetFollowTransform(doorTransform);
                keyScript.StartDestructionTimer(destructionTime);
                keyObjects.RemoveAt(keyObjects.Count - 1);
                print("used key: " + i);
            }

            keyAmount = 0;
        }

        private void SetStateBehavioursReferences() {
            var playerAttackBehaviours = animator.GetBehaviours<PlayerAttackBehaviour>();
            foreach (var playerAttackBehaviour in playerAttackBehaviours) {
                playerAttackBehaviour.hitbox = hitbox;
            }
        }

        private IEnumerator ResetPlayerInput() {
            yield return null;
            yield return null;
            playerInput.enabled = false;
            yield return null;
            playerInput.enabled = true;
        }

        public void CheckpointHealing() {
            if (healthComponent.Health == healthComponent.maxTempHealth) {
                healthComponent.TempHealth++;
            }
            else {
                healthComponent.Heal(2);
            }

            if (playerLives == 1) {
                playerLives++;
            }
        }

        #region ---DataBindingMethods---
        public void Bind() {
            if (SaveLoadSystemManager != null) {
                SaveLoadSystemManager.Bind(this, DataInGameData);
            }
            else {
                Debug.LogWarning("No SaveLoadSystemManager found");
            }
        }

        [ContextMenu(nameof(LoadBindData))]
        public void LoadBindData() {
            healthComponent.Health = SaveSystemData.health;
            playerLives = SaveSystemData.lives;
            gems = SaveSystemData.gems;
        }

        [ContextMenu(nameof(SaveBindData))]
        public void SaveBindData() {
            SaveSystemData.ID = ID;
            SaveSystemData.health = healthComponent.Health;
            SaveSystemData.lives = playerLives;
            SaveSystemData.gems = gems;
        }
        #endregion
    }
}