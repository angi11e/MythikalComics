using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.WhatsHerFace
{
	public class PullThePinsCardController : WhatsHerFaceBaseCardController
	{
		/*
		 * Destroy any number of [u]recall[/u] cards.
		 * {WhatsHerFace} deals each non-hero target X Fire damage, where X = the number of cards destroyed this way.
		 */

		public PullThePinsCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsInPlay(IsRecallCriteria());
		}

		public override IEnumerator Play()
		{
			// Destroy any number of [u]recall[/u] cards.
			List<DestroyCardAction> actions = new List<DestroyCardAction>();

			IEnumerator destroyCR = GameController.SelectAndDestroyCards(
				DecisionMaker,
				IsRecallCriteria(),
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

			// ...where X = the number of cards destroyed this way.
			Func<Card, int?> amount = (Card c) => actions.Where(
				(DestroyCardAction d) => d.CardToDestroy != null && d.WasCardDestroyed
			).Count();

			// {WhatsHerFace} deals each non-hero target X Fire damage...
			IEnumerator damageCR = DealDamage(
				CharacterCard,
				(Card c) => !IsHeroTarget(c),
				amount,
				DamageType.Fire
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(damageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(damageCR);
			}

			yield break;
		}
	}
}