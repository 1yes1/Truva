using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Diplomacy;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;
using TaleWorlds.ScreenSystem;
using Truva.ViewModel;

namespace Truva.Extensions
{
    [ViewModelMixin("SetSelectedCategory")]
    internal class ClanScreenVMMixin : BaseViewModelMixin<ClanManagementVM>
    {
        private bool _isTruvaSelected = false;

        [DataSourceProperty]
        public bool IsTruvaSelected { get => _isTruvaSelected; set => SetField(ref _isTruvaSelected, value, nameof(IsTruvaSelected)); }


        public ClanScreenVMMixin(ClanManagementVM vm) : base(vm)
        {

        }


        [DataSourceMethod]
        public void SetTruvaCategory()
        {
            ScreenManager.PushScreen(ViewCreatorManager.CreateScreenView<TruvaTroopScreen>());
        }

        private void UnSelectAllCategories()
        {
            ViewModel!.IsMembersSelected = false;
            ViewModel!.IsPartiesSelected = false;
            ViewModel!.IsFiefsSelected = false;
            ViewModel!.IsIncomeSelected = false;
            IsTruvaSelected = false;
        }


    }
}
