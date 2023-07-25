using System;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using Truva.CampaignBehaviors;
using static Truva.MissionBehaviors.TruvaSiegeAttackLogic;
using static Truva.MissionBehaviors.TruvaSiegeLogic;

namespace Truva.MissionBehaviors
{
    public class TruvaInputManager : MissionLogic
    {
        public event Action<bool> OnChooseAttackTypeKeyPressing;

        public override void OnPreMissionTick(float dt)
        {
            if (!Mission.Current.IsSiegeBattle || Mission.Current.PlayerTeam.Side == TaleWorlds.Core.BattleSideEnum.Defender)
                return;

            OnChooseAttackTypeKeyPressing?.Invoke(Input.IsKeyDownImmediate(InputKey.Q));

            //if (InputKey.LeftControl.IsDown() && InputKey.V.IsPressed())
            //{
            //    //OnKeyCtrlKPressed?.Invoke();
            //    Blow b = new Blow
            //    {
            //        DamageCalculated = true,
            //        BaseMagnitude = (float)150,
            //        InflictedDamage = 150,
            //        DamagedPercentage = 1f,
            //        OwnerId = Agent.Main.Index
            //    };
            //    Agent.Main.Die(b);
            //}
        }
    }
}
