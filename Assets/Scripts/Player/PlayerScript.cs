using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Attributes;
using Consensus;
using ExtensionMethods;
using SaveLoadSystem.Data;
using SceneCollections;
using TetraCreations.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities.DataBinding;
using VContainer;

namespace Player {
    public partial class PlayerScript : MonoBehaviour, IDamageable, IBind<PlayerData>{
        #region ---Fields---
        #region ***---GetOrAddComponents---***
        [GetOrAddComponent, HideInInspector] public PlayerHealth healthComponent;
        [GetOrAddComponent, HideInInspector] public SpriteRenderer spriteRenderer;
        [GetOrAddComponent, HideInInspector] public GravityObjectPlayerMovement playerMovement;
        [GetOrAddComponent, HideInInspector] public PlayerInput playerInput;
        #endregion

        #region ***---SerializedFields---***
        [SerializeField] public MultipleAnimatorControllers animator;
        [SerializeField] private SpriteRenderer legRenderer;
        [SerializeField] private SceneCollectionSO gameOverScene;
        public Transform cameraFollowTransform;
        [SerializeField] private float lookahead = 2;

        [Header("Player")] 
        public int playerLives = 5;
        public int playerLivesStartAmount;
        [SerializeField] private LayerMask whatIsInteractable;
        public bool isAttacking;
        public PlayerHitbox hitbox;
        [SerializeField] private ParticleSystem bloodSplatter;
        [SerializeField] private ParticleSystem checkpointTextEmitter;

        [Header("Taking Damage")] 
        [SerializeField] private float alphaWhenTakingDamage = 0.2f;

        [SerializeField] private float takeDamageScreenShakeIntensity = 0.2f;
        [SerializeField] private float takeDamageScreenShakeDuration = 0.2f;
        [SerializeField] private float takeDamageTimeSlowDuration = 0.5f;

        [Header("Keys")] 
        public int keyAmount;
        public List<GameObject> keyObjects = new();

        [Header("Player Can Move")] [SerializeField, Attributes.Required]
        private AllInFavourConsensusSO playerCanMove;

        [Header("Debug Logger")] [SerializeField, Attributes.Required]
        public DebugLoggerSO debugLogger;
        #endregion

        private bool _isDead;
        private bool _canAttack = true;

        private bool _isLookingUp;
        private bool _isLookingDown;

        private bool PlayerMovementEnabled => playerCanMove.consensus;
        private readonly ScriptVotePair _isAliveVotePair = new();
        public readonly ScriptVotePair MoveVotePair = new();

        public event Action OnRespawn = delegate { };
        
        public static event Action SkipDialogueEvent = delegate { };

        private string _currentLevelMusicID;

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

        private ISaveLoadSystemManager _saveLoadSystemManager;
        private CameraEffects _cameraEffects;
        private IControlSchemeChanger _controlSchemeChanger;
        private IPauseManager _pauseManager;
        
        public IAudioManager AudioManager;

        #region ***---SoundID---***
        private const string PlayerSlash_SoundID = "Player_Slash";
        private const string PlayerDeath_SoundD = "Player_Death";
        private const string PlayerHurtSoundID = "Player_Hurt";
        
        public const string PlayerDash_SoundID = "Player_Dash";
        public const string PlayerStep_SoundID = "Player_Step";
        public const string PlayerRespawn_SoundID = "Player_Respawn";
        public const string PlayerGunJump_SoundID = "Player_GunJump";
        #endregion
        #endregion

        #region ---Initialization---
        [Inject]
        public void Construct(IObjectResolver container) {
            _saveLoadSystemManager = container.Resolve<ISaveLoadSystemManager>();
            _cameraEffects = container.Resolve<CameraEffects>();
            _controlSchemeChanger = container.Resolve<IControlSchemeChanger>();
            _pauseManager = container.Resolve<IPauseManager>();
            AudioManager = container.Resolve<IAudioManager>();
        }
        
        private void Awake() {
            ServiceLocator.Register(this);
            debugLogger.messageOrigin = this;
            
            Bind();
        }
        #endregion

        #region ---EventFunctions---
        private void Start() {
            playerLivesStartAmount = playerLives;
            
            playerCanMove.ChangeVote(MoveVotePair, true);
            
            healthComponent.OnDeath += () => StartCoroutine(Die());

            healthComponent.InvulnerabilityTimer.OnBegin +=
                () => {
                    spriteRenderer.color = new Color(1, 1, 1, alphaWhenTakingDamage);
                    legRenderer.color = new Color(1, 1, 1, alphaWhenTakingDamage);
                };
            healthComponent.InvulnerabilityTimer.OnComplete += () => {
                spriteRenderer.color = new Color(1, 1, 1, 1);
                legRenderer.color = new Color(1, 1, 1, 1);
            };

            SetStateBehavioursReferences();

            Gamepad.current?.SetMotorSpeeds(0.123f, 0.234f);
            InputSystem.PauseHaptics();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            playerInput.actions["LookUp"].performed += _ => _isLookingUp = true;
            playerInput.actions["LookUp"].canceled += _ => _isLookingUp = false;
            playerInput.actions["LookDown"].performed += _ => _isLookingDown = true;
            playerInput.actions["LookDown"].canceled += _ => _isLookingDown = false;

            _controlSchemeChanger.OnControlSchemeChanged +=
                data => playerInput.SwitchCurrentControlScheme(data.Name, data.Devices);
            playerInput.SwitchCurrentControlScheme(_controlSchemeChanger.CurrentControlSchemeName,
                _controlSchemeChanger.CurrentControlSchemeData.Devices);
            
            _pauseManager.OnPause += OnPaused;
            _pauseManager.OnResume += OnResumed;
        }

        private void OnDestroy() {
            ServiceLocator.Unregister<PlayerScript>();
            playerCanMove.Clear();
            InputSystem.PauseHaptics();
            _pauseManager.OnPause -= OnPaused;
            _pauseManager.OnResume -= OnResumed;
        }

        private void Update() {
            var offset = 0f;
            if (_isLookingUp && Mathf.Abs(playerMovement.inputVector.x) <= 0.2) offset += lookahead;
            if (_isLookingDown && Mathf.Abs(playerMovement.inputVector.x) <= 0.2) offset -= lookahead;
            cameraFollowTransform.position = transform.position.WithOffset(y: offset);

            if (OverlapLadder()) return;
            playerMovement.PlayerLadderLogic.LadderTop = null;
            playerMovement.PlayerLadderLogic.LadderMiddle = null;
            playerMovement.PlayerLadderLogic.LadderBottom = null;
        }
        
        private bool OverlapLadder() {
            var hitCount = Physics2D.OverlapCircleAll(transform.position, 2);
            return hitCount.Any(x => x.transform.CompareTag("Ladder"));
        }


        #region ***---InputActions---***
        public void OnSkipDialogue(InputValue inputValue) {
            if (!inputValue.isPressed) return;
            SkipDialogueEvent.Invoke();
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
                if (playerMovement.inputVector.x == 0) {
                    transform.localScale = playerMovement.isFacingRight ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
                }
                else {
                    var u = playerMovement.inputVector.x >= 0 ? 1 : -1;
                    transform.localScale = new Vector3(u, 1, 1);
                }
            }
            
            playerInput.actions["LookUp"].Reset();
            playerInput.actions["LookDown"].Reset();

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

            AudioManager.PlayOneShot(PlayerSlash_SoundID);
            _canAttack = false;
            isAttacking = true;
            animator.SetBool(AnimatorIsAttackingBool, true);
        }

        private void DuckAttack() {
            if (!_canAttack) return;
            debugLogger.Log("DuckAttack");

            animator.SetTrigger(AnimatorDuckAttackTrigger);
            AudioManager.PlayOneShot(PlayerSlash_SoundID);
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
            if (_isDead || _pauseManager.Paused) return;

            debugLogger.Log($"Taking Damage: {damageInstance.damage}");

            damageInstance.TriggerOnHitAction();
            
            AudioManager.PlayOneShot(PlayerHurtSoundID);
            
            playerMovement.DashTimer.StopTimer();
            playerMovement.Knockback(damageInstance.knockback);
            animator.SetTrigger(AnimatorTakeDamageTrigger);
            healthComponent.TakeDamage(damageInstance.damage, damageInstance.ignoreInvincibility);

            _ = _cameraEffects.ShakeCamera(takeDamageScreenShakeIntensity, takeDamageScreenShakeDuration);
            _ = _cameraEffects.TimeSlow(takeDamageTimeSlowDuration);

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
            _isDead = true;
            playerCanMove.ChangeVote(_isAliveVotePair, false);
            animator.SetBool(AnimatorIsDeadBool, true);
            
            _currentLevelMusicID = AudioManager.CurrentMusicID;
            AudioManager.StopAllMusic();
            AudioManager.PlayOneShot(PlayerDeath_SoundD);

            animator.ResetTrigger(AnimatorTakeDamageTrigger);
            debugLogger.Log("Setting death trigger");
            animator.SetTrigger(AnimatorDeathTrigger);

            _cameraEffects.CircleFadeOut(2);

            yield return new WaitUntil(() => animator.GetCurrentAnimationClip().name == "[DVD]Death");
            yield return new WaitForSeconds(animator.GetCurrentAnimationClip().length + 2f);

            playerLives--;

            if (playerLives < 0) {
                debugLogger.Log("Ran out of lives");
                _ = SceneCollectionManager.LoadSceneCollectionAsync(gameOverScene,
                    () => _cameraEffects.CircleFadeIn(0.5f));
            }
            else {
                StartCoroutine(Respawn());
                _cameraEffects.CircleFadeIn(0.5f);
                AudioManager.PlayMusic(_currentLevelMusicID);
            }
        }

        private IEnumerator Respawn() {
            debugLogger.Log("Respawning");

            OnRespawn.Invoke();

            playerCanMove.ChangeVote(_isAliveVotePair, true);

            _isDead = false;

            animator.SetBool(AnimatorIsDeadBool, false);

            healthComponent.Health = healthComponent.maxHealth;

            LoseGems();

            animator.ResetTrigger(AnimatorDeathTrigger);

            _saveLoadSystemManager.CheckpointLoad();
            yield return null;
            _saveLoadSystemManager.CheckpointLoad();
            
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
                keyScript.followSpeed *= 2;
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

        public void CheckpointHealing() {
            checkpointTextEmitter.Play();
            
            if (healthComponent.Health == healthComponent.maxTempHealth) {
                healthComponent.TempHealth += 1;
            }
            else {
                healthComponent.Heal(3);
            }

            if (playerLives == 1) {
                playerLives++;
            }
        }

        public void PlayStepSound() {
            AudioManager.PlayOneShot(PlayerStep_SoundID);
        }
        
        #region ---Pause---
        private void OnPaused() {
            playerInput.actions.actionMaps[0].Disable();
        }
        
        private void OnResumed() {
            playerInput.actions.actionMaps[0].Enable();
        }
        #endregion
    }
}