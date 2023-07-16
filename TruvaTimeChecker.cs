using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;

namespace Truva
{
    internal class TruvaTimeChecker
    {
        private CampaignTime _targetCampaignTime;
        TruvaTroop _truvaTroop;

        private event Action<TruvaTroop,TruvaTimeChecker> OnTimeIsUp;

        public TruvaTimeChecker(CampaignTime targetCampaignTime,TruvaTroop truvaTroop, Action<TruvaTroop,TruvaTimeChecker> action)
        {
            _targetCampaignTime = targetCampaignTime;
            _truvaTroop = truvaTroop;
            OnTimeIsUp += action;
        }

        public void CheckTime()
        {
            if (_targetCampaignTime <= CampaignTime.Now)
            {
                OnTimeIsUp?.Invoke(_truvaTroop,this);
            }
        }

    }
}
