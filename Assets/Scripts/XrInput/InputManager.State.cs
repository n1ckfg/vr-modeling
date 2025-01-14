﻿using System;
using UnityEngine;
using UnityEngine.XR;

namespace XrInput
{
    public partial class InputManager
    {
        #region Tools

        /// <summary>
        /// Sets the active tool and updates the UI
        /// </summary>
        public void SetActiveTool(ToolType value)
        {
            State.ActiveTool = value;

            RepaintInputHints();

            if (BrushL) BrushL.OnActiveToolChanged();
            if (BrushR) BrushR.OnActiveToolChanged();

            OnActiveToolChanged();
        }

        /// <summary>
        /// Safely repaint the input hints on the controllers, specify which hands should be repainted.
        /// </summary>
        public void RepaintInputHints(bool left = true, bool right = true)
        {
            if (left && HandHintsL)
                HandHintsL.Repaint();
            if (right && HandHintsR)
                HandHintsR.Repaint();
        }

        #endregion

        #region Updating the InputState

        /// <summary>
        /// Updates the <see cref="InputState"/> <see cref="State"/>.
        /// Implementation note: The hands may not be initialized/detected yet.
        /// </summary>
        private void UpdateSharedState()
        {
            StatePrev = State;

            HandL.TryGetFeatureValue(CommonUsages.primaryButton, out State.PrimaryBtnL);
            HandL.TryGetFeatureValue(CommonUsages.secondaryButton, out State.SecondaryBtnL);
            HandL.TryGetFeatureValue(CommonUsages.primary2DAxis, out State.PrimaryAxisL);

            HandR.TryGetFeatureValue(CommonUsages.primaryButton, out State.PrimaryBtnR);
            HandR.TryGetFeatureValue(CommonUsages.secondaryButton, out State.SecondaryBtnR);
            HandR.TryGetFeatureValue(CommonUsages.primary2DAxis, out State.PrimaryAxisR);


            // Read values and then convert to world space
            HandL.TryGetFeatureValue(CommonUsages.grip, out State.GripL);
            HandL.TryGetFeatureValue(CommonUsages.trigger, out State.TriggerL);
            HandL.TryGetFeatureValue(CommonUsages.devicePosition, out State.HandPosL);
            HandL.TryGetFeatureValue(CommonUsages.deviceRotation, out State.HandRotL);

            HandR.TryGetFeatureValue(CommonUsages.grip, out State.GripR);
            HandR.TryGetFeatureValue(CommonUsages.trigger, out State.TriggerR);
            HandR.TryGetFeatureValue(CommonUsages.devicePosition, out State.HandPosR);
            HandR.TryGetFeatureValue(CommonUsages.deviceRotation, out State.HandRotR);

            // Convert to world space
            var xrRigRotation = xrRig.rotation;
            State.HandPosL = xrRig.TransformPoint(State.HandPosL);
            State.HandRotL = xrRigRotation * State.HandRotL;
            State.HandPosR = xrRig.TransformPoint(State.HandPosR);
            State.HandRotR = xrRigRotation * State.HandRotR;

            // Update hover targets of rays
            State.IsTeleporting = State.PrimaryAxisL.y > 0.1f; // Threshold should be lower than teleportRayL's
            if (!State.IsTeleporting)
            {
                if (handInteractorL)
                    handInteractorL.GetHoverTargets(_rayHoverTargetsL);
                if (handInteractorR)
                    handInteractorR.GetHoverTargets(_rayHoverTargetsR);
            }

            // Ray Interactor & Teleportation
            UpdateRayInteractors();

            // Input conflict with interactables
            if (handInteractorL)
                State.GripL *= handInteractorL.selectTarget ? 0f : 1f;
            if (handInteractorR)
                State.GripR *= handInteractorR.selectTarget ? 0f : 1f;
            State.TriggerL *= !State.IsTeleporting && _rayHoverTargetsL.Count == 0 ? 1f : 0f;
            State.TriggerR *= !State.IsTeleporting && _rayHoverTargetsR.Count == 0 ? 1f : 0f;

            // Changing Active Tool
            if (State.SecondaryBtnL && !StatePrev.SecondaryBtnL)
                SetActiveTool((ToolType)
                    ((State.ActiveTool.GetHashCode() + 1) % Enum.GetNames(typeof(ToolType)).Length));

            // Brush Resizing
            if (Mathf.Abs(State.PrimaryAxisR.y) > XrBrush.ResizeDeadZone)
            {
                State.BrushRadius = Mathf.Clamp(
                    State.BrushRadius + XrBrush.ResizeSpeed * Time.deltaTime * State.PrimaryAxisR.y,
                    XrBrush.RadiusRange.x, XrBrush.RadiusRange.y);

                if (BrushL)
                    BrushL.SetRadius(State.BrushRadius);
                BrushR.SetRadius(State.BrushRadius);
            }

            // Changing the Active Mesh
            if (State.ActiveTool == ToolType.Transform &&
                State.TriggerR > 0.1f && StatePrev.TriggerR < 0.1f)
            {
                BrushR.SetActiveMesh();
            }
        }

        #endregion
    }
}