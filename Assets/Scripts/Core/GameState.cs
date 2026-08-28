using UnityEngine;

public class GameState : MonoBehaviour
{
    public bool BlackFeatherCollected { get; private set; }

    public void CollectBlackFeather()
    {
        BlackFeatherCollected = true;
    }
}
