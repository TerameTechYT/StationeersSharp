using Assets.Scripts;
using System.Collections;
using System.IO;
using System.Reflection;

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

    public static IEnumerator DelayIncident(TileData tileData, Incident incident, int delay, WorldManager.TerrainFeatureIncident relatedValues)
    {
        yield return Yielders.WaitForSeconds(delay);
        // code being removed:
        //if (tileData.IsCenterTile)
        //{
        //    _ = Incident.LaunchIncident(incident);
        //    if (relatedValues.IsRepeating)
        //    {
        //        TileSystem.Instance.GenerateRandomIncident(tileData, false);
        //    }
        //}
        //else
        if (relatedValues.CanLaunchOutsideTile)
        {
            _ = Incident.LaunchIncident(incident);
        }
        yield break;
    }
}