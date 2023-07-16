using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace Truva.ViewModel
{
    internal class TruvaTroopVM: TaleWorlds.Library.ViewModel
    {
        private MBBindingList<TruvaTroopItemVM> _truvaTroops;
        private string _truvaTroopsText;

        public TruvaTroopVM()
        {
            TruvaTroops = new MBBindingList<TruvaTroopItemVM>();
            //InformationManager.DisplayMessage(new InformationMessage("Truva Troops Count: " + TruvaCampaignBehavior.TruvaTroops.Count, Colors.Magenta));

            for (int i = 0; i < TruvaCampaignBehavior.TruvaTroops.Count; i++)
            {
                TruvaTroops.Add(new TruvaTroopItemVM(this,TruvaCampaignBehavior.TruvaTroops[i]) );
            }
        }

        public void ExecuteCancel()
        {
            ScreenManager.PopScreen();
        }

        public void ExecuteDone()
        {
            ScreenManager.PopScreen();
        }

        [DataSourceProperty]
        public string TruvaTroopsText
        {
            get
            {
                return this._truvaTroopsText;
            }
            set
            {
                if (value != this._truvaTroopsText)
                {
                    this._truvaTroopsText = value;
                    base.OnPropertyChangedWithValue<string>(value, "TruvaTroopsText");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<TruvaTroopItemVM> TruvaTroops
        {
            get
            {
                return this._truvaTroops;
            }
            set
            {
                if (value != this._truvaTroops)
                {
                    this._truvaTroops = value;
                    base.OnPropertyChangedWithValue<MBBindingList<TruvaTroopItemVM>>(value, "TruvaTroops");
                }
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            for (int i = 0; i < TruvaCampaignBehavior.TruvaTroops.Count; i++)
            {
                TruvaTroops[i].RefreshValues();
            }
        }

    }
}
