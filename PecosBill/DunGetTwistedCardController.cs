using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public class DunGetTwistedCardController : HyperboleBaseCardController
	{
		/*
		 * Play this card next to [i]Tamed Twister[/i].
		 * If [i]Tamed Twister[/i] is ever not in play, destroy this card.
		 * 
		 * when [i]Tamed Twister[/i] destroys a villain target,
		 * move it to the bottom of the villain deck instead of the trash.
		 * 
		 * TALL TALE
		 * Destroy an environment card.
		 */

		public DunGetTwistedCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController, "TamedTwister")
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// when [i]Tamed Twister[/i] destroys a villain target...
			AddTrigger(
				(DestroyCardAction d) => d.WasCardDestroyed
					&& d.PostDestroyDestinationCanBeChanged
					&& d.CardToDestroy.Card.IsTarget
					&& d.CardSource.Card.Identifier == "TamedTwister",
				PutUnderDeckResponse,
				TriggerType.MoveCard,
				TriggerTiming.After
			);
		}

		private IEnumerator PutUnderDeckResponse(DestroyCardAction d)
		{
			// ...move it to the bottom of the villain deck instead of the trash.
			d.SetPostDestroyDestination(
				GetNativeDeck(d.CardToDestroy.Card),
				true,
				showMessage: true,
				cardSource: GetCardSource()
			);

			yield return null;
		}

		public override IEnumerator ActivateTallTale()
		{
			// Destroy an environment card.
			IEnumerator destroyCR = GameController.SelectAndDestroyCard(
				DecisionMaker,
				new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"),
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

			yield break;
		}
	}
}