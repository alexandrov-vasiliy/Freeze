using UnityEngine;

public class WarmZone : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent<Player>(out _))
            other.GetComponent<Player>().campFireColliderInRange = true;
        
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<Player>(out _))
            other.GetComponent<Player>().campFireColliderInRange = false;
        
    }
}
