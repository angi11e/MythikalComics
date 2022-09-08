using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public class GoDownFightinCardController : PecosBillBaseCardController
	{
		/*
		 * You may activate a [u]tall tale[/u] text.
		 * 
		 * If you have at least 1 [i]folk[/i] card in play, a player may draw a card.
		 * If you have at least 2 [i]folk[/i] cards in play, a player may play a card.
		 * If you have 3 [i]folk[/i] cards in play, a hero may use a power.
		 * 
		 * You may draw or play a card.
		 */

		public GoDownFightinCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowListOfCardsInPlay(IsHyperboleCriteria());
			SpecialStringMaker.ShowNumberOfCardsInPlay(IsFolkCriteria());
		}

		public override IEnumerator Play()
		{
			// You may activate a [u]tall tale[/u] text.
			IEnumerator activateCR = GameController.SelectAndActivateAbility(
				DecisionMaker,
				"tall tale",
				optional: true,
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

			// If you have at least 1 [i]folk[/i] card in play, a player may draw a card.
			if (FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsFolk(c)).Any())
			{
				IEnumerator drawCR = GameController.SelectHeroToDrawCard(
					DecisionMaker,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(drawCR);
				}
				else
				{
					GameController.ExhaustCoroutine(drawCR);
				}
			}

			// If you have at least 2 [i]folk[/i] cards in play, a player may play a card.
			if (FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsFolk(c)).Count() > 1)
			{
				IEnumerator playCR = GameController.SelectHeroToPlayCard(
					DecisionMaker,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playCR);
				}
			}

			// If you have 3 [i]folk[/i] cards in play, a hero may use a power.
			if (FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsFolk(c)).Count() > 2)
			{
				IEnumerator powerCR = GameController.SelectHeroToUsePower(
					DecisionMaker,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(powerCR);
				}
				else
				{
					GameController.ExhaustCoroutine(powerCR);
				}
			}

			// You may draw or play a card.
			IEnumerator drawPlayCR = DrawACardOrPlayACard(DecisionMaker, optional: false);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(drawPlayCR);
			}
			else
			{
				GameController.ExhaustCoroutine(drawPlayCR);
			}

			yield break;
		}
	}
}