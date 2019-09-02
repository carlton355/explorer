﻿using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;

namespace DCL.Components
{
    public abstract class ParametrizedShape<T> : BaseShape where T : BaseShape.Model, new()
    {
        public T model = new T();

        public abstract Mesh GenerateGeometry();

        protected virtual void DestroyGeometry()
        {
            GameObject.Destroy(currentMesh);
        }

        public Mesh currentMesh { get; private set; }

        public ParametrizedShape(ParcelScene scene) : base(scene)
        {
            OnAttach += OnShapeAttached;
            OnDetach += OnShapeDetached;
        }

        void OnShapeAttached(DecentralandEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            entity.EnsureMeshGameObject(componentName + " mesh");

            MeshFilter meshFilter = entity.meshGameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = entity.meshGameObject.AddComponent<MeshRenderer>();

            meshFilter.sharedMesh = currentMesh;

            if (Configuration.ParcelSettings.VISUAL_LOADING_ENABLED)
            {
                MaterialTransitionController transition =
                    entity.meshGameObject.AddComponent<MaterialTransitionController>();
                Material finalMaterial = Utils.EnsureResourcesMaterial("Materials/Default");
                transition.delay = 0;
                transition.useHologram = false;
                transition.fadeThickness = 20;
                transition.OnDidFinishLoading(finalMaterial);
            }
            else
            {
                meshRenderer.sharedMaterial = Utils.EnsureResourcesMaterial("Materials/Default");
            }

            if (entity.OnShapeUpdated != null)
            {
                entity.OnShapeUpdated.Invoke(entity);
            }

            CollidersManager.i.ConfigureColliders(entity.meshGameObject, model.withCollisions, entity: entity);
        }

        void OnShapeDetached(DecentralandEntity entity)
        {
            if (entity == null || entity.meshGameObject == null)
            {
                return;
            }

            if (attachedEntities.Count == 0)
            {
                if (currentMesh != null)
                {
                    DestroyGeometry();
                }

                Utils.CleanMaterials(entity.meshGameObject.GetComponent<Renderer>());
            }

            Utils.SafeDestroy(entity.meshGameObject);
            entity.meshGameObject = null;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            var newModel = SceneController.i.SafeFromJson<T>(newJson);
            bool updateVisibility = newModel.visible != model.visible;
            bool updateCollisions = newModel.withCollisions != model.withCollisions;
            bool generateNewMesh = ShouldGenerateNewMesh(newModel);
            model = newModel;

            if (generateNewMesh)
                currentMesh = GenerateGeometry();

            if (generateNewMesh || updateVisibility || updateCollisions)
            {
                foreach (var entity in this.attachedEntities)
                {
                    if (generateNewMesh)
                    {
                        OnShapeDetached(entity);
                        OnShapeAttached(entity);
                    }

                    if (updateVisibility)
                        ConfigureVisibility(entity.meshGameObject, model.visible);

                    if (updateCollisions)
                        CollidersManager.i.ConfigureColliders(entity.meshGameObject, model.withCollisions);
                }
            }

            return null;
        }

        public override void AttachTo(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            base.AttachTo(entity);
            ConfigureVisibility(entity.meshGameObject, model.visible);
        }

        protected virtual bool ShouldGenerateNewMesh(BaseShape.Model newModel)
        {
            return true;
        }
    }
}
