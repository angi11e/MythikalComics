using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.RedRifle
{
	public class NanoPlateCardController : RedRifleBaseCardController
	{
		/*
		 * At the start of your turn either add 2 tokens to your trueshot pool, or {RedRifle} regains 2 HP.
		 * Reduce damage dealt to {RedRifle} by 1.
		 */

		public NanoPlateCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override bool AllowFastCoroutinesDuringPretend
		{
			get => base.CharacterCard.MaximumHitPoints == base.CharacterCard.HitPoints;
		}

		public override void AddTriggers()
		{
			// At the start of your turn either add 2 tokens to your trueshot pool, or {RedRifle} regains 2 HP.
			List<Function> functionList = new List<Function>();

			// first 2 tokens option
			functionList.Add(
				new Function(
					DecisionMaker,
					"add 2 tokens to trueshot pool",
					SelectionType.AddTokens,
					() => AddTrueshotTokens(2)
				)
			);

			// ...or regain hp option
			functionList.Add(
				new Function(
					DecisionMaker,
					"regain 2 HP",
					SelectionType.GainHP,
					() => GameController.GainHP(
						base.CharacterCard,
						2,
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

			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == base.TurnTaker,
				(PhaseChangeAction p) => GameController.SelectAndPerformFunction(selectFunction),
				new TriggerType[] {TriggerType.AddTokensToPool, TriggerType.GainHP}
			);

			// Reduce damage dealt to {RedRifle} by 1.
			AddReduceDamageTrigger((Card c) => c == base.CharacterCard, 1);

			base.AddTriggers();
		}
	}
}