using UnityEngine;
using UnityEngine.XR;

namespace libigl.Behaviour
{
    public unsafe partial class LibiglBehaviour
    {
        // C# only input variables
        private Vector2 _lastPrimaryAxisValueL;
        
        private void UpdateInput()
        {
            if (!InputManager.get.RightHand.isValid) return;

            switch (_input.ActiveTool)
            {
                case ToolType.Default:
                    UpdateInputDefault();
                    UpdateInputTransform();
                    break;
                case ToolType.Select:
                    UpdateInputSelect();
                    UpdateInputTransform();
                    break;
            }
        }

        /// <summary>
        /// Input for the default tool
        /// </summary>
        private void UpdateInputDefault()
        {
            if (InputManager.get.RightHand.TryGetFeatureValue(CommonUsages.secondaryButton,
                out var secondaryBtnValue) && secondaryBtnValue)
            {
                _input.DoTransform = true;
            }
        }

        /// <summary>
        /// Gathering input for the select tool
        /// </summary>
        private void UpdateInputSelect()
        {
            // Change the selection with the left hand primary2DAxis.y
            if (InputManager.get.LeftHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out var primaryAxisL))
            {
                if (Mathf.Abs(_lastPrimaryAxisValueL.y) < 0.05f && Mathf.Abs(primaryAxisL.y) > 0.05f)
                    _input.ChangeActiveSelection((int) Mathf.Sign(primaryAxisL.y));

                _lastPrimaryAxisValueL = primaryAxisL;
            }

            if (InputManager.get.RightHand.TryGetFeatureValue(CommonUsages.primaryButton, out var primaryBtnValue) &&
                primaryBtnValue)
            {
                _input.DoSelect = true;
                if (InputManager.get.RightHand.TryGetFeatureValue(CommonUsages.devicePosition,
                    out var rightHandPos))
                {
                    _input.SelectPos = _libiglMesh.transform.InverseTransformPoint(
                        InputManager.get.XRRig.TransformPoint(rightHandPos));
                }
                else
                    Debug.LogWarning("Could not get Right Hand Position");
            }

            if (InputManager.get.RightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out var primaryAxisValue))
            {
                if (Mathf.Abs(primaryAxisValue.y) > 0.01f)
                {
                    _input.SelectRadiusSqr = Mathf.Clamp(
                        Mathf.Sqrt(_input.SelectRadiusSqr) + 0.5f * primaryAxisValue.y * Time.deltaTime,
                        0.01f, 1f);
                    _input.SelectRadiusSqr *= _input.SelectRadiusSqr;
                }
            }
        }

        private void UpdateInputTransform()
        {
            HandTransformInput(InputManager.get.LeftHand, false, ref _input.GripL, ref _input.HandPosL);
            HandTransformInput(InputManager.get.RightHand, true, ref _input.GripR, ref _input.HandPosR);
        }

        /// <summary>
        /// Updates transform tool input for a hand
        /// </summary>
        /// <param name="inputGrip">Where to store the trigger input value</param>
        /// <param name="inputHandPos">Where to store the hand position input value</param>
        private void HandTransformInput(InputDevice inputDevice, bool isRight, ref float inputGrip, ref Vector3 inputHandPos)
        {
            if (!inputDevice.TryGetFeatureValue(CommonUsages.grip, out var grip)) return;
            
            inputGrip = grip;
            inputDevice.TryGetFeatureValue(CommonUsages.devicePosition, out var handPos);
            inputHandPos = handPos;

            // Handling changes in the selection 'state machine'
            if (grip > 0.01f)
            {
                if (!_input.DoTransform)
                {
                    _input.DoTransform = true;
                    _input.PrimaryTransformHand = isRight;
                }
                else
                    _input.SecondaryTransformHandActive = true;
            }
            else
            {
                if (_input.DoTransform && _input.PrimaryTransformHand)
                {
                    if (_input.SecondaryTransformHandActive)
                        _input.PrimaryTransformHand = !isRight;
                    else
                        _input.DoTransform = false;
                }
            }

        }

        /// <summary>
        /// Consumes and resets flags raised. Should be called in PreExecute after copying to the State.
        /// </summary>
        private void ConsumeInput()
        {
            if(_input.ActiveTool == ToolType.Select && _input.DoTransform)
            {
                // Only update this if we are transforming on the thread, i.e. transforming the selection
                _input.PrevTrafoHandPosL = _input.HandPosL;
                _input.PrevTrafoHandPosR = _input.HandPosR;
            }
            
            // Consume inputs here
            _input.DoTransform = false;
            _input.DoSelect = false;
            _input.DoHarmonic = false;
        }
    }
}