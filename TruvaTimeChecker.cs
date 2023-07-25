using Bannerlord.ButterLib.Common.Extensions;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;
using Truva.CampaignBehaviors;
using Truva.ViewModel;

namespace Truva
{
    public class TruvaTimeChecker
    {
        [SaveableField(1)] private CampaignTime _targetCampaignTime;

        [SaveableField(2)] private TruvaTroop _truvaTroop;

        [SaveableField(3)] private TroopRoster _troopRoster;

        public event Action<TruvaTroop, TroopRoster> OnTimeIsUpEvent;
        public TruvaTimeChecker(CampaignTime targetCampaignTime,TruvaTroop truvaTroop,TroopRoster troopRoster, Action<TruvaTroop,TroopRoster> action)
        {
            _targetCampaignTime = targetCampaignTime;
            _truvaTroop = truvaTroop;
            _troopRoster = troopRoster;
            _truvaTroop.IsOnTheWay = true;
            OnTimeIsUpEvent -= action;
            OnTimeIsUpEvent += action;
        }

        public void CheckTime()
        {
            if (_targetCampaignTime <= CampaignTime.Now)
            {
                Campaign.Current.GetCampaignBehavior<TruvaCampaignBehavior>().RemoveFromTimeCheckers(this);
                //InformationManager.DisplayMessage(new InformationMessage("------------------------- Time Is Upp --------------------------", Colors.Magenta));
                OnTimeIsUpEvent?.Invoke(_truvaTroop, _troopRoster);
                _truvaTroop.OnTimeIsUp();
            }
        }

    }
}
