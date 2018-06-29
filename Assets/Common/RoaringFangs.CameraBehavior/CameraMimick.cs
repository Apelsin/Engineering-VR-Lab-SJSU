/*
The MIT License (MIT)

Copyright (c) 2016 Roaring Fangs Entertainment

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using RoaringFangs.GSR;
using System;
using UnityEngine;

namespace RoaringFangs.CameraBehavior
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class CameraMimick : MonoBehaviour, ITexturable<RenderTexture>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private Camera _Camera;

        [SerializeField]
        private Camera _Source;

        public Camera Source
        {
            get { return _Source; }
            set { _Source = value; }
        }

        [SerializeField]
        private StereoTargetEyeMask _SourceEye;

        public StereoTargetEyeMask SourceEye
        {
            get { return _SourceEye; }
            set { _SourceEye = value; }
        }

        [Serializable]
        public struct PreservationFlags
        {
            public bool Texture;
            public bool CullingMask;
            public bool ClearFlags;
            public bool BackgroundColor;
            public bool Depth;
            public bool TargetEye;
            public bool Aspect;
            public bool FieldOfView;
        }

        public PreservationFlags SettingsToPreserve;

        public RenderTexture Texture
        {
            get
            {
                return _Camera.targetTexture;
            }
            set
            {
                _Camera.targetTexture = value;
            }
        }

        private void OnPreCull()
        {
            if (enabled && _Source)
            {
                var prior_texture = _Camera.targetTexture;
                var prior_culling_mask = _Camera.cullingMask;
                var prior_clear_flags = _Camera.clearFlags;
                var prior_background_color = _Camera.backgroundColor;
                var prior_depth = _Camera.depth;
                var prior_target_eye = _Camera.stereoTargetEye;
                var prior_aspect = _Camera.aspect;
                var prior_fov = _Camera.fieldOfView;

                _Camera.CopyFrom(_Source);

                switch (SourceEye)
                {
                    default:
                    case StereoTargetEyeMask.None:
                        break;

                    case StereoTargetEyeMask.Left:
                        var projection_matrix = _Source.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                        _Camera.projectionMatrix = projection_matrix;
                        //_Camera.worldToCameraMatrix = _Source.GetStereoViewMatrix(Camera.StereoscopicEye.Left);

                        break;

                    case StereoTargetEyeMask.Right:
                        _Camera.projectionMatrix = _Source.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                        _Camera.worldToCameraMatrix = _Source.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
                        break;
                        //case StereoTargetEyeMask.Both:
                        //    _Camera.SetStereoViewMatrix(Camera.StereoscopicEye.Left, _Source.GetStereoViewMatrix(Camera.StereoscopicEye.Left));
                        //    _Camera.SetStereoViewMatrix(Camera.StereoscopicEye.Right, _Source.GetStereoViewMatrix(Camera.StereoscopicEye.Right));
                        //    _Camera.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, _Source.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left));
                        //    _Camera.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, _Source.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right));
                        //    break;
                }

                if (SettingsToPreserve.TargetEye)
                    _Camera.stereoTargetEye = prior_target_eye;
                if (SettingsToPreserve.Texture)
                    _Camera.targetTexture = prior_texture;
                if (SettingsToPreserve.CullingMask)
                    _Camera.cullingMask = prior_culling_mask;
                if (SettingsToPreserve.ClearFlags)
                    _Camera.clearFlags = prior_clear_flags;
                if (SettingsToPreserve.BackgroundColor)
                    _Camera.backgroundColor = prior_background_color;
                if (SettingsToPreserve.Depth)
                    _Camera.depth = prior_depth;
                if (SettingsToPreserve.Aspect)
                    _Camera.ResetAspect();
                if (SettingsToPreserve.FieldOfView)
                    _Camera.fieldOfView = prior_fov;
            }
        }

        private void Start()
        {
        }

        public void OnBeforeSerialize()
        {
            if (_Camera == null)
                _Camera = GetComponent<Camera>();
            if (Source == null && transform.parent != null)
                Source = transform.parent.GetComponentInParent<Camera>();
            //EditorUtilities.OnBeforeSerializeAutoProperties(this);
        }

        public void OnAfterDeserialize()
        {
        }
    }
}