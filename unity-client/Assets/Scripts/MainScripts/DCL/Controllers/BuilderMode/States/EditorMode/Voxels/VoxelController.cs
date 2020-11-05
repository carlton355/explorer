using DCL.Controllers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelController : MonoBehaviour
{
    [Header("References")]
    public VoxelPrefab voxelPrefab;
    public BuilderInputWrapper builderInputWrapper;
    public BuildModeController buildModeController;
    public BuildEditorMode buildEditorMode;
    public OutlinerController outlinerController;
    public FreeCameraMovement freeCameraMovement;
    
    public LayerMask groundLayer;

    DecentralandEntityToEdit lastVoxelCreated;

    GameObject editionGO;
    bool mousePressed = false, isVoxelModelActivated = false,isCreatingMultipleVoxels = false; 
    Vector3Int lastVoxelPositionPressed;
    Vector3 lastMousePosition;
    Dictionary<Vector3Int, VoxelPrefab> createdVoxels = new Dictionary<Vector3Int, VoxelPrefab>();
    ParcelScene currentScene;
    private void Start()
    {
        builderInputWrapper.OnMouseDown += MouseDown;
        builderInputWrapper.OnMouseUp += MouseUp;
 
    }

    private void Update()
    {
        if (mousePressed && isVoxelModelActivated && isCreatingMultipleVoxels)
        {

            bool fillVoxels = false;
            Vector3Int currentPosition = Vector3Int.zero;
            VoxelEntityHit voxelHit = buildModeController.GetCloserUnselectedVoxelEntityOnPointer();
            if (voxelHit != null && voxelHit.entityHitted.tag == "Voxel" && !voxelHit.entityHitted.IsSelected)
            {
                Vector3Int position = ConverPositionToVoxelPosition(voxelHit.entityHitted.rootEntity.gameObject.transform.position);
                position += voxelHit.hitVector;

                currentPosition = position;
                fillVoxels = true;
            }
            else
            {
                RaycastHit hit;
                UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, 9999, groundLayer))
                {
                    currentPosition = ConverPositionToVoxelPosition(hit.point);
                    fillVoxels = true;

                }
            }
            if (fillVoxels)
            {
                FillVoxels(lastVoxelPositionPressed, currentPosition);
            }

        }
    }

    public void SetSceneToEdit(ParcelScene scene)
    {
        currentScene = scene;
    }

    public void SetEditObjectLikeVoxel()
    {
        if (!mousePressed && isVoxelModelActivated)
        {
            VoxelEntityHit voxelHit = buildModeController.GetCloserUnselectedVoxelEntityOnPointer();

            if (voxelHit != null && voxelHit.entityHitted.IsSelected) return;
            if (voxelHit != null && voxelHit.entityHitted.tag == "Voxel")
            {


                Vector3 position = ConverPositionToVoxelPosition(voxelHit.entityHitted.rootEntity.gameObject.transform.position);
                position += voxelHit.hitVector;
                editionGO.transform.position = position;
            }
            else
            {
                RaycastHit hit;
                UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, 9999, groundLayer))
                {
                    Vector3 position = hit.point;
                    editionGO.transform.position = ConverPositionToVoxelPosition(hit.point);
                }
            }
        }
    }

    public void SetEditionGO(GameObject _editionGO)
    {
        editionGO = _editionGO;
    }

    public bool IsActive()
    {
        return isVoxelModelActivated;
    }
    public void SetActiveMode(bool isActive)
    {
        isVoxelModelActivated = isActive;
    }

    public void EndMultiVoxelSelection()
    {
        List<DecentralandEntityToEdit> voxelEntities = buildModeController.GetAllVoxelsEntities();

        foreach (DecentralandEntityToEdit voxelEntity in voxelEntities)
        {
            if (BuildModeUtils.IsWithInSelectionBounds(voxelEntity.gameObject.transform, lastMousePosition, Input.mousePosition))
            {
                buildModeController.SelectEntity(voxelEntity);
            }
        }
        buildModeController.SetOutlineCheckActive(true);
        outlinerController.CancelAllOutlines();
    }

    void FillVoxels(Vector3Int firstPosition, Vector3Int lastPosition)
    {
        int xDifference = Mathf.Abs(firstPosition.x - lastPosition.x);
        int yDifference = Mathf.Abs(firstPosition.y - lastPosition.y);
        int zDifference = Mathf.Abs(firstPosition.z - lastPosition.z);


        List<Vector3Int> mustContainVoxelList = new List<Vector3Int>();
        List<DecentralandEntityToEdit> voxelEntities = buildModeController.GetAllVoxelsEntities();
        List<DecentralandEntityToEdit> allEntities = buildModeController.GetAllEntitiesFromCurrentScene();
        


        for (int x = 0; x <= xDifference; x++)
        {
            int contX = x;
            if (firstPosition.x > lastPosition.x) contX = -contX;

            for (int y = 0; y <= yDifference; y++)
            {
                int contY = y;
                if (firstPosition.y > lastPosition.y) contY = -contY;

                for (int z = 0; z <= zDifference; z++)
                {
                    int contZ = z;
                    if (firstPosition.z > lastPosition.z) contZ = -contZ;

                    Vector3Int positionOfVoxel = new Vector3Int(firstPosition.x + contX, firstPosition.y + contY, firstPosition.z + contZ);
                    if (positionOfVoxel == firstPosition) continue;
                    if (ExistVoxelAtPosition(positionOfVoxel,voxelEntities)) continue;
                    CreateVoxel(positionOfVoxel);
                    mustContainVoxelList.Add(positionOfVoxel);
                }
            }
        }


        List<Vector3Int> voxelToRemove = new List<Vector3Int>();
        foreach(Vector3Int position in createdVoxels.Keys)
        {
            if (!mustContainVoxelList.Contains(position)) voxelToRemove.Add(position);
        }

        foreach(Vector3Int vector in voxelToRemove)
        {
            Destroy(createdVoxels[vector].gameObject);
            createdVoxels.Remove(vector);
        }
        foreach (VoxelPrefab keyValuePair in createdVoxels.Values)
        {
            if (IsVoxelAtValidPoint(keyValuePair,allEntities)) keyValuePair.SetAvailability(true);
            else keyValuePair.SetAvailability(false);
        }
    }

    bool ExistVoxelAtPosition(Vector3Int position,List<DecentralandEntityToEdit> voxelEntities)
    {
        foreach (DecentralandEntityToEdit voxelEntity in voxelEntities)
        {
            if (position == ConverPositionToVoxelPosition(voxelEntity.transform.position)) return true;
        }
        return false;
    }

    VoxelPrefab CreateVoxel(Vector3Int position)
    {
        if (!createdVoxels.ContainsKey(position))
        {

            VoxelPrefab go = Instantiate(voxelPrefab.gameObject, position, lastVoxelCreated.rootEntity.gameObject.transform.rotation).GetComponent<VoxelPrefab>();
            createdVoxels.Add(position, go);
            return go;
        }
        return null;

    }

    private void MouseUp(int buttonID, Vector3 position)
    {
        if (mousePressed && buttonID == 0)
        {
            if(isCreatingMultipleVoxels)
            {
                lastVoxelCreated.transform.SetParent(null);
                bool canVoxelsBeCreated = true;
                foreach (VoxelPrefab voxel in createdVoxels.Values)
                {
                    if(!voxel.IsAvailable())
                    {
                        canVoxelsBeCreated = false;
                        break;
                    }
                }

                foreach (Vector3Int voxelPosition in createdVoxels.Keys)
                {
                    if (canVoxelsBeCreated)
                    {

                        DecentralandEntity entity = buildModeController.DuplicateEntity(lastVoxelCreated);
                        entity.gameObject.tag = "Voxel";
                        entity.gameObject.transform.position = voxelPosition;
                    }
                    Destroy(createdVoxels[voxelPosition].gameObject);
                }
                if (!canVoxelsBeCreated) buildModeController.DeleteEntity(lastVoxelCreated);
                createdVoxels.Clear();
                buildModeController.DeselectEntities();

                lastVoxelCreated = null;
                isCreatingMultipleVoxels = false;

                mousePressed = false;
                freeCameraMovement.SetCameraCanMove(true);
            }
     
        }
    }

    void MouseDown(int buttonID, Vector3 position)
    {
        if (buttonID == 0)
        {
 
            if (isVoxelModelActivated && lastVoxelCreated != null)
            {         
                lastVoxelPositionPressed = ConverPositionToVoxelPosition(lastVoxelCreated.transform.position);
                mousePressed = true;
                freeCameraMovement.SetCameraCanMove(false);
                isCreatingMultipleVoxels = true;

            }
        }
    }

    public void SetVoxelSelected(DecentralandEntityToEdit decentralandEntityToEdit)
    {
        lastVoxelCreated = decentralandEntityToEdit;
        lastVoxelCreated.transform.localPosition = Vector3.zero;
    }


    public Vector3Int ConverPositionToVoxelPosition(Vector3 rawPosition)
    {
        Vector3Int position = Vector3Int.zero;
        position.x = Mathf.CeilToInt(rawPosition.x);
        position.y = Mathf.CeilToInt(rawPosition.y);
        position.z = Mathf.CeilToInt(rawPosition.z);
        return position;
    }

    bool IsVoxelAtValidPoint(VoxelPrefab voxelPrefab,List<DecentralandEntityToEdit> entitiesToCheck)
    {
        if (!currentScene.IsInsideSceneBoundaries(voxelPrefab.meshRenderer.bounds)) return false;
        Bounds bounds = voxelPrefab.meshRenderer.bounds;
        bounds.size -= Vector3.one * 0.05f; 
        foreach (DecentralandEntityToEdit entity in entitiesToCheck)
        {
            if (entity.rootEntity.meshesInfo == null || entity.rootEntity.meshesInfo.renderers == null) continue;
            if (bounds.Intersects(entity.rootEntity.meshesInfo.mergedBounds)) return false;
        }
        bounds.size += Vector3.one * 0.05f;
        return true;
    }
   

}
