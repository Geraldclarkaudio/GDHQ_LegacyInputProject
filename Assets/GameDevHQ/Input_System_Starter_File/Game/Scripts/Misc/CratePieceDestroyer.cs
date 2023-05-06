using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CratePieceDestroyer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Piece"))
        {
            Destroy(other.gameObject);
        }
    }
}
