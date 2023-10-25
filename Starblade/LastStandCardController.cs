using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Starblade
{
	public class LastStandCardController : CardController
	{
		/*
		 * activate a [u]technique[/u] text.
		 * 
		 * destroy any number of construct cards.
		 * {Starblade} deals 1 target X energy damage,
		 * then X hero targets each regain 3 hp,
		 * where X = the number of construct cards destroyed this way.
		 * 
		 * you may play a postura card.
		 */

		public LastStandCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// activate a [u]technique[/u] text.
			IEnumerator activateCR = GameController.SelectAndActivateAbility(
				DecisionMaker,
				"technique",
				optional: false,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(activateCR);
			}
			else
			{
				GameController.ExhaustCoroutine(activateCR);
			}

			// destroy any number of construct cards.
			List<DestroyCardAction> actions = new List<DestroyCardAction>();

			IEnumerator destroyCR = GameController.SelectAndDestroyCards(
				DecisionMaker,
				new LinqCardCriteria((Card c) => c.IsConstruct, "construct"),
				null,
				requiredDecisions: 0,
				storedResultsAction: actions,
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

			// where X = the number of construct cards destroyed this way.
			Func<Card, int?> amount = (Card c) => actions.Where(
				(DestroyCardAction d) => d.CardToDestroy != null && d.WasCardDestroyed
			).Count();

			// {Starblade} deals 1 target X energy damage,
			IEnumerator damageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				amount,
				DamageType.Energy,
				() => 1,
				false,
				1,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(damageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(damageCR);
			}

			// then X hero targets each regain 3 hp,
			IEnumerator healCR = GameController.SelectAndGainHP(
				DecisionMaker,
				3,
				additionalCriteria: (Card c) => IsHeroTarget(c),
				numberOfTargets: amount(this.Card) ?? 0,
				requiredDecisions: 0,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(healCR);
			}
			else
			{
				GameController.ExhaustCoroutine(healCR);
			}

			// you may play a postura card.
			IEnumerator playPosturaCR = SelectAndPlayCardFromHand(
				this.HeroTurnTakerController,
				cardCriteria: new LinqCardCriteria(
					(Card c) => c.DoKeywordsContain("postura"),
					"postura"
				)
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playPosturaCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playPosturaCR);
			}

			yield break;
		}
	}
}