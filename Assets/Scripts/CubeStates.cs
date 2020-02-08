using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeStates : MonoBehaviour
{
    public Material material;
    private FSM fsm;

    private void GoRed()
    {
        material.SetColor("_Color", Color.red);
    }

    private void GoBlue()
    {
        material.SetColor("_Color", Color.blue);
    }

    private bool BlueTime()
    { 
        return (Time.fixedTime % 2) == 1;
    }
    
    private bool RedTime()
    {
        return (Time.fixedTime % 2) == 0;
    }

    void Start()
    {
        FSMCondition bt = BlueTime;
        FSMCondition rt = RedTime;

        FSMTransition t1 = new FSMTransition(bt);
        FSMTransition t2 = new FSMTransition(rt);

        FSMState Red = new FSMState();
        Red.enterActions.Add(GoRed);

        FSMState Blue = new FSMState();
        Blue.enterActions.Add(GoBlue);

        Blue.AddTransition(t2, Red);
        Red.AddTransition(t1, Blue);

        fsm = new FSM(Red);

    }

    void Update()
    {
        
    }
}
