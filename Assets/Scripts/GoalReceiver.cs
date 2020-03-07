using UnityEngine;

public class GoalReceiver : MonoBehaviour
{
    public Collider ball;
    public GameManager gm;
    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other != ball) return;
        gm.Restart();
    }
}
