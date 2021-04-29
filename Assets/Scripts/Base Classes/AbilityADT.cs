using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This interface is in every ability, it allows for them to be keybound to the three different
// keys and properly integrate the three input methods
public interface AbilityADT
{
    void InputGetDown();

    void InputGet();

    void InputGetUp();

    void SetScriptStatus(bool status);
}
