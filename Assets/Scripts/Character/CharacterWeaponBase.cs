using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character;

public abstract class CharacterWeaponBase : ScriptableObject
{
    public virtual void InitCharacterStateController(CharacterStateController stateController) { }
}
