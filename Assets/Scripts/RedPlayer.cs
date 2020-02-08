using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedPlayer : MonoBehaviour
{
    public BelongsTo ball = null;
    public Rigidbody BallBody = null;
    public int DangerRange = 10;
    private Rigidbody rb;
    private Vector3 BluePos;
    private Vector3 RedPos;
    private System.Random Rand = new System.Random();
    private int Skill;
    private FSM fsm;

    //actual conditions
    private int EnemiesInRange()
    {
        int enemies = 0;
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("BluePlayer"))
        {
            if ((go.transform.position - transform.position).magnitude < DangerRange) enemies += 1;
        }
        return enemies;
    }

    private bool BallInSight()
    {
        RaycastHit hit;
        bool ray = Physics.Raycast(transform.position, BallBody.position - transform.position, out hit, 1);
        return (hit.transform == BallBody.transform);
    }

    private bool GoalInSight()
    {
        RaycastHit hit;
        bool ray = Physics.Raycast(transform.position, BluePos - transform.position, out hit, 1);
        return (hit.transform.position == BluePos & hit.distance < 40);
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
    private bool AlliedBall() { return ball.player.tag == gameObject.tag; }
    private bool EnemyBall() { return !AlliedBall(); }
    private bool BallNotInSight() { return !BallInSight(); }
    private bool BallToMate() { return AlliedBall() & !BallControl(); }
    private bool CanIPass() { return LookForAlly() != null; }

    //actual actions
    private void BringBallAhead() { ApplyForceToReachVelocity(rb, new Vector3(8, 0, 0), 20); ApplyForceToReachVelocity(BallBody, new Vector3(8, 0, 0), 20); }
    private void RetreatToBall() { ApplyForceToReachVelocity(rb, new Vector3(-10, 0, 0), 20); }
    private void ChaseBall() { ApplyForceToReachVelocity(rb, (BallBody.position - transform.position).normalized * 10, 30); }
    private void SpeedRun() { if ((transform.position - BallBody.transform.position).magnitude <= 2 * DangerRange) rb.AddForce((RedPos - transform.position).normalized * 30); }
    private void ShootBall()
    {
        if (!GoalInSight()) return;
        BallBody.AddForce((BluePos - transform.position).normalized * 2500);
        ball.player = null;
    }

    private GameObject LookForAlly()
    {
        GameObject target = null;
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("RedPlayer"))
        {
            RaycastHit hit;
            float highestX = 100;
            if (go.transform.position != transform.position)
            {
                bool ray = Physics.Raycast(transform.position, go.transform.position - transform.position, out hit, 1);
                if (go.transform == hit.transform & hit.transform.position.x > highestX) { target = go; }
            }
        }
        return target;
    }

    private void PassTheBall()
    {
        if (MoreEnemiesAround() & CanIPass())
        {
            GameObject receiver = LookForAlly();
            BallBody.AddForce((receiver.transform.position - transform.position).normalized * 2500);
        }
        ball.player = null;
    }

    private void ReachPosition()
    {
        if (transform.position.x >= -60)
        {
            float sign = (transform.position.z - ball.player.transform.position.z) / Math.Abs(transform.position.z - ball.player.transform.position.z);
            ApplyForceToReachVelocity(rb, (new Vector3(60, 0, -50 * sign) - transform.position).normalized * 10, 20);
        }
        else
        {
            Strafing();
        }
    }

    private void TryDribble()
    {
        if (!OneEnemyAround()) return;
        bool success = Rand.Next(5) <= Skill;
        Rigidbody EnemyBody = null;
        GameObject Enemy = null;
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("BluePlayer"))
        {
            if ((go.transform.position - transform.position).magnitude < DangerRange)
            {
                Enemy = go;
                EnemyBody = go.GetComponent<Rigidbody>();
                break;
            }
        }

        //the player dashes in any case
        rb.AddForce(50, 0, 0);

        if (success)
        {
            //get rid of the enemy in a simple way
            EnemyBody.AddForce(0, 0, 1000);
            BallBody.AddForce(50, 0, 0);
        }
        else
        {
            //loses ball control
            ball.SetPlayer(Enemy);
        }
    }

    private void Strafing()
    {
        bool GoingOnward = false;
        if (!GoingOnward)
        {
            ApplyForceToReachVelocity(rb, new Vector3(10, 0, 0), 20);
            if (transform.position.x <= 60) { GoingOnward = true; }
        }
        else
        {
            ApplyForceToReachVelocity(rb, new Vector3(-10, 0, 0), 20);
            if (transform.position.x >= 80) { GoingOnward = false; }
        }
    }

    private void HardDribble()
    {
        if (MoreEnemiesAround() & !CanIPass())
        {
            Skill = Skill - 1;
            TryDribble();
            Skill = Skill + 1;
        }
    }

    private void CatchBall()
    {
        if ((BallBody.transform.position - transform.position).magnitude <= 1) { ball.SetPlayer(gameObject); }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (!BallBody) return;

        RedPos = GameObject.Find("RedGoal").GetComponent<Rigidbody>().position;
        BluePos = GameObject.Find("BlueGoal").GetComponent<Rigidbody>().position;
        //each player can be average or skilled, setting dribble chance at 50% (meh) or 67% (literally Messi)
        Skill = Rand.Next(1) + 3;

        //formal actions
        FSMAction AdvanceBall = BringBallAhead;
        FSMAction BackToDefense = RetreatToBall;
        FSMAction ToBall = ChaseBall;
        FSMAction RunToDef = SpeedRun;
        FSMAction TryShooting = ShootBall;
        FSMAction Dribbling = TryDribble;
        FSMAction SupportAdvance = ReachPosition;
        FSMAction Passage = PassTheBall;
        //FSMAction BackAndForth = Strafing;
        FSMAction GetOutOfThere = HardDribble;

        FSMState Advance = new FSMState();
        Advance.enterActions.Add(AdvanceBall);
        Advance.stayActions.Add(AdvanceBall);
        Advance.stayActions.Add(Dribbling);
        Advance.stayActions.Add(GetOutOfThere);
        Advance.stayActions.Add(TryShooting);
        Advance.stayActions.Add(Passage);

        //FSMState Dribble = new FSMState();
        //Dribble.enterActions.Add(Dribbling);

        //FSMState PassBall = new FSMState();
        //PassBall.enterActions.Add(Passage);

        //FSMState InferiorityDribble = new FSMState();
        //InferiorityDribble.enterActions.Add(GetOutOfThere);

        //FSMState WaitForBall = new FSMState();
        //WaitForBall.stayActions.Add(BackAndForth);

        FSMState SupportAdv = new FSMState();
        SupportAdv.enterActions.Add(SupportAdvance);
        SupportAdv.enterActions.Add(SupportAdvance);

        FSMState Backing = new FSMState();
        Backing.enterActions.Add(BackToDefense);
        Backing.stayActions.Add(BackToDefense);

        FSMState Chase = new FSMState();
        Chase.enterActions.Add(ToBall);
        Chase.stayActions.Add(ToBall);
        Chase.stayActions.Add(RunToDef);

        //FSMState Dash = new FSMState();
        //Dash.enterActions.Add(RunToDef);

        //FSMState Shoot = new FSMState();
        //Shoot.enterActions.Add(TryShooting);

        //formal conditions
        FSMCondition bc = BallControl;
        FSMCondition ab = AlliedBall;
        //FSMCondition oea = OneEnemyAround;
        //FSMCondition mea = MoreEnemiesAround;
        FSMCondition bis = BallInSight;
        FSMCondition notbis = BallNotInSight;
        FSMCondition btm = BallToMate;
        FSMCondition eb = EnemyBall;
        //FSMCondition gis = GoalInSight;

        FSMTransition t1 = new FSMTransition(bc);
        FSMTransition t2 = new FSMTransition(btm);
        FSMTransition t3 = new FSMTransition(eb);
        FSMTransition t4 = new FSMTransition(bis);
        FSMTransition t5 = new FSMTransition(notbis);

        Advance.AddTransition(t2, SupportAdv);
        Advance.AddTransition(t3, Chase);
        //WaitForBall.AddTransition(eb, Backing);
        //WaitForBall.AddTransition(bc, Advance);
        SupportAdv.AddTransition(t1, Advance);
        SupportAdv.AddTransition(t3, Backing);
        Chase.AddTransition(t1, Advance);
        Chase.AddTransition(t5, Backing);
        Chase.AddTransition(t2, SupportAdv);
        Backing.AddTransition(t4, Chase);
        Backing.AddTransition(t2, SupportAdv);

        FSMState StartingState = null;
        if (BallControl()) StartingState = Advance;
        if (AlliedBall() & !BallControl()) StartingState = SupportAdv;
        if (EnemyBall()) StartingState = Chase;
        fsm = new FSM(StartingState);

    }

    //void Update() { }

    //applies the force necessary to reach the desired velocity
    private static void ApplyForceToReachVelocity(Rigidbody rigidbody, Vector3 velocity, float force = 1, ForceMode mode = ForceMode.Force)
    {
        if (force == 0 || velocity.magnitude == 0)
            return;

        velocity = velocity + velocity.normalized * 0.2f * rigidbody.drag;

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
