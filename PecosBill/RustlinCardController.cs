using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public class RustlinCardController : HyperboleBaseCardController
	{
		/*
		 * Play this card next to [i]Loyal Lightning[/i].
		 * If [i]Loyal Lightning[/i] is ever not in play, destroy this card.
		 * 
		 * Reduce damage dealt to [u]folk[/u] cards by 1.
		 * 
		 * TALL TALE
		 * Reveal the top card of a deck. Put it into play or discard it.
		 */

		public RustlinCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController, "LoyalLightning")
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// Reduce damage dealt to [u]folk[/u] cards by 1.
			AddReduceDamageTrigger((Card c) => IsFolk(c), 1);
		}

		public override IEnumerator ActivateTallTale()
		{
			// Reveal the top card of a deck. Put it into play or discard it.
			List<SelectLocationDecision> storedResults = new List<SelectLocationDecision>();
			IEnumerator selectDeckCR = GameController.SelectADeck(
				DecisionMaker,
				SelectionType.RevealTopCardOfDeck,
				(Location l) => l.IsDeck && !l.OwnerTurnTaker.IsIncapacitatedOrOutOfGame,
				storedResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectDeckCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectDeckCR);
			}

			Location selectedLocation = GetSelectedLocation(storedResults);
			if (selectedLocation != null)
			{
				IEnumerator revealCR = RevealCard_PlayItOrDiscardIt(
					this.TurnTakerController,
					selectedLocation,
					isPutIntoPlay: true,
					responsibleTurnTaker: this.TurnTaker
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(revealCR);
				}
				else
				{
					GameController.ExhaustCoroutine(revealCR);
				}
			}
			yield break;
		}
	}
}