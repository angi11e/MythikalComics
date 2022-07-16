using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;
using Handelabra;

namespace Angille.TheUndersiders
{
	public class TenebrousCloudCardController : TheUndersidersBaseCardController
	{
		private const string FirstDamageToVCC = "FirstDamageToVCC";

		public TenebrousCloudCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
		}

		public override bool AskIfCardIsIndestructible(Card card)
		{
			// Skull: This card is indestructible.
			return IsEnabled("skull") && card == base.Card;
		}

		public override void AddTriggers()
		{
			// Reduce HP recovery for hero targets by 1.
			AddTrigger(
				(GainHPAction g) => g.HpGainer.IsHero,
				(GainHPAction g) => base.GameController.ReduceHPGain(
					g,
					1,
					GetCardSource()
				),
				new TriggerType[2]
				{
					TriggerType.ReduceHPGain,
					TriggerType.ModifyHPGain
				},
				TriggerTiming.Before
			);

			// Increase HP recovery for villain targets by 1.
			AddTrigger(
				(GainHPAction g) => g.HpGainer.IsVillain,
				(GainHPAction g) => base.GameController.IncreaseHPGain(
					g,
					1,
					GetCardSource()
				),
				new TriggerType[2]
				{
					TriggerType.IncreaseHPGain,
					TriggerType.ModifyHPGain
				},
				TriggerTiming.Before
			);

			// Tattle: After the first time a villain target is dealt damage by a target each turn, the villain character target with the lowest HP regains HP equal to the amount of damage dealt.
			AddTrigger(
				(DealDamageAction dd) =>
					!IsPropertyTrue(FirstDamageToVCC)
					&& dd.DamageSource.IsTarget
					&& dd.Target.IsVillain
					&& dd.DidDealDamage,
				HealingResponse,
				TriggerType.GainHP,
				TriggerTiming.After
			);

			AddAfterLeavesPlayAction(
				(GameAction ga) => ResetFlagAfterLeavesPlay(FirstDamageToVCC),
				TriggerType.Hidden
			);

			base.AddTriggers();
		}

		private IEnumerator HealingResponse(DealDamageAction dd)
		{
			SetCardPropertyToTrueIfRealAction(FirstDamageToVCC);

			if (IsEnabled("tattle"))
			{
				List<Card> thePatient = new List<Card>();
				IEnumerator getPatientCR = GameController.FindTargetWithLowestHitPoints(
					1,
					(Card c) => c.IsVillainCharacterCard && c.IsInPlayAndNotUnderCard && !c.IsFlipped,
					thePatient,
					cardSource: GetCardSource()
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(getPatientCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(getPatientCR);
				}

				IEnumerator healingCR = GameController.GainHP(
					thePatient.FirstOrDefault(),
					dd.Amount,
					cardSource: GetCardSource()
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(healingCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(healingCR);
				}
			}

			yield break;
		}

		public override IEnumerator Play()
		{
			yield break;
		}
	}
}
