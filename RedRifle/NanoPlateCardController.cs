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
		 * At the start of your turn either add 1 token to your trueshot pool, or {RedRifle} regains 1 HP.
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
			// At the start of your turn either add 1 tokens to your trueshot pool, or {RedRifle} regains 1 HP.
			// moved to its own function, because the gamestate inadvertantly "remembers" your very first choice
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == base.TurnTaker,
				(PhaseChangeAction p) => ChooseOneResponse(p),
				new TriggerType[] {TriggerType.AddTokensToPool, TriggerType.GainHP}
			);

			// Reduce damage dealt to {RedRifle} by 1.
			AddReduceDamageTrigger((Card c) => c == base.CharacterCard, 1);

			base.AddTriggers();
		}

		private IEnumerator ChooseOneResponse(PhaseChangeAction p)
		{
			// At the start of your turn either add 1 tokens to your trueshot pool, or {RedRifle} regains 1 HP.
			List<Function> functionList = new List<Function>();

			// first 2 tokens option
			functionList.Add(
				new Function(
					DecisionMaker,
					"add 1 token to trueshot pool",
					SelectionType.AddTokens,
					() => AddTrueshotTokens(1)
				)
			);

			// ...or regain hp option
			functionList.Add(
				new Function(
					DecisionMaker,
					"regain 1 HP",
					SelectionType.GainHP,
					() => GameController.GainHP(
						base.CharacterCard,
						1,
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

			IEnumerator chooseCR = GameController.SelectAndPerformFunction(selectFunction);
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(chooseCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(chooseCR);
			}

			yield break;
		}
	}
}