using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects.Siege;
using Truva.CampaignBehaviors;
using static TaleWorlds.MountAndBlade.DestructableComponent;
using static TaleWorlds.MountAndBlade.FormationAI;

namespace Truva.MissionBehaviors
{
    public class TruvaMissionHelper
    {
        public static TruvaTroop TruvaTroop { get => TruvaHelper.FindTruvaTroop(Settlement.CurrentSettlement.StringId); }

        public static List<SiegeWeapon> GetSiegeWeapons()
        {
            List<MissionObject> missionObjects = Mission.Current.ActiveMissionObjects;

            List<SiegeWeapon> siegeWeaponsList = new List<SiegeWeapon>();

            for (int i = 0; i < missionObjects.Count; i++)
            {
                if (missionObjects[i] is RangedSiegeWeapon)
                {
                    SiegeWeapon siegeWeapon = (SiegeWeapon)missionObjects[i];

                    if(siegeWeapon.GetSide() == TaleWorlds.Core.BattleSideEnum.Defender)
                         siegeWeaponsList.Add(siegeWeapon);
                }
            }

            return siegeWeaponsList;
        }


        public static Tuple<WorldPosition,WorldPosition> GetWallPositions()
        {
            List<MissionObject> missionObjects = Mission.Current.ActiveMissionObjects;
            WorldPosition posRight = WorldPosition.Invalid;
            WorldPosition posLeft = WorldPosition.Invalid;
            WorldPosition posMiddle = WorldPosition.Invalid;

            foreach (MissionObject missionObject in missionObjects)
            {
                if(missionObject is SiegeLadder)
                {
                    SiegeLadder siegeLadder = (SiegeLadder)missionObject;
                    if(siegeLadder.WeaponSide == BehaviorSide.Right)
                        posRight = siegeLadder.GetTargetStandingPointOfAIAgent(null).GetUserFrameForAgent(null).Origin;
                    else if(siegeLadder.WeaponSide == BehaviorSide.Left)
                        posLeft = siegeLadder.GetTargetStandingPointOfAIAgent(null).GetUserFrameForAgent(null).Origin;
                    else if(siegeLadder.WeaponSide == BehaviorSide.Middle || siegeLadder.WeaponSide == BehaviorSide.BehaviorSideNotSet)
                    {
                        posMiddle = siegeLadder.GetTargetStandingPointOfAIAgent(null).GetUserFrameForAgent(null).Origin;
                    }

                }
                else if (missionObject is SiegeTower)
                {
                    SiegeTower siegeTower = (SiegeTower)missionObject;
                    if (siegeTower.WeaponSide == BehaviorSide.Right)
                        posRight = siegeTower.MovementComponent.GetTargetFrame().origin.ToWorldPosition();
                    else if (siegeTower.WeaponSide == BehaviorSide.Left)
                        posLeft = siegeTower.MovementComponent.GetTargetFrame().origin.ToWorldPosition();
                    else if (siegeTower.WeaponSide == BehaviorSide.Middle || siegeTower.WeaponSide == BehaviorSide.BehaviorSideNotSet)
                    {
                        posMiddle = siegeTower.MovementComponent.GetTargetFrame().origin.ToWorldPosition();
                    }

                }
            }

            if (!posLeft.IsValid)
                posLeft = posMiddle;
            else if(!posRight.IsValid)
                posRight = posMiddle;

            Tuple<WorldPosition, WorldPosition> wallPositions = new Tuple<WorldPosition, WorldPosition>(posLeft, posRight);

            //InformationManager.DisplayMessage(new InformationMessage("Item1: " + wallPositions.Item1, Colors.Red));
            //InformationManager.DisplayMessage(new InformationMessage("Item2: " + wallPositions.Item2, Colors.Red));


            return wallPositions;
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
                }
            }
            return castleGatesList;
        }


        public static void GetTargetSiegeWeapon(IEnumerable<SiegeWeapon> siegeWeapons, Vec3 formationPos, OnHitTakenAndDestroyedDelegate onHitTakenAndDestroyedDelegate, out SiegeWeapon targetSiegeWeapon)
        {
            targetSiegeWeapon = null;

            if (siegeWeapons.Count() == 0)
                return;

            IEnumerator<SiegeWeapon> enumerator = siegeWeapons.GetEnumerator();

            float minDistance = float.MaxValue;
            SiegeWeapon tempWeapon = null;

            while (enumerator.MoveNext())
            {
                tempWeapon = enumerator.Current;

                if (tempWeapon.DestructionComponent == null)
                    continue;

                if (!tempWeapon.DestructionComponent.IsDestroyed && tempWeapon.GameEntity.GlobalPosition.Distance(formationPos) < minDistance)
                {
                    minDistance = tempWeapon.GameEntity.GlobalPosition.Distance(formationPos);
                    targetSiegeWeapon = tempWeapon;
                }
            }

            if (minDistance != float.MaxValue)
            {
                targetSiegeWeapon.DestructionComponent.OnDestroyed -= onHitTakenAndDestroyedDelegate;
                targetSiegeWeapon.DestructionComponent.OnDestroyed += onHitTakenAndDestroyedDelegate;
            }
            else
                targetSiegeWeapon = null;
        }


        public static void GetTargetWallPositions(Tuple<WorldPosition, WorldPosition> siegeLadders, BehaviorSide behaviorSide, out WorldPosition targetWallPosition)
        {
            if (behaviorSide == BehaviorSide.Right)
            {
                targetWallPosition = siegeLadders.Item2;
                return;
            }
            else if(behaviorSide == BehaviorSide.Left)
            {
                targetWallPosition = siegeLadders.Item1;
                return;
            }
            else
                targetWallPosition = WorldPosition.Invalid;

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
