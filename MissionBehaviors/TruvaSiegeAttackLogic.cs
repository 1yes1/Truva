using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Diamond;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using Truva.CampaignBehaviors;
using Truva.MissionView;
using Truva.ViewModel;
using static TaleWorlds.MountAndBlade.FormationAI;

namespace Truva.MissionBehaviors
{
    public class TruvaSiegeAttackLogic : MissionLogic
    {
        public enum TruvaAttackType
        {
            None,
            Charge,
            ToSiegeWeapons,
            ToRightWalls,
            ToLeftWalls,
            ToGate,
            Fallback,
        }

        private TruvaAttackType _currentAttackType = TruvaAttackType.None;

        private bool _isArrivedToWallPosition = false;

        private bool _isDeploymentFinished = false;

        private bool _isRetreating = false;

        private TruvaSiegeMissionView _truvaSiegeMissionView;

        private CastleGate _targetCastleGate;

        private SiegeWeapon _targetSiegeWeapon;

        private Formation _truvaFormation;

        private WorldPosition _targetWallPosition;

        private IEnumerable<SiegeWeapon> _siegeWeapons;

        private Tuple<WorldPosition,WorldPosition> _wallPositions;

        private IEnumerable<CastleGate> _castleGates;

        private TruvaAttackType _fallbackingFor;

        private List<Agent> _agents;

        public TruvaInputManager InputManager { get; private set; }

        private TruvaSiegeLogic TruvaSiegeLogic { get; set; }

        private bool IsTruvaTroopSiegeBattle { get => (Mission.Current.IsSiegeBattle || TruvaSiegeLogic != null || TruvaSiegeLogic.HasTruvaTroop); }

        public TruvaSiegeAttackLogic(TruvaSiegeMissionView truvaSiegeMissionView)
        {
            _truvaSiegeMissionView = truvaSiegeMissionView;
        }

        public override void AfterStart()
        {

            TruvaSiegeLogic = Mission.GetMissionBehavior<TruvaSiegeLogic>();

            InputManager = Mission.GetMissionBehavior<TruvaInputManager>();
        }

        public override void OnDeploymentFinished()
        {
            if (!IsTruvaTroopSiegeBattle)
                return;

            _isDeploymentFinished = true;
            _siegeWeapons = TruvaMissionHelper.GetSiegeWeapons();
            _wallPositions = TruvaMissionHelper.GetWallPositions();
            _castleGates = TruvaMissionHelper.GetCastleGates();

            //InputManager.OnAttackKeysPressed -= StartAttack;
            //InputManager.OnAttackKeysPressed += StartAttack;

            TruvaSiegeLogic.OnTruvaTroopsSpawnedEvent += OnTruvaTroopsSpawned;
            _truvaSiegeMissionView.OnAttackTypeSelectedEvent += StartAttack;

        }

        public override void OnMissionTick(float dt)
        {
            if (!IsTruvaTroopSiegeBattle || !TruvaSiegeLogic.IsTruvaTroopSpawned)
                return;
            
            if (!_isArrivedToWallPosition && _isDeploymentFinished && _targetWallPosition.IsValid)
            {
                Vec3 upVec = _targetWallPosition.GetGroundVec3();
                upVec.z = 10;
                float distanceToWall = upVec.Distance(_truvaFormation.GetAveragePositionOfUnits(true, true).ToVec3());
                if (distanceToWall < 23)
                {
                    _isArrivedToWallPosition = true;
                    StartAttack(TruvaAttackType.Charge);
                }
            }

            if (_isRetreating)
            {
                float distanceToRetreatPos = TruvaSiegeLogic.TruvaSpawnFrame.origin.AsVec2.Distance(_truvaFormation.GetAveragePositionOfUnits(true, true));

                if (_fallbackingFor == TruvaAttackType.ToGate && distanceToRetreatPos < 25)
                {
                    //InformationManager.DisplayMessage(new InformationMessage("Retreat OK! Now Attack To The Gate ", Colors.Red));
                    _currentAttackType = TruvaAttackType.None;
                    StartAttack(TruvaAttackType.ToGate);
                    _isRetreating = false;
                    _fallbackingFor = TruvaAttackType.None;
                }
                else if (distanceToRetreatPos < 5)
                {
                    _isRetreating = false;
                }
            }
        }

        private void OnTruvaTroopsSpawned(List<Agent> agents, Formation formation)
        {
            _agents = agents;

            _truvaFormation = formation;

            TruvaMissionHelper.GetTargetCastleGate(_castleGates, _truvaFormation.Captain, OnCastleGateDestroyed, out _targetCastleGate);
        }

        private void DisableAgentTargets()
        {
            _targetWallPosition = WorldPosition.Invalid;
            _targetSiegeWeapon = null;
            for (int i = 0; i < _agents.Count; i++)
            {
                Agent agent = _agents[i];
                agent.AIMoveToGameObjectDisable();
                agent.DisableScriptedCombatMovement();
                agent.DisableScriptedMovement();
            }
        }

        public void StartAttack(TruvaAttackType attackType)
        {
            if (!TruvaSiegeLogic.IsTruvaTroopSpawned || TruvaSiegeLogic.IsBattleResultReady)
                return;

            if (TruvaSiegeLogic.IsTruvaDestroyed)
            {
                TruvaSiegeLogic.ShowMissionMessage("Truva Troop Destroyed!");
                return;
            }

            DisableAgentTargets();

            if (attackType == TruvaAttackType.Charge)
                Charge();
            else if (attackType == TruvaAttackType.ToSiegeWeapons)
                AttackToSiegeWeapons();
            else if (attackType == TruvaAttackType.ToRightWalls)
                AttackToWalls(FormationAI.BehaviorSide.Right);
            else if (attackType == TruvaAttackType.ToLeftWalls)
                AttackToWalls(FormationAI.BehaviorSide.Left);
            else if (attackType == TruvaAttackType.ToGate)
                AttackToGate();
            else if (attackType == TruvaAttackType.Fallback)
                Fallback();
        }

        private void AttackToSiegeWeapons()
        {
            TruvaMissionHelper.GetTargetSiegeWeapon(_siegeWeapons, _truvaFormation.GetAveragePositionOfUnits(true, true).ToVec3(), OnSiegeMachineDestroyed, out _targetSiegeWeapon);

            if (_targetSiegeWeapon != null)
            {
                _currentAttackType = TruvaAttackType.ToSiegeWeapons;

                WorldPosition worldPosition = _targetSiegeWeapon.GameEntity.GlobalPosition.ToWorldPosition();
                for (int i = 0; i < _agents.Count; i++)
                {
                    _agents[i].SetScriptedTargetEntityAndPosition(_targetSiegeWeapon.GameEntity, worldPosition, Agent.AISpecialCombatModeFlags.AttackEntity);
                }

                TruvaSiegeLogic.ShowMissionMessage("Truva Troop is attacking to the Siege Weapons!");
            }
            else
                StartAttack(TruvaAttackType.Charge);
        }

        private void AttackToWalls(FormationAI.BehaviorSide behaviorSide)
        {
            TruvaMissionHelper.GetTargetWallPositions(_wallPositions, behaviorSide, out _targetWallPosition);

            if (_targetWallPosition.IsValid)
            {
                if (behaviorSide == FormationAI.BehaviorSide.Left)
                    _currentAttackType = TruvaAttackType.ToLeftWalls;
                else
                    _currentAttackType = TruvaAttackType.ToRightWalls;

                WorldPosition worldPosition = _targetWallPosition;

                _isArrivedToWallPosition = false;

                for (int i = 0; i < _agents.Count; i++)
                    _agents[i].SetScriptedPosition(ref worldPosition, false, Agent.AIScriptedFrameFlags.GoToPosition);

                TruvaSiegeLogic.ShowMissionMessage("Truva Troop is attacking to the " + behaviorSide.ToString() + " Wall!");
            }
            else
            {
                StartAttack(TruvaAttackType.Charge);
            }
        }

        private void AttackToGate()
        {
            if (_targetCastleGate != null)
            {
                //Eğer zaten saldırıyorsa herhangi bir yere duvar üstündeki bugları önlemek için önce geri çekicez sonra saldırtcaz
                if (_currentAttackType != TruvaAttackType.None)
                {
                    Fallback(TruvaAttackType.ToGate);
                    return;
                }

                _currentAttackType = TruvaAttackType.ToGate;

                //WorldPosition worldPosition = _targetCastleGate.GameEntity.GlobalPosition.AsVec2.ToVec3().ToWorldPosition();
                WorldPosition worldPosition = _targetCastleGate.StandingPoints[0].GameEntityWithWorldPosition.WorldPosition.GetGroundVec3().ToWorldPosition();

                for (int i = 0; i < _agents.Count; i++)
                    _agents[i].SetScriptedTargetEntityAndPosition(_targetCastleGate.GameEntity, worldPosition, Agent.AISpecialCombatModeFlags.SurroundAttackEntity);

                TruvaSiegeLogic.ShowMissionMessage("Truva Troop is attacking to the Gate!");
            }
            else
                StartAttack(TruvaAttackType.Charge);
        }

        private void Charge()
        {
            _truvaFormation.SetMovementOrder(MovementOrder.MovementOrderCharge);
            _currentAttackType = TruvaAttackType.Charge;

            TruvaSiegeLogic.ShowMissionMessage("Truva Troop is Charging!");
        }

        private void Fallback(TruvaAttackType fallbackingFor = TruvaAttackType.None)
        {
            WorldPosition worldPosition = TruvaSiegeLogic.TruvaSpawnFrame.origin.ToWorldPosition();
            for (int i = 0; i < _agents.Count; i++)
            {
                _agents[i].SetScriptedPosition(ref worldPosition, false, Agent.AIScriptedFrameFlags.GoToPosition);
            }

            _isRetreating = true;

            if (_fallbackingFor == TruvaAttackType.None)
                TruvaSiegeLogic.ShowMissionMessage("Truva Troops is Falling Back!");

            _fallbackingFor = fallbackingFor;
        }


        private void OnSiegeMachineDestroyed(DestructableComponent target, Agent attackerAgent, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, int inflictedDamage)
        {
            if (_targetSiegeWeapon.DestructionComponent == target && _targetSiegeWeapon.DestructionComponent.IsDestroyed)
            {
                //InformationManager.DisplayMessage(new InformationMessage("Siege Machine Destroyed!!", Colors.Red));
                StartAttack(TruvaAttackType.ToSiegeWeapons);
            }
        }

        private void OnCastleGateDestroyed(DestructableComponent target, Agent attackerAgent, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, int inflictedDamage)
        {
            if (_targetCastleGate.DestructionComponent == target && _targetCastleGate.DestructionComponent.IsDestroyed)
            {
                //InformationManager.DisplayMessage(new InformationMessage("Castle Gate Destroyed!!", Colors.Red));
                StartAttack(TruvaAttackType.Charge);
            }
        }

    }
}
