using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Starblade
{
	public class ChainAndSpikesCardController : StarbladeConstructCardController
	{
		/*
		 * the first time each turn {Starblade} deals a non-hero target energy damage,
		 * this card deals each non-hero target 1 melee damage.
		 * 
		 * TECHNIQUE
		 * destroy an ongoing card. if it was a hero card, its player may draw a card.
		 * otherwise, this card deals itself 2 energy damage.
		 */

		private const string FirstEnergyDamageByStarblade = "FirstEnergyDamageByStarblade";

		public ChainAndSpikesCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowHasBeenUsedThisTurn(
				FirstEnergyDamageByStarblade,
				"{0} has already lashed out this turn.",
				"{0} has not yet lashed out this turn."
			);
		}

		public override void AddTriggers()
		{
			// the first time each turn {Starblade} deals a non-hero target energy damage,
			AddTrigger(
				(DealDamageAction dd) =>
					!IsHeroTarget(dd.Target)
					&& dd.DamageSource.IsCard
					&& dd.DamageSource.Card == this.CharacterCard
					&& dd.DidDealDamage
					&& dd.DamageType == DamageType.Energy
					&& !HasBeenSetToTrueThisTurn(FirstEnergyDamageByStarblade),
				(DealDamageAction dd) => HelpStrikeResponse(dd),
				TriggerType.DealDamage,
				TriggerTiming.After
			);
			AddAfterLeavesPlayAction(
				(GameAction ga) => ResetFlagAfterLeavesPlay(FirstEnergyDamageByStarblade),
				TriggerType.Hidden
			);

			base.AddTriggers();
		}

		private IEnumerator HelpStrikeResponse(DealDamageAction dd)
		{
			// this card deals each non-hero target 1 melee damage.
			SetCardPropertyToTrueIfRealAction(FirstEnergyDamageByStarblade);
			IEnumerator strikeCR = GameController.DealDamage(
				DecisionMaker,
				this.Card,
				(Card c) => !IsHeroTarget(c),
				1,
				DamageType.Melee,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(strikeCR);
			}
			else
			{
				GameController.ExhaustCoroutine(strikeCR);
			}

			yield break;
		}

		public override IEnumerator ActivateTechnique()
		{
			// destroy an ongoing card.
			List<DestroyCardAction> storedResults = new List<DestroyCardAction>();
			IEnumerator destroyCR = GameController.SelectAndDestroyCard(
				DecisionMaker,
				new LinqCardCriteria(
					(Card c) => IsOngoing(c),
					"ongoing"
				),
				optional: false,
				storedResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destroyCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destroyCR);
			}

			// if it was a hero card...
			if (DidDestroyCard(storedResults) && IsHero(storedResults.First().CardToDestroy.Card))
			{
				// ...its player may draw a card.
				HeroTurnTakerController httc = storedResults.First().CardToDestroy.DecisionMaker;

				IEnumerator drawCR = DrawCards(httc, 1, true);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(drawCR);
				}
				else
				{
					GameController.ExhaustCoroutine(drawCR);
				}
			}
			else
			{
				// otherwise this card deals itself 2 energy damage.
				IEnumerator dealDamageCR = DealDamage(
					this.Card,
					this.Card,
					2,
					DamageType.Energy,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(dealDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(dealDamageCR);
				}
			}

			yield break;
		}
	}
}