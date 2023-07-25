using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Diamond;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Truva.Menus
{
    public class TruvaChooseSettlementMenu
    {
        public TruvaChooseSettlementMenu(Action<List<InquiryElement>> OnSettlementChoosed, Action<List<InquiryElement>> OnSettlementChooseCanceled)
        {
            List<InquiryElement> list = new List<InquiryElement>();

            IEnumerable<IFaction> enemyFactionsNum = FactionManager.GetEnemyFactions(Clan.PlayerClan);
            //InformationManager.DisplayMessage(new InformationMessage("Normal: " + enemyFactionsNum.Count(), Colors.Red));

            if (Clan.PlayerClan.Kingdom != null)
            {
                IEnumerable<IFaction> kingdomNum = FactionManager.GetEnemyFactions(Clan.PlayerClan.Kingdom);
                enemyFactionsNum = enemyFactionsNum.Concat(kingdomNum);
            }

            MBList<IFaction> enemyList = enemyFactionsNum.ToMBList();


            enemyList.Distinct();

            for (int i = 0; i < enemyList.Count; i++)
            {
                IFaction faction = enemyList[i];

                if (faction.IsClan)
                {
                    Clan clan = (Clan)faction;

                    for (int j = 0; j < clan.Settlements.Count; j++)
                    {
                        Settlement item = clan.Settlements[j];

                        if ((!item.IsCastle && !item.IsTown) || item.IsUnderSiege || TruvaHelper.FindTruvaTroop(item.StringId) != null)
                            continue;

                        ImageIdentifier imageIdentifier;

                        if (clan.Kingdom != null)
                            imageIdentifier = new ImageIdentifier(clan.Kingdom.Banner);
                        else
                            imageIdentifier = new ImageIdentifier(clan.Banner);

                        list.Add(new InquiryElement(item, item.Name.ToString(), imageIdentifier));
                    }
                }
                else if (faction.IsKingdomFaction)
                {
                    Kingdom kingdom = (Kingdom)faction;

                    for (int j = 0; j < kingdom.Settlements.Count; j++)
                    {
                        Settlement item = kingdom.Settlements[j];

                        if ((!item.IsCastle && !item.IsTown) || item.IsUnderSiege || TruvaHelper.FindTruvaTroop(item.StringId) != null)
                            continue;

                        ImageIdentifier imageIdentifier = new ImageIdentifier(kingdom.Banner);

                        list.Add(new InquiryElement(item, item.Name.ToString(), imageIdentifier));
                    }
                }

            }

            if (list.Count <= 0)
            {
                InformationManager.ShowInquiry(new InquiryData(new TextObject("Info", null).ToString(), new TextObject("There is no clan that you are enemy to!", null).ToString(),
true, false, new TextObject("{=yS7PvrTD}OK", null).ToString(), null, null, null, "", 0f, null, null, null), false, true);
                return;
            }

            list.Sort((a, b) => a.Title.CompareTo(b.Title));

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
            new TextObject("Truva Troop Settlement", null).ToString(), string.Empty, list, true, 1, new TextObject("Choose Settlement", null).ToString(),
            new TextObject("{=3CpNUnVl}Cancel", null).ToString(), new Action<List<InquiryElement>>(OnSettlementChoosed),
            OnSettlementChooseCanceled, ""), false, false);
        }
    }
}
