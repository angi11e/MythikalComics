using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.NightMare
{
	public class HoofLooseCardController : NightMareBaseCardController
	{
		/*
		 * When this card enters play, draw 2 cards.
		 * If {NightMare} would be dealt 3 or more Damage from a single source,
		 *  reduce it by 3, then destroy this card.
		 *  
		 * DISCARD
		 * Draw 1 card.
		 */

		private ITrigger _reduceDamageTrigger;

		public HoofLooseCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// When this card enters play, draw 2 cards.
			IEnumerator drawCardCR = DrawCards(DecisionMaker, 2);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(drawCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(drawCardCR);
			}

			yield break;
		}

		public override void AddTriggers()
		{
			// If {NightMare} would be dealt 3 or more Damage from a single source...
			_reduceDamageTrigger = AddTrigger(
				(DealDamageAction dda) => dda.Target == this.CharacterCard && dda.Amount >= 3,
				ReduceAndDestroyResponse,
				TriggerType.ReduceDamage,
				TriggerTiming.Before
			);

			base.AddTriggers();
		}

		private IEnumerator ReduceAndDestroyResponse(DealDamageAction dda)
		{
			// ...reduce it by 3...
			ReduceDamageAction reduceDamage = new ReduceDamageAction(
				GetCardSource(),
				dda,
				3,
				_reduceDamageTrigger
			);

			IEnumerator reduceCR = DoAction(reduceDamage);

			// ...then destroy this card.
			IEnumerator destructionCR = GameController.DestroyCard(
				DecisionMaker,
				this.Card,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(reduceCR);
				yield return GameController.StartCoroutine(destructionCR);
			}
			else
			{
				GameController.ExhaustCoroutine(reduceCR);
				GameController.ExhaustCoroutine(destructionCR);
			}

			yield break;
		}

		protected override IEnumerator DiscardResponse(GameAction ga)
		{
			// Draw 1 card.
			IEnumerator drawCardCR = DrawCard(HeroTurnTaker);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(drawCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(drawCardCR);
			}

			yield break;
		}
	}
}