using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Starblade
{
	public class StarburstCardController : CardController
	{
		/*
		 * increase damage dealt by {Starblade} and by construct cards by 1.
		 * 
		 * you may use an additional power during your power phase.
		 * 
		 * at the start of your turn, destroy this card.
		 */

		public StarburstCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			AddThisCardControllerToList(CardControllerListType.IncreasePhaseActionCount);
		}

		public override void AddTriggers()
		{
			// increase damage dealt by {Starblade} and by construct cards by 1.
			AddIncreaseDamageTrigger(
				(DealDamageAction dda) => dda.DamageSource.IsCard
					&& (dda.DamageSource.Card == this.CharacterCard || dda.DamageSource.Card.IsConstruct),
				1
			);

			// you may use an additional power during your power phase.
			AddAdditionalPhaseActionTrigger((TurnTaker tt) => tt == this.TurnTaker, Phase.UsePower, 1);

			// at the start of your turn, destroy this card.
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				(PhaseChangeAction pca) => GameController.DestroyCard(
					DecisionMaker,
					this.Card,
					cardSource: GetCardSource()
				),
				TriggerType.DestroySelf
			);

			base.AddTriggers();
		}

		public override IEnumerator Play()
		{
			IEnumerator morePowerCR = IncreasePhaseActionCountIfInPhase(
				(TurnTaker tt) => tt == this.TurnTaker,
				Phase.UsePower,
				1
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(base.Play());
				yield return GameController.StartCoroutine(morePowerCR);
			}
			else
			{
				GameController.ExhaustCoroutine(base.Play());
				GameController.ExhaustCoroutine(morePowerCR);
			}
		}

		public override bool AskIfIncreasingCurrentPhaseActionCount()
		{
			if (GameController.ActiveTurnPhase.IsUsePower)
			{
				return GameController.ActiveTurnTaker == this.TurnTaker;
			}
			return false;
		}
	}
}