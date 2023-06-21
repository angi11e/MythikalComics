using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;
using Handelabra;

namespace Angille.TheUndersiders
{
	public class ParianCharacterCardController : TheUndersidersVillainCardController
	{
		public ParianCharacterCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// When this card enters play, restore all villain plush targets to 6 HP. if there are non in play, search the villain trash and deck for a plush card and put it into play. if the villain deck was searched, shuffle it.
			if (FindCardsWhere((Card c) =>
				c.IsInPlayAndHasGameText
				&& IsVillainTarget(c)
				&& c.DoKeywordsContain("plush")
			).Count() == 0)
			{
				IEnumerator getBearCR = PlayCardFromLocations(
					new Location[2]
					{
						this.TurnTaker.Deck,
						this.TurnTaker.Trash
					},
					"AnimatePlush"
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(getBearCR);
				}
				else
				{
					GameController.ExhaustCoroutine(getBearCR);
				}
			}
			else
			{
				IEnumerator restoreCR = GameController.GainHP(
					DecisionMaker,
					(Card c) => c.DoKeywordsContain("plush") && IsVillainTarget(c),
					(Card c) => c.MaximumHitPoints.Value - c.HitPoints.Value,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(restoreCR);
				}
				else
				{
					GameController.ExhaustCoroutine(restoreCR);
				}
			}

			yield break;
		}

		public override void AddSideTriggers()
		{
			if (!this.Card.IsFlipped)
			{
				// Villain targets are immune to damage from environment cards.
				AddSideTrigger(AddImmuneToDamageTrigger(
					(DealDamageAction dd) => dd.DamageSource.IsEnvironmentCard && IsVillainTarget(dd.Target)
				));

				// Treat {Bear} effects as active. (this is done by the cards)
			}
			else
			{
				// At the end of the villain turn, discard the top card of the environment deck. if it's a target, play the top card of the villain deck.
				AddSideTrigger(AddEndOfTurnTrigger(
					(TurnTaker tt) => tt == this.TurnTaker,
					DiscardEnvironmentResponse,
					new TriggerType[]
					{
						TriggerType.DiscardCard,
						TriggerType.PlayCard
					}
				));
			}
			base.AddSideTriggers();
		}

		private IEnumerator DiscardEnvironmentResponse(PhaseChangeAction p)
		{
			List<MoveCardAction> storedDiscard = new List<MoveCardAction>();
			IEnumerator discardEnvironmentCR = GameController.DiscardTopCard(
				FindEnvironment().TurnTaker.Deck,
				storedDiscard,
				(Card c) => true,
				this.TurnTaker,
				GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardEnvironmentCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardEnvironmentCR);
			}

			MoveCardAction discardedCard = storedDiscard.FirstOrDefault();
			if (discardedCard != null && discardedCard.CardToMove.IsTarget)
			{
				IEnumerator playCardCR = PlayTheTopCardOfTheVillainDeckResponse(p);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playCardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playCardCR);
				}
			}

			yield break;
		}
	}
}
