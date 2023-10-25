using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Nexus
{
	public class AtmosphericDisturbanceCardController : NexusOneShotCardController
	{
		/*
		 * {Nexus} deals 1 target 2 projectile damage and 1 different target 2 fire damage, in either order.
		 * 
		 * reveal the top card of your deck.
		 * if it is a one-shot, put it into play.
		 * otherwise, move it to your hand.
		 */

		public AtmosphericDisturbanceCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController, DamageType.Projectile, DamageType.Fire)
		{
		}

		public override IEnumerator Play()
		{
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(base.Play());
			}
			else
			{
				GameController.ExhaustCoroutine(base.Play());
			}

			// reveal the top card of your deck.
			List<Card> revealed = new List<Card>();
			IEnumerator revealCR = GameController.RevealCards(
				TurnTakerController,
				this.TurnTaker.Deck,
				1,
				revealed,
				revealedCardDisplay: RevealedCardDisplay.Message,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(revealCR);
			}
			else
			{
				GameController.ExhaustCoroutine(revealCR);
			}

			Location destination = null;
			Card theCard = revealed.FirstOrDefault();
			if (theCard != null && theCard.IsOneShot)
			{
				// if it is a one-shot, put it into play.
				destination = this.TurnTaker.PlayArea;
			}
			else
			{
				// otherwise, move it to your hand.
				destination = this.HeroTurnTaker.Hand;
			}

			if (destination != null && theCard != null)
			{
				IEnumerator moveCR = GameController.MoveCard(
					TurnTakerController,
					theCard,
					destination,
					isPutIntoPlay: true,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(moveCR);
				}
				else
				{
					GameController.ExhaustCoroutine(moveCR);
				}
			}

			yield break;
		}
	}
}