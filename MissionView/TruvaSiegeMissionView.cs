using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.TwoDimension;
using Truva.MissionBehaviors;
using static Truva.MissionBehaviors.TruvaSiegeAttackLogic;

namespace Truva.MissionView
{
    public class TruvaSiegeMissionView : TaleWorlds.MountAndBlade.View.MissionViews.MissionView
    {
        public event Action<TruvaAttackType> OnAttackTypeSelectedEvent;

        public event Action<bool> OnTruvaSpawnChooseEvent;

        private float _angle;

        private bool _isActive = false;

        private bool _spawnTroop = false;

        private TruvaSiegeVM _dataSource;

        private GauntletLayer _gauntletLayer;

        private IGauntletMovie _movie;

        private TruvaInputManager _truvaInputManager;

        private TruvaSiegeLogic _truvaSiegeLogic;

        private TruvaAttackType _truvaAttackType;

        private Vec2 mouseStartPos = Vec2.Zero;

        private MatrixFrame _matrixFrame = MatrixFrame.Zero;


        public TruvaSiegeMissionView(TruvaInputManager truvaInputManager,TruvaSiegeLogic truvaSiegeLogic)
        {
            _truvaInputManager = truvaInputManager;
            _truvaSiegeLogic = truvaSiegeLogic;
        }

        public override void OnDeploymentFinished()
        {
            //Yani bu yerleşkede truva troop yoksa UI görünmesin
            if (!_truvaSiegeLogic.HasTruvaTroop)
                return;

            _truvaInputManager.OnChooseAttackTypeKeyPressing += ToggleLayer;
        }

        public void AddLayer()
        {
            if (_gauntletLayer != null)
                return;

            if (_dataSource == null)
                _dataSource = new TruvaSiegeVM();

            _gauntletLayer = new GauntletLayer(100)
            {
                IsFocusLayer = true
            };
            _movie = _gauntletLayer.LoadMovie("TruvaSiegeHUD", _dataSource);
            _gauntletLayer.InputRestrictions.SetInputRestrictions(false,InputUsageMask.Mouse);
            _gauntletLayer.InputRestrictions.SetMouseVisibility(false);
            MissionScreen.AddLayer(_gauntletLayer);
            //InformationManager.DisplayMessage(new InformationMessage("Pressed!", Colors.Red));
            MakeAllHidden();

        }

        public void RemoveLayer()
        {
            if (_dataSource == null)
                return;

            MissionScreen.RemoveLayer(_gauntletLayer);
            _gauntletLayer = null;
            _dataSource = null;
            _movie = null;
            _matrixFrame = MatrixFrame.Zero;
            //InformationManager.DisplayMessage(new InformationMessage("Released!", Colors.Red));

            if (_truvaSiegeLogic.IsTruvaTroopSpawned)
                OnAttackTypeSelectedEvent?.Invoke(_truvaAttackType);
            else
                OnTruvaSpawnChooseEvent?.Invoke(_spawnTroop);

        }

        public void ToggleLayer(bool _isPressing)
        {
            //if (Agent.Main == null || !Agent.Main.IsActive())
            //    return;

            if (!_isActive)
            {
                _matrixFrame = MissionScreen.CombatCamera.Frame;

                if (Agent.Main != null && Agent.Main.IsActive())
                    _matrixFrame.rotation.u.z = Agent.Main.LookDirection.z;
                else
                    _matrixFrame.rotation.u.z = 0;

            }

            _isActive = _isPressing;

            if (_isActive)
            {
                if (_gauntletLayer == null)//1 defa çalışacak
                    _truvaAttackType = TruvaAttackType.None;

                MissionScreen.UpdateFreeCamera(_matrixFrame);
                AddLayer();
            }
            else 
                RemoveLayer();
        }

        public override void OnMissionTick(float dt)
        {
            if (_gauntletLayer == null)
                return;

            Vec2 mouseDelta = new Vec2(Input.GetMouseMoveX(), Input.GetMouseMoveY());
            //InformationManager.DisplayMessage(new InformationMessage("mouseDelta.Length: " + mouseDelta.Length, Colors.Yellow));

            if (mouseDelta.Length <= 35)
                return;

            _angle = Mathf.Atan2(mouseDelta.y, mouseDelta.x) * Mathf.Rad2Deg;

            if (_angle < 0) 
                _angle += 360;

            //InformationManager.DisplayMessage(new InformationMessage("_angle: " + _angle, Colors.Yellow));

            mouseStartPos = Input.GetMousePositionPixel();

            if(_truvaSiegeLogic.IsTruvaTroopSpawned)
            {
                if (_angle > 0 && _angle <= 60)
                    _truvaAttackType = TruvaAttackType.ToGate;
                else if (_angle > 60 && _angle <= 120)
                    _truvaAttackType = TruvaAttackType.Fallback;
                else if (_angle > 120 && _angle <= 180)
                    _truvaAttackType = TruvaAttackType.ToSiegeWeapons;
                else if (_angle > 180 && _angle <= 240)
                    _truvaAttackType = TruvaAttackType.ToLeftWalls;
                else if (_angle > 240 && _angle <= 300)
                    _truvaAttackType = TruvaAttackType.Charge;
                else if (_angle > 300 && _angle <= 360)
                    _truvaAttackType = TruvaAttackType.ToRightWalls;

                HoverAttackType(_truvaAttackType);
            }
            else
            {
                if (_angle > 0 && _angle <= 180)
                    _spawnTroop = false;
                else if(_angle > 180 && _angle <= 360)
                    _spawnTroop = true;

                HoverSpawnTroopChoose();
            }


        }


        private void MakeAllHidden()
        {
            _dataSource.Type0IsHidden = true;
            _dataSource.Type1IsHidden = true;
            _dataSource.Type2IsHidden = true;
            _dataSource.Type3IsHidden = true;
            _dataSource.Type4IsHidden = true;
            _dataSource.Type5IsHidden = true;
            _dataSource.CallTruvaIsHidden = true;
            _dataSource.CallTruvaCancelIsHidden = true;
        }

        private void HoverAttackType(TruvaAttackType truvaAttackType)
        {
            MakeAllHidden();
            if (truvaAttackType == TruvaAttackType.Charge)
                _dataSource.Type0IsHidden = false;
            else if(truvaAttackType == TruvaAttackType.ToRightWalls)
                _dataSource.Type1IsHidden = false;
            else if (truvaAttackType == TruvaAttackType.ToGate)
                _dataSource.Type2IsHidden = false;
            else if (truvaAttackType == TruvaAttackType.Fallback)
                _dataSource.Type3IsHidden = false;
            else if (truvaAttackType == TruvaAttackType.ToSiegeWeapons)
                _dataSource.Type4IsHidden = false;
            else if (truvaAttackType == TruvaAttackType.ToLeftWalls)
                _dataSource.Type5IsHidden = false;
        }

        private void HoverSpawnTroopChoose()
        {
            MakeAllHidden();
            _dataSource.CallTruvaCancelIsHidden = _spawnTroop;
            _dataSource.CallTruvaIsHidden = !_spawnTroop;
        }
    }
}
