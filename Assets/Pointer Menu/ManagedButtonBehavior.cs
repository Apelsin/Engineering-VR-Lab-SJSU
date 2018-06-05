using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct ManagedButtonBehavior
{
    public Func<bool> EnabledCallback;
    public UnityEvent Clicked;
    public bool IsEnabled { get { return EnabledCallback == null || EnabledCallback(); } }
}
