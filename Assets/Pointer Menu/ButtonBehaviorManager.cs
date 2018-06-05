using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ButtonBehaviorManager : MonoBehaviour
{
    [Serializable]
    public struct ButtonBehaviorEntry
    {
        public string Id;
        public ManagedButtonBehavior Behavior;
    }

    [SerializeField]
    private ButtonBehaviorEntry[] _ButtonBehaviors;
    public IDictionary<string, ManagedButtonBehavior> GetButtonBehaviorsMap()
    {
        return _ButtonBehaviors.ToDictionary(b => b.Id, b => b.Behavior);
    }
}
