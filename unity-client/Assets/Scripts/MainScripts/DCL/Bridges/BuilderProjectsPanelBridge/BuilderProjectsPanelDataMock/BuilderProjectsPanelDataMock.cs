﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BuilderProjectsPanelDataMock
{
    private GameObject bridge;

    private Dictionary<string, BuilderProjectsPanelSceneDataMock> projects =
        new Dictionary<string, BuilderProjectsPanelSceneDataMock>();

    public BuilderProjectsPanelDataMock(GameObject bridge)
    {
        this.bridge = bridge;
        GenerateMockProjects();
    }

    public void SendFetchProjects()
    {
        FakeResponse("OnReceivedProjects",FakeProjectsPayload());
    }

    public void SendDuplicateProject(string id)
    {
        if (!projects.TryGetValue(id, out BuilderProjectsPanelSceneDataMock project))
        {
            return;
        }

        int dupliCount = 1;
        string newId = project.id + project.id.Substring(project.id.Length - 1);
        while (projects.ContainsKey(newId))
        {
            newId += newId.Substring(newId.Length - 1);
            dupliCount++;
        }

        var newProject = project;
        newProject.id = newId;
        newProject.isDeployed = false;
        newProject.name = $"{project.name}({dupliCount})";
        projects.Add(newId, newProject);

        SendFetchProjects();
    }

    public void SendUnPublish(string id)
    {
        if (!projects.TryGetValue(id, out BuilderProjectsPanelSceneDataMock project))
        {
            return;
        }

        project.isDeployed = false;
        projects[project.id] = project;
        SendFetchProjects();
    }

    public void SendDelete(string id)
    {
        if (projects.Remove(id))
        {
            SendFetchProjects();
        }
    }

    public void SendQuitContributor(string id)
    {
        if (!projects.TryGetValue(id, out BuilderProjectsPanelSceneDataMock project))
        {
            return;
        }

        project.isContributor = false;
        projects[project.id] = project;
        SendFetchProjects();
    }

    public void SendSceneUpdate(string id, string name, string description, string[] requiredPermissions, bool isMatureContent, bool allowVoiceChat)
    {
        if (!projects.TryGetValue(id, out BuilderProjectsPanelSceneDataMock project))
        {
            return;
        }
        
        project.name = name;
        project.description = description;
        project.requiredPermissions = requiredPermissions;
        project.isMatureContent = isMatureContent;
        project.allowVoiceChat = allowVoiceChat;
        projects[project.id] = project;
        SendFetchProjects();
    }

    private void GenerateMockProjects()
    {
        const int deployedCount = 4;
        const int projectCount = 6;
        const string contentUrl = "https://peer.decentraland.org/content/contents/";
        string[] avatarThumbnail = new[] {"", $"{contentUrl}QmPXa5strH7cWJTZWKYtKiVG8it2nEpkdpQYhKxxnMMapP"};
        string[] scenesThumbnail = new[]
        {
            "", "https://decentraland.org/images/thumbnail/genesis-plaza.png","https://peer.decentraland.org/content/contents/Qmc9nJxR6MoRMaspUMYPRcanrYGeRrz9PCAdo1qSYboaKm",
            "https://peer.decentraland.org/content/contents/QmeRyqAuJojXxtHh26aMQLa2RwgoHW4fLJu7hctQ1Rba3W","https://peer.decentraland.org/content/contents/Qmb7mYufcDYiLv9Vg3zxyCGvpbtLoY2gah6sdonzgRURRV"
        };

        string[][] permissions =
        {
            null, new [] { "" }, new [] { "ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE" }, new [] { "ALLOW_TO_TRIGGER_AVATAR_EMOTE" },
            new [] { "ALLOW_TO_TRIGGER_AVATAR_EMOTE", "ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE" }
        };

        // string[][] contributors =
        // {
        //     null, new []{"0x6bb7a5acab90a40161ee43b094460ba621dfb47f","0x9db59920d3776c2d8a3aa0cbd7b16d81fcab0a2b"},
        //     new []{"0x51777c0b8dba8b4dfe8a1c3d0a1edaa5b139b4e0"}, 
        //     new []{"0x51777c0b8dba8b4dfe8a1c3d0a1edaa5b139b4e0","0x9db59920d3776c2d8a3aa0cbd7b16d81fcab0a2b"}, 
        //     new []{"0x6bb7a5acab90a40161ee43b094460ba621dfb47f", "0xe100cf9c1d7a96a7790cb54b86658572c755ab2f"},
        //     new []{"0x6bb7a5acab90a40161ee43b094460ba621dfb47f", "0xe100cf9c1d7a96a7790cb54b86658572c755ab2f",
        //         "0x51777c0b8dba8b4dfe8a1c3d0a1edaa5b139b4e0", "0x9db59920d3776c2d8a3aa0cbd7b16d81fcab0a2b"}
        // };
        
        string[][] contributors =
        {
            new []{"0x6bb7a5acab90a40161ee43b094460ba621dfb47f", "0xe100cf9c1d7a96a7790cb54b86658572c755ab2f",
                "0x51777c0b8dba8b4dfe8a1c3d0a1edaa5b139b4e0", "0x9db59920d3776c2d8a3aa0cbd7b16d81fcab0a2b"}
        };

        for (int i = 0; i < deployedCount; i++)
        {
            int role = Random.Range(0, 3);
            string id = $"MyDeployedScene{i}";
            projects.Add(id, new BuilderProjectsPanelSceneDataMock()
            {
                id = id,
                name = $"MyDeployedScene{i}",
                isDeployed = true,
                isOwner = role == 0,
                isOperator = role == 1,
                isContributor = role == 2,
                size = new Vector2Int(Random.Range(1,6),Random.Range(1,6)),
                coords = new Vector2Int(Random.Range(-100,100),Random.Range(-100,100)),
                entitiesCount = Random.Range(1,60),
                authorName = $"User#{Random.Range(100,6000)}",
                authorThumbnail = avatarThumbnail[Random.Range(0,avatarThumbnail.Length)],
                thumbnailUrl = scenesThumbnail[Random.Range(0,scenesThumbnail.Length)],
                allowVoiceChat = Random.Range(0, 1) == 1,
                isMatureContent = Random.Range(0, 5) < 1,
                requiredPermissions = permissions[Random.Range(0,permissions.Length)],
                description = Random.Range(0, 1) == 1? "Some description" : "",
                contributors = contributors[Random.Range(0,contributors.Length)],
            });
        }
        for (int i = 0; i < projectCount; i++)
        {
            int role = Random.Range(0, 2);
            string id = $"MyProject{i}";
            projects.Add(id, new BuilderProjectsPanelSceneDataMock()
            {
                id = id,
                name = $"MyProject{i}",
                isDeployed = false,
                isOwner = role == 0,
                isContributor = role == 1,
                size = new Vector2Int(Random.Range(1,6),Random.Range(1,6)),
                coords = new Vector2Int(Random.Range(-100,100),Random.Range(-100,100)),
                entitiesCount = Random.Range(1,60),
                authorName = $"User#{Random.Range(100,6000)}",
                authorThumbnail = avatarThumbnail[Random.Range(0,avatarThumbnail.Length)],
                thumbnailUrl = scenesThumbnail[Random.Range(0,scenesThumbnail.Length)],
                allowVoiceChat = Random.Range(0, 1) == 1,
                isMatureContent = Random.Range(0, 5) < 1,
                requiredPermissions = permissions[Random.Range(0,permissions.Length)],
                description = Random.Range(0, 1) == 1? "Some description" : "",
                contributors = contributors[Random.Range(0,contributors.Length)],
            });
        }
    }

    private string FakeProjectsPayload()
    {
        string value = "[";
        var projectsArray = projects.Values.ToArray();
        for (int i = 0; i < projectsArray.Length; i++)
        {
            value += JsonUtility.ToJson(projectsArray[i]);
            if (i < projectsArray.Length - 1) value += ",";
        }
        value += "]";
        return value;
    }

    private void FakeResponse(string method, string payload)
    {
        bridge.SendMessage(method,payload);
    }
}
