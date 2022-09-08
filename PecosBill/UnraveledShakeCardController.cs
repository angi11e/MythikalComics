using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public class UnraveledShakeCardController : HyperboleBaseCardController
	{
		/*
		 * Play this card next to [i]Shake the Snake[/i].
		 * If [i]Shake the Snake[/i] is ever not in play, destroy this card.
		 * 
		 * When damage dealt by a target in this play area destroys a target, draw a card.
		 * 
		 * TALL TALE
		 * Destroy an ongoing card.
		 */

		public UnraveledShakeCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController, "ShakeTheSnake")
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// When damage dealt by a target in this play area destroys a target...
			AddTrigger(
				(DestroyCardAction d) => d.WasCardDestroyed
					&& d.ActionSource is DealDamageAction
					&& ((DealDamageAction)d.ActionSource).DamageSource.Card.IsInLocation(this.TurnTaker.PlayArea),
				// ...draw a card.
				(DestroyCardAction p) => DrawCard(this.HeroTurnTaker),
				TriggerType.GainHP,
				TriggerTiming.After
			);
		}

		public override IEnumerator ActivateTallTale()
		{
			// Destroy an ongoing card.
			IEnumerator destroyCR = GameController.SelectAndDestroyCard(
				DecisionMaker,
				new LinqCardCriteria((Card c) => c.IsOngoing, "ongoing"),
				false,
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

			// removed the next, but leaving the comment in case I want it to come back
			// The villain target with the highest HP deals [i]Shake the Snake[/i] 3 melee damage.

			yield break;
		}
	}
}