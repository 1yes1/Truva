using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Library;
using Truva.ViewModel;

namespace Truva
{
    public class TruvaTimeChecker
    {
        private CampaignTime _targetCampaignTime;

        private TruvaTroop _truvaTroop;

        private TroopRoster _troopRoster;

        private event Action<TruvaTroop,TroopRoster> OnTimeIsUp;

        public TruvaTimeChecker(CampaignTime targetCampaignTime,TruvaTroop truvaTroop,TroopRoster troopRoster, Action<TruvaTroop,TroopRoster> action)
        {
            _targetCampaignTime = targetCampaignTime;
            _truvaTroop = truvaTroop;
            _troopRoster = troopRoster;
            _truvaTroop.IsOnTheWay = true;
            OnTimeIsUp += action;
        }

        public void CheckTime()
        {
            if (_targetCampaignTime <= CampaignTime.Now)
            {
                OnTimeIsUp?.Invoke(_truvaTroop,_troopRoster);
                Campaign.Current.GetCampaignBehavior<TruvaCampaignBehavior>().RemoveFromTimeCheckers(this);
                _truvaTroop.IsOnTheWay = false;
            }
        }

    }
}
