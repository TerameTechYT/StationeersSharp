#region

using Cysharp.Threading.Tasks;
using System.Security.Cryptography;
using UnityEngine;

#endregion

namespace LoulanRevived;

internal class Functions {
    public static void LoadIncidents(ref WorldManager worldManager) {
        string[] directories = Directory.GetDirectories(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "Incidents"));

        foreach (string directory in directories) {
            MissionData incidentData = Incident.GetIncident(new DirectoryInfo(directory));

            Incident.LoadIncident(incidentData);
            Plugin.LogInfo($"Loaded incident {incidentData.FolderName}");
        }
    }

    public static void GenerateRandomIncident(ref TileSystem tileSystem, TileData tileData, bool onTileEnter) {
        if (tileSystem.CurrentIncidentTypes.Count == 0) {
            return;
        }

        int key = tileSystem.CurrentIncidentTypes.ElementAt(RandomNumberGenerator.GetInt32(0, tileSystem.CurrentIncidentTypes.Count)).Key;
        WorldManager.TerrainFeatureIncident featureIncident = tileSystem.CurrentIncidentTypes[key];
        if (!Incident.CachedGridIncidents.TryGetValue(featureIncident.Type, out List<MissionData> missionData) || missionData.Count == 0) {
            return;
        }

        if (onTileEnter && !featureIncident.RunOnTileEnter) {
            return;
        }

        if (featureIncident.RequiresHumanInTile && !tileData.IsCenterTile) {
            return;
        }

        for (int i = 0; i < RandomNumberGenerator.GetInt32(0, featureIncident.MaxPerTile + 1); i++) {
            if (RandomNumberGenerator.GetInt32(0, 10000) > featureIncident.SpawnChance) {
                continue;
            }

            float x = RandomNumberGenerator.GetInt32(tileData.OriginWorldCoords.x, tileData.MaxWorldCoords.x);
            float z = RandomNumberGenerator.GetInt32(tileData.OriginWorldCoords.y, tileData.MaxWorldCoords.y);
            Vector3 vector = new(x, 0f, z);

            if (tileData.triggerPosition != Vector3.zero &&
                !TileData.Incidents.ContainsKey(vector.ToGrid()) &&
                (!featureIncident.ContainStructures || !TileSystem.PlayerEnteredTiles.Contains(tileData.tileCoord))) {

                int index = RandomNumberGenerator.GetInt32(0, missionData.Count);
                Incident incident = new(InventoryManager.ParentHuman, vector) {
                    ThisMissionData = missionData[index],
                    ContainsStructures = featureIncident.ContainStructures
                };

                int delay = (featureIncident.MinDelay < featureIncident.MaxDelay) ? RandomNumberGenerator.GetInt32(featureIncident.MinDelay, featureIncident.MaxDelay) : 0;

                DelayIncidentAsync(tileSystem, tileData, incident, delay, featureIncident).Forget();

                if (!featureIncident.IsRepeating) {
                    TileData.Incidents.Add(vector.ToGrid(), incident);
                }

                /*if (featureIncident.ContainStructures) {
                    TileData.IncidentsToNotSave.Add(tileData.tileCoord);
                    tileSystem.WorldContainsStructures = true;
                }*/
            }
        }
    }

    public static async UniTask DelayIncidentAsync(TileSystem tileSystem, TileData tileData, Incident incident, int delay, WorldManager.TerrainFeatureIncident relatedValues) {
        await UniTask.Delay(delay * 1000);

        Plugin.LogWarning($"Spawning wrecked building at x={incident.customPosition.x} z={incident.customPosition.z}");

        if (tileData.IsCenterTile) {
            Incident.LaunchIncident(incident);
        }

        if (relatedValues.CanLaunchOutsideTile) {
            _ = Incident.LaunchIncident(incident);
        }
        Plugin.LogWarning($"Spawned wrecked building");
    }
}

internal static class Extensions {
    internal static bool OpcodeIs(this CodeInstruction code, OpCode opcode) => code.opcode == opcode;

    internal static bool InstructionIs(this CodeInstruction code, OpCode opcode, object operand) => code.OpcodeIs(opcode) && code.OperandIs(operand);
}