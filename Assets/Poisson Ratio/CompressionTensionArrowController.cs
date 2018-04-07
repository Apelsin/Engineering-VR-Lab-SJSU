using UnityEngine;

[ExecuteInEditMode]
public class CompressionTensionArrowController : MonoBehaviour, ISerializationCallbackReceiver
{
    public Transform Tip;
    public Transform End;

    public Transform Target;
    public Transform Reference;

    public void OnAfterDeserialize()
    {
    }

    public void OnBeforeSerialize()
    {
    }

    private void Start()
    {
    }

    private void Update()
    {
        if (Tip && End && Target && Reference)
        {
            bool is_forward = Reference.InverseTransformPoint(Target.position).z > 0f;
            var offset = (is_forward ? Tip.position : End.position) - transform.position;
            transform.position = Target.position + offset;
            transform.rotation = Target.rotation;
            var scale = new Vector3(1f, 1f, is_forward ? 1f : -1f);
            transform.localScale = scale;
        }
    }
}