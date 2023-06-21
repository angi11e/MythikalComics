using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Supplicate
{
	public class WingedSnakeSpiritCardController : YaojingBaseCardController
	{
		/*
		 * at the start of your turn,
		 * this card deals itself or {Supplicate} 2 irreducible psychic damage.
		 * 
		 * at the end of your turn, discard up to 3 cards.
		 * this card deals X targets 1 toxic damage each,
		 * where X = the number of cards discarded this way.
		 * until the start of your next turn,
		 * reduce damage dealt by targets dealt damage this way by the damage they take.
		 */

		public WingedSnakeSpiritCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// at the end of your turn,
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				ChompChompResponse,
				TriggerType.DealDamage
			);
		}

		private IEnumerator ChompChompResponse(PhaseChangeAction pca)
		{
			// discard up to 3 cards.
			List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
			IEnumerator discardCR = SelectAndDiscardCards(
				DecisionMaker,
				3,
				optional: false,
				0,
				storedResults
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			if (storedResults.Any())
			{
				// where X = the number of cards discarded this way.
				int targetNumeral = storedResults.Count();

				// this card deals X targets 1 toxic damage each.
				IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
					DecisionMaker,
					new DamageSource(GameController, this.Card),
					1,
					DamageType.Toxic,
					targetNumeral,
					false,
					targetNumeral,
					addStatusEffect: ReduceDamageByDamageResponse,
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

		private IEnumerator ReduceDamageByDamageResponse(DealDamageAction dda)
		{
			// reduce damage dealt by targets dealt damage this way by the damage they take.
			if (dda.DidDealDamage)
			{
				ReduceDamageStatusEffect reduceDamageSE = new ReduceDamageStatusEffect(dda.Amount);
				reduceDamageSE.SourceCriteria.IsSpecificCard = dda.Target;
				reduceDamageSE.UntilCardLeavesPlay(dda.Target);

				// until the start of your next turn,
				reduceDamageSE.UntilStartOfNextTurn(this.TurnTaker);

				IEnumerator reduceDamageCR = AddStatusEffect(reduceDamageSE);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(reduceDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(reduceDamageCR);
				}
			}
		}
	}
}