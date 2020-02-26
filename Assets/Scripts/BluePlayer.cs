using System;
using System.Collections;
using UnityEngine;

public class BluePlayer : MonoBehaviour
{
    public BelongsTo ball = null;
    public Rigidbody BallBody = null;
    public int DangerRange = 5;
    private Rigidbody rb;
    private Vector3 BluePos;
    private Vector3 RedPos;
    private readonly System.Random Rand = new System.Random();
    private int Skill;
    private FSM fsm;
    public float reactionTime = 0.1f;
    private bool GoingOnward = false;
    private GameObject[] allies = new GameObject[5];
    private GameObject[] enemies = new GameObject[5];

    //actual conditions
    private int EnemiesInRange()
    {
        int count = 0;
        foreach (GameObject go in enemies)
        {
            if ((go.transform.position - gameObject.transform.position).magnitude < DangerRange) count += 1;
            // && go.transform.position.x < gameObject.transform.position.x
        }
        return count;
    }

    private bool BallInSight()
    {
        if (ball.player != null)
        {
            bool ray = Physics.Raycast(gameObject.transform.position, ball.player.transform.position - gameObject.transform.position, out RaycastHit hit, Mathf.Infinity);
            return (ray && hit.transform == ball.player.transform);
        }
        else
        {
            bool ray = Physics.Raycast(gameObject.transform.position, BallBody.transform.position - gameObject.transform.position, out RaycastHit hit, Mathf.Infinity);
            Debug.Log("ray = " + ray + " hit = " + hit.collider.gameObject.name);
            return (ray && hit.transform == BallBody.transform);
        }
    }

    private bool GoalInSight()
    {
        bool ray = Physics.Raycast(gameObject.transform.position, RedPos - gameObject.transform.position, out RaycastHit hit, Mathf.Infinity);
        return (ray && hit.transform.position == RedPos & hit.distance < 40);
    }

    private bool OneEnemyAround()
    {
        return EnemiesInRange() == 1;
    }

    private bool MoreEnemiesAround()
    {
        return this.EnemiesInRange() > 1;
    }

    private bool BallControl() { return ball.player == gameObject; }
    private bool AlliedBall()
    {
        return !EnemyBall();
    }
    private bool EnemyBall() { return ball.player == null || !ball.player.CompareTag("BluePlayer"); }
    private bool BallNotInSight() { return !BallInSight(); }
    private bool BallToMate() { return AlliedBall() & !BallControl(); }
    private bool CanIPass() { return LookForAlly() != null; }

    //actual actions
    private void BringBallAhead()
    {
        //Debug.Log("BringBallAhead");
        if (gameObject.transform.position.z < 50 || gameObject.transform.position.z > -50)
        {
            ApplyForceToReachVelocity(rb, new Vector3(-8, 0, 0), 20); ApplyForceToReachVelocity(BallBody, new Vector3(-8, 0, 0), 20);
        }
        else
        {
            ApplyForceToReachVelocity(rb, RedPos - gameObject.transform.position, 20); ApplyForceToReachVelocity(BallBody, RedPos - gameObject.transform.position, 20);
        }
        if ((gameObject.transform.position - BallBody.transform.position).magnitude > DangerRange) ball.SetPlayer(null);
    }
    private void RetreatToBall() { ApplyForceToReachVelocity(rb, new Vector3(10, 0, 0), 20); }
    private void ChaseBall()
    {
        ApplyForceToReachVelocity(rb, (BallBody.position - gameObject.transform.position).normalized * 10, 30);
        CatchBall();
    }
    private void SpeedRun() { if ((gameObject.transform.position - BallBody.transform.position).magnitude <= 2*DangerRange)  rb.AddForce((BluePos - gameObject.transform.position).normalized*30); }
    private void ShootBall()
    {
        if (!GoalInSight()) return;
        //Debug.Log("Provo a tirare");
        BallBody.AddForce((RedPos - gameObject.transform.position).normalized * 2500);
        ball.SetPlayer(null);
    }

    private GameObject LookForAlly()
    {
        GameObject target = null;
        foreach (GameObject go in allies)
        {
            float lowestX = 10000;
            if (go.transform.position != gameObject.transform.position)
            {
                bool ray = Physics.Raycast(gameObject.transform.position, go.transform.position - gameObject.transform.position, out RaycastHit hit, Mathf.Infinity, 1);
                if (go.transform == hit.transform & hit.transform.position.x < lowestX) { target = go; }
            }
        }
        return target;
    }

    //La riga ball.setplayer(null) era fuori dall'if, e tutto andava ma non come dovuto
    private void PassTheBall()
    {
        if (MoreEnemiesAround() & CanIPass())
        {
            GameObject receiver = LookForAlly();
            Debug.Log("Passo a " + receiver.ToString());
            BallBody.AddForce((receiver.transform.position - gameObject.transform.position).normalized * 2500);
            ball.SetPlayer(null);
        }
    }

    private void ReachPosition()
    {
        if (gameObject.transform.position.x >= -60)
        {
            float meanZ = 0;
            foreach (GameObject go in allies)
            {
                meanZ += go.transform.position.z;
            }
            meanZ /= 5;
            float sign = (gameObject.transform.position.z - meanZ) / Math.Abs(gameObject.transform.position.z - meanZ);
            ApplyForceToReachVelocity(rb, (new Vector3(-60, 0, -50 * sign) - gameObject.transform.position).normalized * 10, 20);
        }
        else
        {
            Strafing();
        }
    }

    public IEnumerator Play()
    {
        while (true)
        {
            fsm.Update();
            yield return new WaitForSeconds(reactionTime);
        }
    }

    private void TryDribble()
    {
        if (!OneEnemyAround()) return;
        bool success = Rand.Next(5) <= Skill;

        Debug.Log("Dribbling: " + success);

        Rigidbody EnemyBody = null;
        GameObject Enemy = null;
        foreach (GameObject go in enemies)
        {
            if ((go.transform.position - gameObject.transform.position).magnitude < DangerRange)
            {
                Enemy = go;
                EnemyBody = go.GetComponent<Rigidbody>();
                break;
            }
        }

        //the player dashes in any case
        rb.AddForce(-50, 0, 0);

        if (success)
        {
            //get rid of the enemy in a simple way
            EnemyBody.AddForce(1000, 0, -1000);
            BallBody.AddForce(-50, 0, 0);
        }
        else
        {
            Debug.Log(Enemy.ToString());
            ball.SetPlayer(Enemy);
        }
    }

    private void Strafing()
    {
        if (!GoingOnward)
        {
            ApplyForceToReachVelocity(rb, new Vector3(-10, 0, 0), 20);
            if (gameObject.transform.position.x >= -60) { GoingOnward = true; }
        }
        else
        {
            ApplyForceToReachVelocity(rb, new Vector3(10, 0, 0), 20);
            if (gameObject.transform.position.x <= -80) { GoingOnward = false; }
        }
    }

    private void HardDribble()
    {
        if (MoreEnemiesAround() & !CanIPass())
        {
            //Debug.Log("Blue HardDribble");
            Skill -= 1;
            TryDribble();
            Skill += 1;
        }
    }

    private void CatchBall()
    {
        if ((BallBody.transform.position - gameObject.transform.position).magnitude <= 1.5 && ball.player == null) { ball.SetPlayer(gameObject); }
    }
    private void PrintAdvance()
    {
        Debug.Log(gameObject.ToString() + "Advance");
    }
    private void PrintSupport()
    {
        Debug.Log(gameObject.ToString() + "Support - BallToMate = " + BallToMate());
    }
    private void PrintBacking()
    {
        Debug.Log(gameObject.ToString() + "Backing");
    }
    private void PrintChase()
    {
        Debug.Log(gameObject.ToString() + "Chase");
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (!BallBody) return;

        RedPos = GameObject.Find("RedGoal").GetComponent<Rigidbody>().position;
        BluePos = GameObject.Find("BlueGoal").GetComponent<Rigidbody>().position;
        allies = GameObject.FindGameObjectsWithTag("BluePlayer");
        enemies = GameObject.FindGameObjectsWithTag("RedPlayer");
        //each player can be average or skilled, setting dribble chance at 50% (meh) or 67% (literally Messi)
        Skill = Rand.Next(1) + 3;

        FSMState Advance = new FSMState();
        Advance.enterActions.Add(BringBallAhead);
        Advance.stayActions.Add(BringBallAhead);
        Advance.stayActions.Add(TryDribble);
        Advance.stayActions.Add(HardDribble);
        Advance.stayActions.Add(ShootBall);
        Advance.stayActions.Add(PassTheBall);

        FSMState SupportAdv = new FSMState();
        SupportAdv.enterActions.Add(ReachPosition);
        SupportAdv.stayActions.Add(ReachPosition);

        FSMState Backing = new FSMState();
        Backing.enterActions.Add(RetreatToBall);
        Backing.stayActions.Add(RetreatToBall);

        FSMState Chase = new FSMState();
        Chase.enterActions.Add(ChaseBall);
        Chase.stayActions.Add(ChaseBall);
        Chase.stayActions.Add(SpeedRun);

        //Advance.enterActions.Add(PrintAdvance);
        //SupportAdv.enterActions.Add(PrintSupport);
        //Backing.enterActions.Add(PrintBacking);
        //Chase.enterActions.Add(PrintChase);

        FSMTransition t1 = new FSMTransition(BallControl);
        FSMTransition t2 = new FSMTransition(BallToMate);
        FSMTransition t3 = new FSMTransition(EnemyBall);
        FSMTransition t4 = new FSMTransition(BallInSight);
        FSMTransition t5 = new FSMTransition(BallNotInSight);

        Advance.AddTransition(t2, SupportAdv);
        Advance.AddTransition(t3, Chase);
        SupportAdv.AddTransition(t1, Advance);
        SupportAdv.AddTransition(t3, Backing);
        Chase.AddTransition(t1, Advance);
        Chase.AddTransition(t5, Backing);
        Chase.AddTransition(t2, SupportAdv);
        Backing.AddTransition(t4, Chase);
        Backing.AddTransition(t2, SupportAdv);

        FSMState Starting = new FSMState();
        Starting.AddTransition(t1, Advance);
        Starting.AddTransition(t2, SupportAdv);
        Starting.AddTransition(t3, Backing);

        fsm = new FSM(Starting);
        StartCoroutine(Play());

    }

    //applies the force necessary to reach the desired velocity
    private static void ApplyForceToReachVelocity(Rigidbody rigidbody, Vector3 velocity, float force = 1, ForceMode mode = ForceMode.Force)
    {
        if (force == 0 || velocity.magnitude == 0)
            return;

        velocity += velocity.normalized * 0.2f * rigidbody.drag;

        force = Mathf.Clamp(force, -rigidbody.mass / Time.fixedDeltaTime, rigidbody.mass / Time.fixedDeltaTime);

        if (rigidbody.velocity.magnitude == 0)
        {
            rigidbody.AddForce(velocity * force, mode);
        }
        else
        {
            var velocityProjectedToTarget = (velocity.normalized * Vector3.Dot(velocity, rigidbody.velocity) / velocity.magnitude);
            rigidbody.AddForce((velocity - velocityProjectedToTarget) * force, mode);
        }
    }
}
