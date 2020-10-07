using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityListAdapter : MonoBehaviour
{
    public Color selectedColor;
    public TextMeshProUGUI nameTxt;
    public Image selectedImg, lockImg,showImg;
    public System.Action<BuildModeEntityListController.EntityAction, DecentrelandEntityToEdit, EntityListAdapter> OnActioninvoked;
    DecentrelandEntityToEdit currentEntity;

    public void SetContent(DecentrelandEntityToEdit _decentrelandEntity)
    {
        currentEntity = _decentrelandEntity;
        nameTxt.text = currentEntity.rootEntity.entityId;
        if (currentEntity.rootEntity.gameObject.activeSelf) showImg.color = selectedColor;
        else showImg.color = Color.white;

        if (!currentEntity.isLocked) lockImg.color = Color.white;
        else lockImg.color = selectedColor;


        selectedImg.enabled = _decentrelandEntity.isSelected;
    }

    public void SelectOrDeselect()
    {
        OnActioninvoked?.Invoke(BuildModeEntityListController.EntityAction.SELECT,currentEntity, this);
    }
    public void ShowOrHide()
    {
         OnActioninvoked?.Invoke(BuildModeEntityListController.EntityAction.SHOW, currentEntity, this);
    }

    public void LockOrUnlock()
    {
        OnActioninvoked?.Invoke(BuildModeEntityListController.EntityAction.LOCK, currentEntity, this);
    }

    public void DeleteEntity()
    {
        OnActioninvoked?.Invoke(BuildModeEntityListController.EntityAction.DELETE, currentEntity, this);
    }

    public void DuplicateEntity()
    {
        OnActioninvoked?.Invoke(BuildModeEntityListController.EntityAction.DUPLICATE, currentEntity, this);
    }

    public void GroupEntity()
    {

    }
}
