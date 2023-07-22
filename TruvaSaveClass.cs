using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.SaveSystem;

namespace Truva
{
    public class TruvaSaveClass
    {
        [SaveableField(1)]
        public int aa = 8;

        [SaveableField(2)]
        public bool ki = true;

        public string _merhaba = "hey";

        [SaveableProperty(1)]
        public string SettlementId
        {
            get { return _merhaba; }
            set { }
        }

        public TruvaSaveClass(int _aa,bool _ki) 
        { 
            aa = _aa;
            ki = _ki;
        }


    }
}
