using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Starblade
{
	public class TeamFormationCardController : CardController
	{
		/*
		 * each player may draw a card.
		 * 
		 * one player may play a card or use a power.
		 * 
		 * activate a [u]technique[/u] text.
		 */

		public TeamFormationCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// each player may draw a card.
			IEnumerator drawCR = EachPlayerDrawsACard(
				(HeroTurnTaker htt) => !htt.IsIncapacitatedOrOutOfGame && IsHero(htt),
				true
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(drawCR);
			}
			else
			{
				GameController.ExhaustCoroutine(drawCR);
			}

			// one player may play a card or use a power.
			List<Function> choices = new List<Function>();
			choices.Add(new Function(
				DecisionMaker,
				"One player may play a card",
				SelectionType.PlayCard,
				() => SelectHeroToPlayCard(DecisionMaker),
				FindTurnTakersWhere(
					(TurnTaker tt) => IsHero(tt) && CanPlayCards(FindHeroTurnTakerController(tt.ToHero()))
				).Count() > 0
			));
			choices.Add(new Function(
				DecisionMaker,
				"One player may use a power",
				SelectionType.UsePower,
				() => GameController.SelectHeroToUsePower(
					DecisionMaker,
					cardSource: GetCardSource()
				),
				FindTurnTakersWhere(
					(TurnTaker tt) => IsHero(tt) && GameController.CanUsePowers(
						FindHeroTurnTakerController(tt.ToHero()), GetCardSource()
					)
				).Count() > 0
			));

			SelectFunctionDecision selectFunction = new SelectFunctionDecision(
				GameController,
				DecisionMaker,
				choices,
				optional: false,
				noSelectableFunctionMessage: "There are no heroes who can play cards or use powers.",
				cardSource: GetCardSource()
			);
			IEnumerator selectFunctionCR = GameController.SelectAndPerformFunction(selectFunction);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectFunctionCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectFunctionCR);
			}

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

			yield break;
		}
	}
}