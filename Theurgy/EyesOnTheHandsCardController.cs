using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public class EyesOnTheHandsCardController : TheurgyBaseCardController
	{
		/*
		 * When you discard a [u]charm[/u] card,
		 * {Theurgy} may deal herself 3 irreducible psychic damage.
		 * If she takes damage this way, put the [u]charm[/u] card into play instead.
		 */

		// old version:
		// Whenever you discard a card, you may put it into your hand instead.
		// If you do, either destroy this card or Theurgy deals herself 2 irreducible psychic damage.

		public EyesOnTheHandsCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Whenever you discard a charm card...
			AddTrigger(
				(MoveCardAction d) => d.CardToMove.Owner == this.TurnTaker && IsCharm(d.CardToMove) &&
				(d.Origin.IsHand || d.Origin.IsDeck || d.Origin.IsRevealed) &&
				d.Destination == d.CardToMove.Owner.Trash && d.IsDiscard && d.CanChangeDestination,
				DealWithTheDiscard,
				new TriggerType[2] {
					TriggerType.DealDamage,
					TriggerType.PutIntoPlay
				},
				TriggerTiming.Before
			);

			base.AddTriggers();
		}

		private IEnumerator DealWithTheDiscard(MoveCardAction mc)
		{
			// {Theurgy} may deal herself 3 irreducible psychic damage.
			List<DealDamageAction> storedDamage = new List<DealDamageAction>();
			IEnumerator selfDamageCR = DealDamage(
				this.CharacterCard,
				this.CharacterCard,
				3,
				DamageType.Psychic,
				isIrreducible: true,
				optional: true,
				storedResults: storedDamage
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selfDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selfDamageCR);
			}

			if (DidDealDamage(storedDamage, this.CharacterCard, this.CharacterCard))
			{
				// If she takes damage this way, put the [u]charm[/u] card into play instead.
				List<bool> playStorage = new List<bool>();
				IEnumerator playInsteadCR = GameController.PlayCard(
					DecisionMaker,
					mc.CardToMove,
					wasCardPlayed: playStorage,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playInsteadCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playInsteadCR);
				}

				if (playStorage.Any(x => x))
				{
					IEnumerator cancelCR = CancelAction(mc, false);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(cancelCR);
					}
					else
					{
						GameController.ExhaustCoroutine(cancelCR);
					}
				}
			}

			yield break;
		}
	}
}