using System;
using UnityEngine;
using UnityEngine.Events;

public class InstancedRenderTexture : MonoBehaviour
{
    [Serializable]
    public class RenderTextureInstantiatedEvent : UnityEvent<RenderTexture> { }
    public RenderTexture Reference;
    public RenderTexture Instance { get; private set; }

    public RenderTextureInstantiatedEvent Instantiated;

    private void OnEnable()
    {
        Instance = Instantiate(Reference);
        Instantiated.Invoke(Instance);
    }
}