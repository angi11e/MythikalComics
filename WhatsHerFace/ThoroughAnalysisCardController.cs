using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.WhatsHerFace
{
	public class ThoroughAnalysisCardController : RecallBaseCardController
	{
		/*
		 * Play this card next to a target.
		 * At the start of your turn, either that target gains 2 HP or {WhatsHerFace} deals that target 2 Psychic damage.
		 * If that target leaves play, return this card to your hand.
		 */

		public ThoroughAnalysisCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		// Play this card next to a target.
		protected override LinqCardCriteria CustomCriteria => new LinqCardCriteria(
			(Card c) => c.IsTarget && c.IsInPlayAndHasGameText,
			"target"
		);

		public override void AddTriggers()
		{
			// At the start of your turn, either that target gains 2 HP or {WhatsHerFace} deals that target 2 Psychic damage.
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == base.TurnTaker,
				HelpOrHinderResponse,
				new TriggerType [2] { TriggerType.GainHP, TriggerType.DealDamage }
			);

			// If that target leaves play, return this card to your hand.
			AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToYourHandTrigger();

			base.AddTriggers();
		}

		private IEnumerator HelpOrHinderResponse(PhaseChangeAction p)
		{
			List<Function> functionList = new List<Function>();

			// first gainHP option
			functionList.Add(
				new Function(
					DecisionMaker,
					GetCardThisCardIsNextTo().Title + " gains 2 HP",
					SelectionType.GainHP,
					() => GameController.GainHP(
						GetCardThisCardIsNextTo(),
						2,
						cardSource: GetCardSource()
					)
				)
			);

			// ...or deal 2 psychic damage
			functionList.Add(
				new Function(
					this.DecisionMaker,
					"Deal " + GetCardThisCardIsNextTo().Title + " 2 psychic damage",
					SelectionType.DealDamage,
					() => DealDamage(
						base.CharacterCard,
						GetCardThisCardIsNextTo(),
						2,
						DamageType.Psychic,
						cardSource: GetCardSource()
					)
				)
			);

			// ask for which one
			SelectFunctionDecision selectFunction = new SelectFunctionDecision(
				GameController,
				DecisionMaker,
				functionList,
				false,
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

			yield break;
		}
	}
}