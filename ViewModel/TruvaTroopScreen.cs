using SandBox.BoardGames;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets;
using TaleWorlds.ScreenSystem;

namespace Truva.ViewModel
{
    internal class TruvaTroopScreen: ScreenBase
    {
        private TruvaTroopVM _dataSource;
        private GauntletLayer _gauntletLayer;
        private IGauntletMovie _movie;

        public TruvaTroopScreen( )
        {

        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _dataSource = new TruvaTroopVM();
            _gauntletLayer = new GauntletLayer(100);
            AddLayer(_gauntletLayer);
            _gauntletLayer.InputRestrictions.SetInputRestrictions();
            _movie = _gauntletLayer.LoadMovie("TruvaTroopMenu", _dataSource);
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            ScreenManager.TrySetFocus(_gauntletLayer);
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            _gauntletLayer.IsFocusLayer = false;
            ScreenManager.TryLoseFocus(_gauntletLayer);
        }

        protected override void OnFinalize()
        {
            base.OnFinalize();
            RemoveLayer(_gauntletLayer);
            _dataSource = null;
            _gauntletLayer = null;
        }
    }
}
