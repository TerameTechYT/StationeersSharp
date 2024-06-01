#pragma warning disable CA5394

using Assets.Scripts;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Inventory;
using Assets.Scripts.Util;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LoulanRevived;

internal class Functions
{
    public static void LoadIncidents(ref WorldManager worldManager)
    {
        string ModPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;

        foreach (string directory in Directory.GetDirectories(Path.Combine(ModPath, "Incidents")))
        {
            MissionData incidentData = Incident.GetIncident(new DirectoryInfo(directory));

            Incident.LoadIncident(incidentData);
        }
    }

    public static bool GenerateRandomIncident(ref TileSystem tileSystem, TileData tileData, bool onTileEnter = false)
    {
        if (tileSystem.CurrentIncidentTypes.Count == 0 || tileData.IncidentRand == null)
        {
            return false;
        }

        int key = tileSystem.CurrentIncidentTypes.ElementAt(tileData.IncidentRand.Next(tileSystem.CurrentIncidentTypes.Count)).Key;
        WorldManager.TerrainFeatureIncident terrainFeatureIncident = tileSystem.CurrentIncidentTypes[key];
        if (!Incident.CachedGridIncidents.TryGetValue(terrainFeatureIncident.Type, out List<MissionData> value) || value.Count == 0 || (onTileEnter && !terrainFeatureIncident.RunOnTileEnter) || (terrainFeatureIncident.RequiresHumanInTile && !tileData.IsCenterTile))
        {
            return false;
        }

        int num = tileData.IncidentRand.Next(terrainFeatureIncident.MaxPerTile + 1);
        for (int i = 0; i < num; i++)
        {
            if (tileData.IncidentRand.Next(10000) > terrainFeatureIncident.SpawnChance)
            {
                continue;
            }

            int index = tileData.IncidentRand.Next(value.Count);
            Vector3 vector = new(tileData.IncidentRand.Next(tileData.OriginWorldCoords.x, tileData.MaxWorldCoords.x), 0f, tileData.IncidentRand.Next(tileData.OriginWorldCoords.y, tileData.MaxWorldCoords.y));
            Grid3 key2 = vector.ToGrid();
            if (tileData.triggerPosition != Vector3.zero && !TileData.Incidents.ContainsKey(key2) && (!terrainFeatureIncident.ContainStructures || !TileSystem.PlayerEnteredTiles.Contains(tileData.tileCoord)))
            {
                Incident incident = new(InventoryManager.ParentHuman, vector)
                {
                    ThisMissionData = value[index],
                    ContainsStructures = terrainFeatureIncident.ContainStructures
                };
                int delay = (terrainFeatureIncident.MinDelay < terrainFeatureIncident.MaxDelay) ? tileData.IncidentRand.Next(terrainFeatureIncident.MinDelay, terrainFeatureIncident.MaxDelay) : 0;
                DelayIncidentAsync(tileSystem, tileData, incident, delay, terrainFeatureIncident).Forget();

                if (!terrainFeatureIncident.IsRepeating)
                {
                    TileData.Incidents.Add(key2, incident);
                }

                if (terrainFeatureIncident.ContainStructures)
                {
                    TileData.IncidentsToNotSave.Add(tileData.tileCoord);
                    tileSystem.WorldContainsStructures = true;
                }
            }
        }

        return false;
    }

    public static async UniTask DelayIncidentAsync(TileSystem tileSystem, TileData tileData, Incident incident, int delay, WorldManager.TerrainFeatureIncident relatedValues)
    {
        await UniTask.Delay(delay * 1000);
        if (relatedValues.CanLaunchOutsideTile)
        {
            _ = Incident.LaunchIncident(incident);
        }
    }
}