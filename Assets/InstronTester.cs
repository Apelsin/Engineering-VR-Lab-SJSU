using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InstronTester : MonoBehaviour
{
    public string CurrentTestMaterialType;
    public List<string> TestMaterialTypes;
    public Animator GrabberAnimator;

    public Transform TopClampPoint;
    public Transform BaseClampPoint;
    public GameObject Subject;
    

    [Range(0f, 1f)]
    public float ClampCenterBalance = 0.5f;

    public void OnBeginTensileTest()
    {
        GrabberAnimator.SetTrigger("Start");
    }

    public void OnResetTensileTest()
    {
        GrabberAnimator.SetTrigger("Reset");
    }

    private void Start()
    {
        var type_id = TestMaterialTypes.IndexOf(CurrentTestMaterialType);
        GrabberAnimator.SetInteger("Test Material Type", type_id);
    }

    private void Update()
    {
    }

    static void StretchSubject(Transform @base, Transform @top, Transform subject_xform, float subject_size_x, float center_balance)
    {
        var base_pos = @base.position;
        var top_pos = top.position;
        float length = Vector3.Distance(base_pos, top_pos);
        Vector3 subject_center = Vector3.Lerp(base_pos, top_pos, center_balance);


        // Assuming length is along X axis!
        var subject_length = Vector3.Distance(base_pos, top_pos);
        var subject_length_scale = subject_length / subject_size_x;
        var right = top_pos - base_pos;
        var forward = @base.forward;
        var up = Vector3.Cross(right, forward);
        var subject_rotation = Quaternion.LookRotation(forward, up);
        var subject_scale = subject_xform.localScale;
        subject_scale.x = subject_length_scale;

        subject_xform.position = subject_center;
        subject_xform.rotation = subject_rotation;
        subject_xform.localScale = subject_scale;
    }

    void LateUpdate()
    {
        if(TopClampPoint && BaseClampPoint && Subject)
        {
            //var base_pos = BaseClampPoint.position;
            //var top_pos = TopClampPoint.position;
            //var subject_xform = Subject.transform;

            // TODO: make this not hacky

            var mesh_filter = Subject.GetComponent<MeshFilter>();
            var subject_bounds = mesh_filter?.sharedMesh?.bounds ?? new Bounds(Vector3.zero, Vector3.one);
            var subject_size_x = 2f * subject_bounds.extents.x;

            StretchSubject(BaseClampPoint, TopClampPoint, Subject.transform, subject_size_x, ClampCenterBalance);
        }
    }
}