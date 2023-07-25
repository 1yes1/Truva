using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.Settlements;
using Truva.CampaignBehaviors;

namespace Truva.ViewModel
{
    internal class TruvaTroopItemContentVM : TaleWorlds.Library.ViewModel
    {
        private TruvaTroop _truvaTroop;
        private TruvaTroopVM _truvaTroopVM;

        public TruvaTroopItemContentVM(TruvaTroopVM truvaTroopVM,TruvaTroop truvaTroop)
        {
            _truvaTroop = truvaTroop;
            _truvaTroopVM = truvaTroopVM;
        }

        [DataSourceProperty]
        public string SettlementName
        {
            get
            {
                return this._truvaTroop.SettlementName.ToString();
            }
        }

        [DataSourceProperty]
        public string TroopCount
        {
            get
            {
                return this._truvaTroop.TroopRoster.TotalManCount.ToString();
            }
            private set
            {
                TroopCount = value;
            }
        }

        public void ManageTroop()
        {
            //InformationManager.DisplayMessage(new InformationMessage("Edit Troop", Colors.Magenta));
            Campaign.Current.GetCampaignBehavior<TruvaCampaignBehavior>().OpenTruvaTroopMenu(true,_truvaTroop, Settlement.Find(_truvaTroop.SettlementId));
        }

        public void RemoveTroop()
        {
            //InformationManager.DisplayMessage(new InformationMessage("Remove Troop", Colors.Magenta));
            Campaign.Current.GetCampaignBehavior<TruvaCampaignBehavior>().RemoveTruvaTroop(_truvaTroop,false,true);

            //_truvaTroopVM.RefreshValues();
        }

        [DataSourceProperty]
        public bool IsOnTheWay { get => _truvaTroop.IsOnTheWay; }


        [DataSourceProperty]
        public bool IsWayTextVisible { get => !_truvaTroop.IsOnTheWay; }

    }
}
