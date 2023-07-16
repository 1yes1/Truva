using System;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using static Truva.TruvaSiegeAttackLogic;
using static Truva.TruvaSiegeLogic;

namespace Truva
{
    public class TruvaInputManager: MissionLogic
    {
        public event Action OnKeyCtrlAPressed;

        public event Action<TruvaAttackType> OnAttackKeysPressed;

        public event Action OnAttackTypeScreenKeysPressedEvent;

        public event Action<bool> OnChooseAttackTypeKeyPressing;

        public override void OnPreMissionTick(float dt)
        {
            if (!Mission.Current.IsSiegeBattle)
                return;

            OnChooseAttackTypeKeyPressing?.Invoke(Input.IsKeyDownImmediate(InputKey.Q));
        }


        public override void OnMissionTick(float dt)
        {
            if (!Mission.Current.IsSiegeBattle)
                return;

            if (InputKey.LeftControl.IsDown() && Input.IsKeyPressed(InputKey.A))
                OnKeyCtrlAPressed?.Invoke();

            if (InputKey.LeftControl.IsDown() && Input.IsKeyPressed(InputKey.S))
                OnAttackKeysPressed?.Invoke(TruvaAttackType.ToLeftWalls);

            if (InputKey.LeftControl.IsDown() && Input.IsKeyPressed(InputKey.D))
                OnAttackKeysPressed?.Invoke(TruvaAttackType.ToRightWalls);

            if (InputKey.LeftControl.IsDown() && Input.IsKeyPressed(InputKey.Q))
                OnAttackKeysPressed?.Invoke(TruvaAttackType.ToSiegeWeapons);

            if (InputKey.LeftControl.IsDown() && Input.IsKeyPressed(InputKey.E))
                OnAttackKeysPressed?.Invoke(TruvaAttackType.ToGate);

            if (InputKey.LeftControl.IsDown() && Input.IsKeyPressed(InputKey.W))
                OnAttackKeysPressed?.Invoke(TruvaAttackType.Charge);

            if (InputKey.LeftControl.IsDown() && Input.IsKeyPressed(InputKey.Space))
                OnAttackTypeScreenKeysPressedEvent?.Invoke();

            if (InputKey.LeftControl.IsDown() && InputKey.V.IsPressed())
            {
                //OnKeyCtrlKPressed?.Invoke();
                Blow b = new Blow
                {
                    DamageCalculated = true,
                    BaseMagnitude = (float)150,
                    InflictedDamage = 150,
                    DamagedPercentage = 1f,
                    OwnerId = Agent.Main.Index
                };
                Agent.Main.Die(b);
            }
        }


    }
}
