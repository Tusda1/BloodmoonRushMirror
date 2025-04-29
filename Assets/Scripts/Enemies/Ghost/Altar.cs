using System;
using System.Collections.Generic;
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
          ContactFilter2D contactFilter = new ContactFilter2D();
          contactFilter.SetLayerMask(LayerMask.GetMask("Enemy"));
          contactFilter.useTriggers = true;
          var ghosts = new List<Collider2D>();
          
          Physics2D.OverlapCircle(transform.position, maxSuckRange, contactFilter, ghosts);
          foreach (var ghost in ghosts)
          {
               if (!ghost.TryGetComponent(out Ghost ghostComponent)) continue;
               
               if (Vector2.Distance(ghost.transform.position, transform.position) > maxSuckRange) continue;
               
               if (Vector2.Distance(transform.position, ghost.transform.position) < executeRange)
               {
                    ghostComponent.Execute();
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
