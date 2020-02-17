using System.Collections.Generic;

public delegate bool FSMCondition();

public delegate void FSMAction();

public class FSMTransition
{
    public FSMCondition myCondition;

    private List<FSMAction> myActions = new List<FSMAction>();

    public FSMTransition(FSMCondition condition, FSMAction[] actions = null)
    {
        myCondition = condition;
        if (actions != null) myActions.AddRange(actions);
    }

    public void Fire()
    {
        if (myActions != null) foreach (FSMAction action in myActions) action();
    }
}
