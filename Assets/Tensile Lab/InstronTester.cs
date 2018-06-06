using RoaringFangs.ASM;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CVRLabSJSU
{
    public class InstronTester : MonoBehaviour
    {
        public List<string> TestMaterialTypes;
        public Animator GrabberAnimator;
        public ControlledStateManager GrabberCSM;

        public Transform TopClampPoint;
        public Transform BaseClampPoint;
        public GameObject ClampedSpecimen;

        [Range(0f, 1f)]
        public float ClampCenterBalance = 0.5f;

        public bool GrabberIsReset => GrabberCSM.ActiveStateControllers.Any(c => c.Tag == "Reset");
        public bool GrabberIsBusy => GrabberCSM.ActiveStateControllers.Any(c => c.Tag == "Busy");

        public void UpdateGrabberAnimatorParameters()
        {
            var specimen_properties = ClampedSpecimen?.GetComponent<TTSpecimenProperties>();
            if (specimen_properties)
            {
                var material_type = specimen_properties.MaterialType;
                var material_index = TestMaterialTypes.IndexOf(material_type);
                GrabberAnimator.SetInteger("Material Type", material_index);
            }
        }

        public void OnBeginTensileTest()
        {
            UpdateGrabberAnimatorParameters();
            GrabberAnimator.SetTrigger("Start");
        }

        public void OnToggleTensileTest()
        {
            UpdateGrabberAnimatorParameters();
            GrabberAnimator.SetTrigger("Toggle");
        }

        public void OnResetTensileTest()
        {
            UpdateGrabberAnimatorParameters();
            GrabberAnimator.SetTrigger("Reset");
        }

        private static void StretchSubject(Transform @base, Transform @top, Transform subject_xform, float subject_size_x, float center_balance)
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

        private void LateUpdate()
        {
            if (TopClampPoint && BaseClampPoint && ClampedSpecimen)
            {
                //var base_pos = BaseClampPoint.position;
                //var top_pos = TopClampPoint.position;
                //var subject_xform = Subject.transform;

                // TODO: make this not hacky

                var mesh_filter = ClampedSpecimen.GetComponent<MeshFilter>();
                var subject_bounds = mesh_filter?.sharedMesh?.bounds ?? new Bounds(Vector3.zero, Vector3.one);
                var subject_size_x = 2f * subject_bounds.extents.x;
                StretchSubject(BaseClampPoint, TopClampPoint, ClampedSpecimen.transform, subject_size_x, ClampCenterBalance);
            }
        }
    }
}