using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Animancer;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using VContainer;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class Altar : MonoBehaviour 
{
     [SerializeField] private float maxSuckRange = 25f;
     [SerializeField] private float minSuckSpeed = 5f;
     [SerializeField] private float maxSuckSpeed = 20f;
     [SerializeField] private float executeRange = 1f;
     [SerializeField] private AnimationClip idle;
     [SerializeField] private AnimationClip suck;
     [SerializeField] private AnimationClip caughtGhost;
     
     private bool _sucking;
     private Coroutine _animationCoroutine;
     private AnimancerComponent _animancerComponent;
     private Collider2D _collider;
     private IAudioManager _audioManager;
     private EventInstance _suckingEvent;

     [Inject]
     public void Construct(IAudioManager audioManager) {
          _audioManager = audioManager;
     }
     
     private void Awake()
     {
          _suckingEvent = RuntimeManager.CreateInstance("event:/Sfx/Misc/Vacuum");
          _collider = GetComponent<Collider2D>();
          _animancerComponent = GetComponent<AnimancerComponent>();
          _animancerComponent.Play(idle);
     }

     private void Update()
     {
          _sucking = false;
          
          // Check if a gameobject with ghost tag is within range
          ContactFilter2D contactFilter = new ContactFilter2D();
          contactFilter.SetLayerMask(LayerMask.GetMask("Enemy"));
          contactFilter.useTriggers = true;
          var ghosts = new List<Collider2D>();
          
          Physics2D.OverlapCircle(_collider.bounds.center, maxSuckRange, contactFilter, ghosts);
          
          // Filter out non-ghosts using LINQ
          var filteredGhosts = ghosts.Where(ghost => ghost.CompareTag("Ghost")).ToList();
          
          foreach (var ghost in filteredGhosts)
          {
               if (!ghost.TryGetComponent(out Ghost ghostComponent)) continue;
               
               if (Vector2.Distance(ghost.transform.position, _collider.bounds.center) > maxSuckRange) continue;
               
               if (Vector2.Distance(_collider.bounds.center, ghost.transform.position) < executeRange)
               {
                    ghostComponent.Execute();
                    
                    // check if this ghost is last in the list
                    if (!_sucking && filteredGhosts[^1] == ghost)
                    {
                         // Play the caught ghost animation
                         _animationCoroutine = StartCoroutine(PlayCaughtGhostAnimation());
                    }
                    
                    continue;
               }
               
               // Calculate the distance between the altar and the ghost
               float distance = Vector2.Distance(_collider.bounds.center, ghost.transform.position);

               // Calculate the speed of the ghost based on the distance
               float speed = Mathf.Lerp(maxSuckSpeed, minSuckSpeed, distance / maxSuckRange);

               // Set the direction of the ghost towards the altar
               Vector2 direction = (_collider.bounds.center - ghost.transform.position).normalized * speed;

               // Move the ghost towards the altar
               ghostComponent.sucked = true;
               ghostComponent.suckVelocity = direction;
               _sucking = true;
               PlaySuckAnimation();
          }
     }
     
     private IEnumerator PlayCaughtGhostAnimation() {
          _suckingEvent.stop(STOP_MODE.ALLOWFADEOUT);
          _audioManager.PlayOneShot("Ghost_Eat");
          _animancerComponent.Play(caughtGhost);

          yield return new WaitForSeconds(1f);
          
          _animancerComponent.Play(idle);
     }

     private void PlaySuckAnimation() {
          if (_animancerComponent.IsPlaying(suck)) return;
          _suckingEvent.start();
          
          if (_animationCoroutine != null)
          {
               StopCoroutine(_animationCoroutine);
               _animationCoroutine = null;
          }
          
          _animancerComponent.Play(suck);
     }
}
