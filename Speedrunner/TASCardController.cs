using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class TASCardController : SpeedrunnerBaseCardController
	{
		/*
		 * increase damage dealt by {Speedrunner} by 1.
		 * 
		 * POWER
		 * One player may draw a card, a second player may play a card, and a third player may use a power.
		 * Destroy this card.
		 */

		public TASCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// increase damage dealt by {Speedrunner} by 1.
			AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageSource.IsSameCard(this.CharacterCard), 1);

			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			List<SelectTurnTakerDecision> storedTurnTaker = new List<SelectTurnTakerDecision>();

			// One player may draw a card...
			IEnumerator drawCR = GameController.SelectHeroToDrawCard(
				DecisionMaker,
				storedResults: storedTurnTaker,
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
			TurnTaker firstTT = storedTurnTaker.FirstOrDefault().SelectedTurnTaker;

			// ...a second player may play a card...
			storedTurnTaker = new List<SelectTurnTakerDecision>();
			IEnumerator playCR = GameController.SelectHeroToPlayCard(
				DecisionMaker,
				additionalCriteria: new LinqTurnTakerCriteria(
					(TurnTaker tt) => tt != firstTT
				),
				storedResultsTurnTaker: storedTurnTaker,
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
			TurnTaker secondTT = storedTurnTaker.FirstOrDefault().SelectedTurnTaker;

			// ...and a third player may use a power.
			storedTurnTaker = new List<SelectTurnTakerDecision>();
			IEnumerator powerCR = GameController.SelectHeroToUsePower(
				DecisionMaker,
				storedResultsDecision: storedTurnTaker,
				additionalCriteria: new LinqTurnTakerCriteria(
					(TurnTaker tt) => tt != firstTT && tt != secondTT
				),
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

			// Destroy this card.
			IEnumerator destroyCR = GameController.DestroyCard(
				DecisionMaker,
				this.Card,
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

			yield break;
		}
	}
}