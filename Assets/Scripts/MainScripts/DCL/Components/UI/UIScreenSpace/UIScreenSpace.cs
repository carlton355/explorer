using System;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIScreenSpace : UIShape
    {
        static bool VERBOSE = false;
        public Canvas canvas;

        private static bool globalVisibility = true;
        private static bool GlobalVisibility
        {
            get => globalVisibility;
            set
            {
                globalVisibility = value;
                OnUIGlobalVisibilityChanged.Invoke();
            }
        }

        private static Action OnUIGlobalVisibilityChanged = () => { };

        private Vector3 currentCharacterPosition;

        public UIScreenSpace(ParcelScene scene) : base(scene)
        {
            DCLCharacterController.OnCharacterMoved += OnCharacterMoved;
            //Only no-dcl scenes are listening the the global visibility event
            if (!scene.isPersistent)
            {
                OnUIGlobalVisibilityChanged += UpdateCanvasVisibility;
            }
        }

        public override void AttachTo(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            Debug.LogError(
                "Aborted UIScreenShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = SceneController.i.SafeFromJson<Model>(newJson);

            if (scene.uiScreenSpace == null)
            {
                scene.uiScreenSpace = this;

                SceneController.i.StartCoroutine(InitializeCanvas());
            }
            else if (DCLCharacterController.i != null)
            {
                OnCharacterMoved(DCLCharacterController.i.transform.position);
            }

            return null;
        }

        public override void Dispose()
        {
            DCLCharacterController.OnCharacterMoved -= OnCharacterMoved;
            OnUIGlobalVisibilityChanged -= UpdateCanvasVisibility;

            if (childHookRectTransform != null)
            {
                Utils.SafeDestroy(childHookRectTransform.gameObject);
            }
        }

        void OnCharacterMoved(Vector3 newCharacterPosition)
        {
            if (canvas != null)
            {
                currentCharacterPosition = newCharacterPosition;
                UpdateCanvasVisibility();

                if (VERBOSE)
                {
                    Debug.Log($"set screenspace = {canvas.enabled}... {currentCharacterPosition}");
                }
            }
        }
        private void UpdateCanvasVisibility()
        {
            if (canvas != null && scene != null)
                canvas.enabled = scene.IsInsideSceneBoundaries(currentCharacterPosition) && model.visible && (scene.isPersistent || GlobalVisibility);
        }

        IEnumerator InitializeCanvas()
        {
            if (VERBOSE)
            {
                Debug.Log("Started canvas initialization in " + id);
            }

            GameObject canvasGameObject = new GameObject("UIScreenSpace");
            canvasGameObject.layer = LayerMask.NameToLayer("UI");
            canvasGameObject.transform.SetParent(scene.transform);
            canvasGameObject.transform.ResetLocalTRS();

            canvas = canvasGameObject.AddComponent<Canvas>();
            // Canvas
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Canvas Scaler (for maintaining ui aspect ratio)
            CanvasScaler canvasScaler = canvasGameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1280f, 720f);
            canvasScaler.matchWidthOrHeight = 1f; // Match height, recommended for landscape projects

            // Graphics Raycaster (for allowing touch/click input on the ui components)
            canvasGameObject.AddComponent<GraphicRaycaster>();

            childHookRectTransform = canvas.GetComponent<RectTransform>();

            CanvasGroup canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            canvas.gameObject.SetActive(false);
            canvas.gameObject.SetActive(true);

            // we enable the canvas for 2 frames to force its auto-scaling
            yield return null;
            yield return null;

            canvasGroup.alpha = 1f;

            if (VERBOSE)
            {
                Debug.Log("canvas initialized, width: " + childHookRectTransform.rect.width);
                Debug.Log("canvas initialized, height: " + childHookRectTransform.rect.height);
            }

            if (canvas != null)
            {
                canvas.enabled = false; // It will be enabled later when the player enters this scene
            }

            if (DCLCharacterController.i != null)
            {
                OnCharacterMoved(DCLCharacterController.i.transform.position);
            }

            if (VERBOSE)
            {
                Debug.Log("Finished canvas initialization in " + id);
            }

            //Only for the DCL UI scene
            if (scene.isPersistent)
            {
                CreateGlobalVisibilityToggle();
            }
            else
            {
                UpdateCanvasVisibility();
            }
        }

        private void CreateGlobalVisibilityToggle()
        {
            GameObject toggleGameObject = UnityEngine.Object.Instantiate(Resources.Load("GlobalVisibilityToggle"), childHookRectTransform) as GameObject;
            if (toggleGameObject == null)
            {
                Debug.Log("Cannot find Global Visibility Toggle");
                return;
            }

            var toggle = toggleGameObject.GetComponent<Toggle>();
            if (toggle == null)
            {
                Debug.Log("Global Visibility Toggle contains no toggle");
                return;
            }

            toggle.onValueChanged.AddListener((x) => GlobalVisibility = x);
            toggle.isOn = true;
        }
    }
}