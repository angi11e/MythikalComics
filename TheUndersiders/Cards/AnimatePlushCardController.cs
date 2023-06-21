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
	public class AnimatePlushCardController : TheUndersidersBaseCardController
	{
		private const string FirstDamageToThis = "FirstDamageToThis";

		public AnimatePlushCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			SpecialStringMaker.ShowHasBeenUsedThisTurn(
				FirstDamageToThis,
				"{0} has already redirected damage this turn.",
				"{0} has not yet redirected damage this turn."
			).Condition = () => this.Card.IsInPlayAndHasGameText && IsEnabled("bear");

			SpecialStringMaker.ShowHeroTargetWithHighestHP();

			SpecialStringMaker.ShowSpecialString(() => GetSpecialStringIcons("bear", "spider"));
		}

		public override void AddTriggers()
		{
			// At the end of the villain turn, this card deals the hero target with the highest HP {H - 1} melee damage.
			AddDealDamageAtEndOfTurnTrigger(
				TurnTaker,
				this.Card,
				(Card c) => IsHeroTarget(c),
				TargetType.HighestHP,
				H - 1,
				DamageType.Melee
			);

			// Spider: Damage dealt by this card is Psychic and irreducible.
			AddChangeDamageTypeTrigger(
				(DealDamageAction dda) =>
					dda.DamageSource.IsSameCard(this.Card)
					&& IsEnabled("spider"),
				DamageType.Psychic
			);

			AddMakeDamageIrreducibleTrigger(
				(DealDamageAction dda) =>
					dda.DamageSource.IsSameCard(this.Card)
					&& IsEnabled("spider")
			);

			// Bear: The first time each turn this card would be dealt damage, redirect it to the environment target with the lowest HP.
			AddTrigger(
				(DealDamageAction dd) =>
					!IsPropertyTrue(FirstDamageToThis)
					&& dd.Target == this.Card
					&& dd.DidDealDamage
					&& IsEnabled("bear"),
				RedirectResponse,
				TriggerType.RedirectDamage,
				TriggerTiming.Before
			);

			AddAfterLeavesPlayAction(
				(GameAction ga) => ResetFlagAfterLeavesPlay(FirstDamageToThis),
				TriggerType.Hidden
			);

			base.AddTriggers();
		}

		private IEnumerator RedirectResponse(DealDamageAction dd)
		{
			SetCardPropertyToTrueIfRealAction(FirstDamageToThis);

			List<Card> storedResults = new List<Card>();
			IEnumerator findEnvironmentCR = GameController.FindTargetWithLowestHitPoints(
				1,
				(Card c) => c.IsEnvironmentTarget,
				storedResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(findEnvironmentCR);
			}
			else
			{
				GameController.ExhaustCoroutine(findEnvironmentCR);
			}

			Card newTarget = storedResults.FirstOrDefault();
			if (newTarget != null)
			{
				IEnumerator redirectCR = GameController.RedirectDamage(
					dd,
					newTarget,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(redirectCR);
				}
				else
				{
					GameController.ExhaustCoroutine(redirectCR);
				}
			}

			yield break;
		}
	}
}