using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Truva.Models
{
    public class TruvaModel:GameModel
    {
        public static int WageMultiplier { get => 500; }

        public static int GetPercentageOfCaught(Settlement settlement)
        {
            int securityImpactPercentage = MathF.Ceiling((settlement.Town.Security) * 0.2f);
            int skillImpactPercentage = MathF.Abs(MathF.Ceiling((400 - Hero.MainHero.GetSkillValue(DefaultSkills.Tactics)) * 0.1f));
            return securityImpactPercentage + skillImpactPercentage;
        }
    }
}
