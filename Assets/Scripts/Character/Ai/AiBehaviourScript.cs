using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character;

public abstract class AiBehaviourScript : ScriptableObject
{
    public virtual void InitBehaviourController(AiBehaviourController stateController) { }
}
