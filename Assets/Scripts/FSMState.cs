using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMState
{
    public List<FSMAction> enterActions = new List<FSMAction>();
    public List<FSMAction> stayActions = new List<FSMAction>();
    public List<FSMAction> exitActions = new List<FSMAction>();

    private Dictionary<FSMTransition, FSMState> links;

    public FSMState()
    {
        links = new Dictionary<FSMTransition, FSMState>();
    }

    public void AddTransition(FSMTransition transition, FSMState target)
    {
        links[transition] = target;
    }

    public FSMTransition VerifyTransitions()
    {
        foreach (FSMTransition t in links.Keys)
            { if (t.myCondition()) return t; }
        return null;
    }

    public FSMState NextState(FSMTransition t)
    {
        return links[t];    
    }

    public void Enter() { foreach (FSMAction a in enterActions) a(); }
    public void Stay() { foreach (FSMAction a in stayActions) a(); }
    public void Exit() { foreach (FSMAction a in exitActions) a(); }

}
