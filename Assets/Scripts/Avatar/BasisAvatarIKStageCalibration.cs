using System;
using System.Collections.Generic;
using UnityEngine;
public static partial class BasisAvatarIKStageCalibration
{
    /// <summary>
    ///  = 0.4f;
    /// </summary>
    public static float MaxDistanceBeforeMax(BasisBoneTrackedRole role)
    {

        switch (role)
        {
            case BasisBoneTrackedRole.CenterEye:
                return 0.2f;
            case BasisBoneTrackedRole.Head:
                return 0.2f;
            case BasisBoneTrackedRole.Neck:
                return 0.2f;
            case BasisBoneTrackedRole.Chest:
                return 0.3f;
            case BasisBoneTrackedRole.Hips:
                return 0.3f;
            case BasisBoneTrackedRole.Spine:
                return 0.3f;
            case BasisBoneTrackedRole.LeftUpperLeg:
                return 0.3f;
            case BasisBoneTrackedRole.RightUpperLeg:
                return 0.3f;
            case BasisBoneTrackedRole.LeftLowerLeg:
                return 0.3f;
            case BasisBoneTrackedRole.RightLowerLeg:
                return 0.3f;
            case BasisBoneTrackedRole.LeftFoot:
                return 0.2f;
            case BasisBoneTrackedRole.RightFoot:
                return 0.2f;
            case BasisBoneTrackedRole.UpperChest:
                return 0.3f;
            case BasisBoneTrackedRole.LeftShoulder:
                return 0.2f;
            case BasisBoneTrackedRole.RightShoulder:
                return 0.2f;
            case BasisBoneTrackedRole.LeftUpperArm:
                return 0.3f;
            case BasisBoneTrackedRole.RightUpperArm:
                return 0.3f;
            case BasisBoneTrackedRole.LeftLowerArm:
                return 0.3f;
            case BasisBoneTrackedRole.RightLowerArm:
                return 0.2f;
            case BasisBoneTrackedRole.LeftHand:
                return 0.2f;
            case BasisBoneTrackedRole.RightHand:
                return 0.3f;
            case BasisBoneTrackedRole.LeftToes:
                return 0.2f;
            case BasisBoneTrackedRole.RightToes:
                return 0.2f;
            case BasisBoneTrackedRole.Mouth:
                return 0.3f;
            default:
                Console.WriteLine("Unknown role");
                return 0.4f;
        }
    }

    public static void FullBodyCalibration()
    {
        BasisLocalPlayer.Instance.AvatarDriver.PutAvatarIntoTPose();

        List<BasisBoneTrackedRole> rolesToDiscover = GetAllRoles();
        List<BasisInput> Trackers = GetAllInputsExcludingEyeAndHands(ref rolesToDiscover);
        List<CalibrationConnector> availableBoneControl = GetAvailableBoneControls(Trackers);
        FindOptimalMatches(availableBoneControl, rolesToDiscover);
        BasisLocalPlayer.Instance.AvatarDriver.ResetAvatarAnimator();
        //do the roles after to stop the animator switch issue
        BasisLocalPlayer.Instance.AvatarDriver.CalibrateRoles();
    }
    #region DiscoverWhatsPossible
    private static List<BasisBoneTrackedRole> GetAllRoles()
    {
        List<BasisBoneTrackedRole> rolesToDiscover = new List<BasisBoneTrackedRole>();
        foreach (BasisBoneTrackedRole role in Enum.GetValues(typeof(BasisBoneTrackedRole)))
        {
            rolesToDiscover.Add(role);
        }
        // Create a dictionary for quick index lookup
        Dictionary<BasisBoneTrackedRole, int> orderLookup = new Dictionary<BasisBoneTrackedRole, int>();
        for (int i = 0; i < desiredOrder.Length; i++)
        {
            orderLookup[desiredOrder[i]] = i;
        }

        // Assign a large index value to roles not in the desired order
        int largeIndex = desiredOrder.Length;

        // Sort the list based on the desired order
        rolesToDiscover.Sort((x, y) =>
        {
            int indexX = orderLookup.ContainsKey(x) ? orderLookup[x] : largeIndex;
            int indexY = orderLookup.ContainsKey(y) ? orderLookup[y] : largeIndex;
            return indexX.CompareTo(indexY);
        });

        return rolesToDiscover;
    }
    private static List<BasisInput> GetAllInputsExcludingEyeAndHands(ref List<BasisBoneTrackedRole> rolesToDiscover)
    {
        List<BasisInput> trackInput = new List<BasisInput>();
        foreach (BasisInput baseInput in BasisDeviceManagement.Instance.AllInputDevices)
        {
            if (baseInput.TryGetRole(out BasisBoneTrackedRole role))
            {
                if (BasisBoneTrackedRoleCommonCheck.CheckItsFBTracker(role))
                {
                  //  Debug.Log("Add Tracker that had last role " + role + " with name " + baseInput.name);
                    baseInput.UnAssignFullBodyTrackers();
                    trackInput.Add(baseInput);
                }
                else
                {
                    //Debug.Log("excluding role " + role + " from being used during FB");
                    rolesToDiscover.Remove(role);
                }
            }
            else
            {
               // Debug.Log("Add Tracker with name " + baseInput.name);
                trackInput.Add(baseInput);
            }
        }
        Debug.Log("Completed input tracking");
        return trackInput;
    }
    private static List<CalibrationConnector> GetAvailableBoneControls(List<BasisInput> Trackers)
    {
        List<CalibrationConnector> availableBoneControl = new List<CalibrationConnector>();
        foreach (BasisInput baseInput in Trackers)
        {
            CalibrationConnector calibrationConnector = new CalibrationConnector
            {
                Tracker = baseInput,
                Distance = float.MaxValue
            };
            availableBoneControl.Add(calibrationConnector);
        }
        return availableBoneControl;
    }
    #endregion
    private static void FindOptimalMatches(List<CalibrationConnector> connectors, List<BasisBoneTrackedRole> rolesToDiscover)
    {
        List<BasisTrackerMapping> boneTransformMappings = new List<BasisTrackerMapping>();
        foreach (BasisBoneTrackedRole role in rolesToDiscover)
        {
            if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out BasisBoneControl control, role))
            {
                float scaler = MaxDistanceBeforeMax(role) * BasisLocalPlayer.Instance.RatioAvatarToAvatarEyeDefaultScale;
                BasisTrackerMapping mapping = new BasisTrackerMapping(control, role, connectors, scaler);
                boneTransformMappings.Add(mapping);
            }
            else
            {
                Debug.LogError("Missing bone control for role " + role);
            }
        }
        List<BasisBoneTrackedRole> roles = new List<BasisBoneTrackedRole>();
        List<BasisTrackerMapping> maps = new List<BasisTrackerMapping>();
        List<BasisInput> BasisInputs = new List<BasisInput>();
        // Find optimal matches
        for (int Index = 0; Index < boneTransformMappings.Count; Index++)
        {
            BasisTrackerMapping mapping = boneTransformMappings[Index];
            if (mapping.TargetControl != null)
            {
                RunThroughConnectors(mapping,ref BasisInputs, ref roles,ref maps);
            }
            else
            {
                Debug.LogError("Missing Tracker for index " + Index + " with ID " + mapping);
            }
        }
    }
    public static void RunThroughConnectors(BasisTrackerMapping mapping,ref List<BasisInput> BasisInputs, ref List<BasisBoneTrackedRole> roles, ref List<BasisTrackerMapping> maps)
    {
        foreach (CalibrationConnector Connector in mapping.Candidates)
        {
            if (roles.Contains(mapping.BasisBoneControlRole) == false && maps.Contains(mapping) == false && BasisInputs.Contains(Connector.Tracker) == false)
            {
              //  Debug.Log("Apply Tracked Data for " + mapping.BasisBoneControlRole + " attached to tracker " + Connector.Tracker.UniqueDeviceIdentifier);
                Connector.Tracker.ApplyTrackerCalibration(mapping.BasisBoneControlRole);
                roles.Add(mapping.BasisBoneControlRole);
                maps.Add(mapping);
                BasisInputs.Add(Connector.Tracker);
                break;
            }
        }
    }
}