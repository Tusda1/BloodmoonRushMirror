using System;
using UnityEngine;

public class Altar : MonoBehaviour 
{
     [SerializeField] private float maxSuckRange = 25f;
     [SerializeField] private float minSuckSpeed = 5f;
     [SerializeField] private float maxSuckSpeed = 20f;
     [SerializeField] private float executeRange = 1f;

     private void Update()
     {
          // Check if a gameobject with ghost tag is within range
          Collider2D[] ghosts = Physics2D.OverlapCircleAll(transform.position, maxSuckRange, LayerMask.GetMask("Enemy"));
          foreach (var ghost in ghosts)
          {
               if (!ghost || !ghost.TryGetComponent(out Ghost ghostComponent)) return;

               if (Vector2.Distance(transform.position, ghost.transform.position) < executeRange)
               {
                    ghost.gameObject.SetActive(false);
                    continue;
               }
               
               // Calculate the distance between the altar and the ghost
               float distance = Vector2.Distance(transform.position, ghost.transform.position);

               // Calculate the speed of the ghost based on the distance
               float speed = Mathf.Lerp(maxSuckSpeed, minSuckSpeed, distance / maxSuckRange);

               // Set the direction of the ghost towards the altar
               Vector2 direction = (transform.position - ghost.transform.position).normalized * speed;

               // Move the ghost towards the altar
               ghostComponent.sucked = true;
               ghostComponent.suckVelocity = direction;
          }
     }
}
