using UnityEngine;

public class CanvasGroupAutoInteractable : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup _CanvasGroup;

    // Runtime only
    private CanvasGroup _CachedCanvasGroup;

    public CanvasGroup CanvasGroup
    {
        get
        {
            if (_CanvasGroup)
                return CanvasGroup;
            if (_CachedCanvasGroup == null)
            {
                _CachedCanvasGroup = GetComponent<CanvasGroup>();
                if (_CachedCanvasGroup == null)
                    Debug.LogWarning("CanvasGroup component not found.");
            }
            return _CachedCanvasGroup;
        }
        set { _CanvasGroup = value; }
    }

    private void Start()
    {
    }

    private void Update()
    {
        if(CanvasGroup != null)
        {
            CanvasGroup.interactable = CanvasGroup.alpha > 0f;
        }
    }
}