using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.MountAndBlade.DestructableComponent;
using static TaleWorlds.MountAndBlade.FormationAI;

namespace Truva
{
    public class TruvaMissionHelper
    {
        public static List<SiegeWeapon> GetSiegeWeapons()
        {
            List<MissionObject> missionObjects = Mission.Current.ActiveMissionObjects;

            List<SiegeWeapon> siegeWeaponsList = new List<SiegeWeapon>();

            for (int i = 0; i < missionObjects.Count; i++)
            {
                if (missionObjects[i] is RangedSiegeWeapon)
                {
                    SiegeWeapon siegeWeapon = (SiegeWeapon)missionObjects[i];

                    siegeWeaponsList.Add(siegeWeapon);

                    //InformationManager.DisplayMessage(new InformationMessage("Siege Machine Found: " + siegeWeapon.GameEntity.Name, Colors.Red));
                }
            }

            return siegeWeaponsList;
        }

        public static List<SiegeLadder> GetSiegeLadders()
        {
            List<MissionObject> missionObjects = Mission.Current.ActiveMissionObjects;

            List<SiegeLadder> siegeLaddersList = new List<SiegeLadder>();

            for (int i = 0; i < missionObjects.Count; i++)
            {
                if (missionObjects[i] is SiegeLadder)
                {
                    SiegeLadder siegeLadder = (SiegeLadder)missionObjects[i];

                    siegeLaddersList.Add(siegeLadder);

                    //InformationManager.DisplayMessage(new InformationMessage("Siege Ladder Found: " + siegeLadder.GameEntity.Name, Colors.Yellow));
                }
            }

            return siegeLaddersList;
        }


        public static List<CastleGate> GetCastleGates()
        {
            List<MissionObject> missionObjects = Mission.Current.ActiveMissionObjects;

            List<CastleGate> castleGatesList = new List<CastleGate>();

            for (int i = 0; i < missionObjects.Count; i++)
            {
                if (missionObjects[i] is CastleGate)
                {
                    castleGatesList.Add((CastleGate)missionObjects[i]);

                    //InformationManager.DisplayMessage(new InformationMessage("Castle Gate Found !!: " + missionObjects[i].GameEntity.Name, Colors.Blue));
                }
            }
            return castleGatesList;
        }



        public static void GetTargetSiegeWeapon(IEnumerable<SiegeWeapon> siegeWeapons, OnHitTakenAndDestroyedDelegate onHitTakenAndDestroyedDelegate, out SiegeWeapon targetSiegeWeapon)
        {
            if (siegeWeapons.Count() == 0)
            {
                targetSiegeWeapon = null;
                return;
            }
            IEnumerator<SiegeWeapon> enumerator = siegeWeapons.GetEnumerator();

            while (enumerator.MoveNext())
            {
                targetSiegeWeapon = enumerator.Current;

                if (targetSiegeWeapon.DestructionComponent == null)
                    continue;

                if (!targetSiegeWeapon.DestructionComponent.IsDestroyed)
                {
                    targetSiegeWeapon.DestructionComponent.OnDestroyed -= onHitTakenAndDestroyedDelegate;
                    targetSiegeWeapon.DestructionComponent.OnDestroyed += onHitTakenAndDestroyedDelegate;
                    return;
                }
            }
            targetSiegeWeapon = null;
        }


        public static void GetTargetSiegeLadder(IEnumerable<SiegeLadder> siegeLadders, FormationAI.BehaviorSide behaviorSide, out SiegeLadder targetSiegeLadder)
        {
            if (siegeLadders.Count() == 0)
            {
                targetSiegeLadder = null;
                return;
            }

            IEnumerator<SiegeLadder> enumerator = siegeLadders.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (behaviorSide == enumerator.Current.WeaponSide)
                {
                    targetSiegeLadder = enumerator.Current;
                    return;
                }
            }
            targetSiegeLadder = null;
        }


        public static void GetTargetCastleGate(IEnumerable<CastleGate> castleGates, Agent agentToDistance, OnHitTakenAndDestroyedDelegate onHitTakenAndDestroyedDelegate, out CastleGate castleGate)
        {
            castleGate = null;

            if (castleGates.Count() == 0)
                return;

            IEnumerator<CastleGate> enumerator = castleGates.GetEnumerator();

            float minDistance = float.MaxValue;

            CastleGate tempCastleGate = null;

            while (enumerator.MoveNext())
            {
                tempCastleGate = enumerator.Current;

                if (!tempCastleGate.DestructionComponent.IsDestroyed && tempCastleGate.GetPosition().Distance(agentToDistance.Position) < minDistance)
                {
                    minDistance = tempCastleGate.GetPosition().Distance(agentToDistance.Position);
                    //InformationManager.DisplayMessage(new InformationMessage("Castle : "+tempCastleGate.GameEntity.Name+ " :" + minDistance, Colors.Blue));
                    //InformationManager.DisplayMessage(new InformationMessage("Castle : " + tempCastleGate.GameEntity.Name + " :" + tempCastleGate.MiddlePosition.TacticalRegionMembership.ToString(), Colors.Blue));
                    castleGate = tempCastleGate;
                }
            }

            if (minDistance != float.MaxValue)
            {
                castleGate.DestructionComponent.OnDestroyed -= onHitTakenAndDestroyedDelegate;
                castleGate.DestructionComponent.OnDestroyed += onHitTakenAndDestroyedDelegate;
            }
            else
                castleGate = null;



        }

    }
}
