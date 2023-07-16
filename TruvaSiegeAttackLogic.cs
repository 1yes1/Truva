using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using Truva.MissionView;

namespace Truva
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

        private bool _isArrivedToLadder = false;

        private bool _isDeploymentFinished = false;

        private bool _isRetreating = false;

        private TruvaSiegeMissionView _truvaSiegeMissionView;

        private CastleGate _targetCastleGate;

        private SiegeWeapon _targetSiegeWeapon;

        private SiegeLadder _targetSiegeLadder;

        private Formation _truvaFormation;

        private WorldPosition _targetSiegeLadderPosition;

        private IEnumerable<SiegeWeapon> _siegeWeapons;

        private IEnumerable<SiegeLadder> _siegeLadders;

        private IEnumerable<CastleGate> _castleGates;

        private TruvaAttackType _fallbackingFor;

        private List<Agent> _agents;

        public TruvaInputManager InputManager { get; private set; }

        private TruvaSiegeLogic TruvaSiegeLogic { get; set; }

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
            _isDeploymentFinished = true;
            _siegeWeapons = TruvaMissionHelper.GetSiegeWeapons();
            _siegeLadders = TruvaMissionHelper.GetSiegeLadders();
            _castleGates = TruvaMissionHelper.GetCastleGates();

            //InputManager.OnAttackKeysPressed -= StartAttack;
            //InputManager.OnAttackKeysPressed += StartAttack;
            
            TruvaSiegeLogic.OnTruvaTroopsSpawnedEvent += OnTruvaTroopsSpawned;
            _truvaSiegeMissionView.OnAttackTypeSelectedEvent += StartAttack;

        }

        public override void OnMissionTick(float dt)
        {
            if (!Mission.Current.IsSiegeBattle || TruvaSiegeLogic == null)
                return;

            if (!_isArrivedToLadder && TruvaSiegeLogic.IsTruvaTroopSpawned && _isDeploymentFinished && _targetSiegeLadder != null)
            {
                float distanceToLadder = _targetSiegeLadderPosition.AsVec2.Distance(_truvaFormation.GetAveragePositionOfUnits(true, true));
                if (distanceToLadder < 20)
                {
                    _isArrivedToLadder = true;
                    //InformationManager.DisplayMessage(new InformationMessage("Arrived To Ladders !!! ", Colors.Magenta));
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
                    DisableAgentTargets();
                }

            }

        }

        private void OnTruvaTroopsSpawned(List<Agent> agents,Formation formation)
        {
            _agents = agents;

            _truvaFormation = formation;

            TruvaMissionHelper.GetTargetCastleGate(_castleGates, _truvaFormation.Captain, OnCastleGateDestroyed, out _targetCastleGate);
        }

        private void DisableAgentTargets()
        {
            //_targetCastleGate = null;
            _targetSiegeLadder = null;
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
            if (!TruvaSiegeLogic.IsTruvaTroopSpawned)
                return;

            DisableAgentTargets();

            if (attackType == TruvaAttackType.Charge)
                Charge();
            else if (attackType == TruvaAttackType.ToSiegeWeapons)
                AttackToSiegeWeapons();
            else if (attackType == TruvaAttackType.ToRightWalls)
                AttackToLadders(FormationAI.BehaviorSide.Right);
            else if (attackType == TruvaAttackType.ToLeftWalls)
                AttackToLadders(FormationAI.BehaviorSide.Left);
            else if (attackType == TruvaAttackType.ToGate)
                AttackToGate();
            else if (attackType == TruvaAttackType.Fallback)
                Fallback();

        }

        private void AttackToSiegeWeapons()
        {
            TruvaMissionHelper.GetTargetSiegeWeapon(_siegeWeapons, OnSiegeMachineDestroyed, out _targetSiegeWeapon);

            if (_targetSiegeWeapon != null)
            {
                _currentAttackType = TruvaAttackType.ToSiegeWeapons;

                WorldPosition worldPosition = _targetSiegeWeapon.GameEntity.GlobalPosition.ToWorldPosition();
                for (int i = 0; i < _agents.Count; i++)
                {
                    _agents[i].SetScriptedTargetEntityAndPosition(_targetSiegeWeapon.GameEntity, worldPosition, Agent.AISpecialCombatModeFlags.AttackEntity);
                }
                InformationManager.DisplayMessage(new InformationMessage("Attack To The Siege Weapons!!", Colors.Red));
            }
            else
                StartAttack(TruvaAttackType.Charge);
        }

        private void AttackToLadders(FormationAI.BehaviorSide behaviorSide)
        {   
            TruvaMissionHelper.GetTargetSiegeLadder(_siegeLadders, behaviorSide, out _targetSiegeLadder);

            if (_targetSiegeLadder != null)
            {
                if (behaviorSide == FormationAI.BehaviorSide.Left)
                    _currentAttackType = TruvaAttackType.ToLeftWalls;
                else
                    _currentAttackType = TruvaAttackType.ToRightWalls;

                WorldFrame worldFrame = _targetSiegeLadder.GetTargetStandingPointOfAIAgent(null).GetUserFrameForAgent(null);

                WorldPosition worldPosition = worldFrame.Origin;
                _targetSiegeLadderPosition = worldPosition;

                for (int i = 0; i < _agents.Count; i++)
                    _agents[i].SetScriptedPosition(ref worldPosition, false, Agent.AIScriptedFrameFlags.GoToPosition);

                InformationManager.DisplayMessage(new InformationMessage("Attack To The " + behaviorSide.ToString() + " Wall", Colors.Red));

            }
            else
                StartAttack(TruvaAttackType.Charge);
        }

        private void AttackToGate()
        {
            if (_targetCastleGate != null)
            {
                InformationManager.DisplayMessage(new InformationMessage("Attack To The Gate!!", Colors.Red));

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
            }
            else
                StartAttack(TruvaAttackType.Charge);
        }

        private void Charge()
        {
            InformationManager.DisplayMessage(new InformationMessage("Charge!!", Colors.Red));
            _truvaFormation.SetMovementOrder(MovementOrder.MovementOrderCharge);
            _currentAttackType = TruvaAttackType.Charge;
        }

        private void Fallback(TruvaAttackType fallbackingFor = TruvaAttackType.None)
        {
            WorldPosition worldPosition = TruvaSiegeLogic.TruvaSpawnFrame.origin.ToWorldPosition();
            for (int i = 0; i < _agents.Count; i++)
            {
                _agents[i].SetScriptedPosition(ref worldPosition, false, Agent.AIScriptedFrameFlags.GoToPosition);
            }
            InformationManager.DisplayMessage(new InformationMessage("Retreat!!", Colors.Red));
            _isRetreating = true;

            if (_fallbackingFor != TruvaAttackType.None)
                InformationManager.DisplayMessage(new InformationMessage("Retreating For " + _fallbackingFor.ToString(), Colors.Red));

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
