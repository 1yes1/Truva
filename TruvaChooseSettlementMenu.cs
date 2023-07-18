using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Truva
{
    public class TruvaChooseSettlementMenu
    {
        public TruvaChooseSettlementMenu(Action<List<InquiryElement>> OnSettlementChoosed, Action<List<InquiryElement>> OnSettlementChooseCanceled)
        {
            List<InquiryElement> list = new List<InquiryElement>();

            foreach (var item in Settlement.All)
            {
                if((item.IsCastle || item.IsTown) && TruvaHelper.FindTruvaTroop(item.StringId) == null && item.OwnerClan.Kingdom.IsAtWarWith(Clan.PlayerClan))
                {
                    list.Add(new InquiryElement(item, item.Name.ToString(), new ImageIdentifier(item.OwnerClan.Kingdom.Banner)));
                }
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
            new TextObject("Truva Troop Settlement", null).ToString(), string.Empty, list, true, 1, new TextObject("Choose Settlement", null).ToString(),
            new TextObject("{=3CpNUnVl}Cancel", null).ToString(), new Action<List<InquiryElement>>(OnSettlementChoosed),
            OnSettlementChooseCanceled, ""), false, false);
        }
    }
}
