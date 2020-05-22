using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendsHUDView : MonoBehaviour
{
    public const string NOTIFICATIONS_ID = "Friends";
    static int ANIM_PROPERTY_SELECTED = Animator.StringToHash("Selected");

    const string VIEW_PATH = "FriendsHUD";

    public Button closeButton;
    public Button friendsButton;
    public Button friendRequestsButton;
    public FriendsTabView friendsList;
    public FriendRequestsTabView friendRequestsList;

    internal Coroutine currentNotificationRoutine = null;
    internal GameObject currentNotification = null;
    public float notificationsDuration = 3f;

    FriendsHUDController controller;

    public static FriendsHUDView Create(FriendsHUDController controller)
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<FriendsHUDView>();
        view.Initialize(controller);
        return view;
    }

    internal List<FriendEntryBase> GetAllEntries()
    {
        var result = new List<FriendEntryBase>();
        result.AddRange(friendsList.GetAllEntries());
        result.AddRange(friendRequestsList.GetAllEntries());
        return result;
    }

    private void Initialize(FriendsHUDController controller)
    {
        this.controller = controller;
        friendsList.Initialize(this);
        friendRequestsList.Initialize(this);

        closeButton.onClick.AddListener(Toggle);

        friendsButton.onClick.AddListener(() =>
        {
            friendsButton.animator.SetBool(ANIM_PROPERTY_SELECTED, true);
            friendRequestsButton.animator.SetBool(ANIM_PROPERTY_SELECTED, false);
            friendsList.gameObject.SetActive(true);
            friendRequestsList.gameObject.SetActive(false);
        });

        friendRequestsButton.onClick.AddListener(() =>
        {
            friendsButton.animator.SetBool(ANIM_PROPERTY_SELECTED, false);
            friendRequestsButton.animator.SetBool(ANIM_PROPERTY_SELECTED, true);
            friendsList.gameObject.SetActive(false);
            friendRequestsList.gameObject.SetActive(true);
        });
    }

    public void Toggle()
    {
        this.controller.SetVisibility(!gameObject.activeSelf);
    }

#if UNITY_EDITOR
    [ContextMenu("AddFakeRequestReceived")]
    public void AddFakeRequestReceived()
    {
        string id1 = Random.Range(0, 1000000).ToString();
        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            userId = id1,
            name = "Pravus-" + id1
        });

        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = id1,
            action = FriendsController.FriendshipAction.REQUESTED_FROM
        });
    }

    [ContextMenu("AddFakeRequestSent")]
    public void AddFakeRequestSent()
    {
        string id1 = Random.Range(0, 1000000).ToString();

        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            userId = id1,
            name = "Brian-" + id1
        });

        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = id1,
            action = FriendsController.FriendshipAction.REQUESTED_TO
        });
    }

    [ContextMenu("AddFakeOnlineFriend")]
    public void AddFakeOnlineFriend()
    {
        string id1 = Random.Range(0, 1000000).ToString();

        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            userId = id1,
            name = "Brian-" + id1
        });

        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = id1,
            action = FriendsController.FriendshipAction.APPROVED
        });

        FriendsController.i.UpdateUserStatus(new FriendsController.UserStatus() { userId = id1, presence = FriendsController.PresenceStatus.ONLINE });
    }

    [ContextMenu("AddFakeOfflineFriend")]
    public void AddFakeOfflineFriend()
    {
        string id1 = Random.Range(0, 1000000).ToString();

        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            userId = id1,
            name = "Pravus-" + id1
        });

        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = id1,
            action = FriendsController.FriendshipAction.APPROVED
        });

        FriendsController.i.UpdateUserStatus(new FriendsController.UserStatus() { userId = id1, presence = FriendsController.PresenceStatus.OFFLINE });
    }
#endif
}