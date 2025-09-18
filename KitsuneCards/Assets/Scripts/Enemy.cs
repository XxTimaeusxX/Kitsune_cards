using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
 public CardDeckManager deckManager;


    public void StartEnemyTurn()
    {
        deckManager.StartCoroutine(EnemyTurnRoutine());
    }
   private IEnumerator EnemyTurnRoutine()
    {
        // Enemy turn logic here
        yield return new WaitForSeconds(1f); // Example wait time
        deckManager.OnEnemyEndTurn(); ;
    }
}
