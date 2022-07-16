using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;
using Handelabra;

namespace Angille.CadaverTeam
{
	public class GustCardController : CardController
	{
		public GustCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			SpecialStringMaker.ShowHeroWithMostCards(true);
		}

		public override void AddTriggers()
		{
			// Reduce damage dealt to this card to 0.
			AddReduceDamageToSetAmountTrigger((DealDamageAction dd) => dd.Target == this.Card, 0);

			// At the end of {CadaverTeam}'s turn...
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				EndOfTurnResponse,
				TriggerType.DealDamage
			);

			// If {Angille.NightMare} is active in this game...
			// ...whenever a card would be discarded...
			AddTrigger(
				(MoveCardAction m) =>
					IsHeroActiveInThisGame("NightMareCharacter")
					&& m.Destination.IsTrash
					&& (m.Origin.IsHand || m.Origin.IsDeck || m.Origin.IsRevealed)
					&& m.IsDiscard && m.CanChangeDestination,
				DiscardResponse,
				TriggerType.Other,
				TriggerTiming.Before,
				outOfPlayTrigger: true
			);

			base.AddTriggers();
		}

		private IEnumerator EndOfTurnResponse(PhaseChangeAction p)
		{
			// ...this card deals X projectile damage to the hero character with the most cards in their hand...
			List<TurnTaker> handyTurnTaker = new List<TurnTaker>();
			IEnumerator findHandyCR = FindHeroWithMostCardsInHand(handyTurnTaker);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(findHandyCR);
			}
			else
			{
				GameController.ExhaustCoroutine(findHandyCR);
			}

			if (handyTurnTaker.FirstOrDefault() != null)
			{
				List<Card> storedCharacter = new List<Card>();
				IEnumerator findTargetCR = FindCharacterCard(
					handyTurnTaker.FirstOrDefault(),
					SelectionType.SelectTarget,
					storedCharacter
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(findTargetCR);
				}
				else
				{
					GameController.ExhaustCoroutine(findTargetCR);
				}

				Card theTarget = storedCharacter.FirstOrDefault();
				if (theTarget != null)
				{
					// ...where X = the lower of {H + 1} or the number of cards in their hand.
					int damageNumeral = theTarget.Owner.ToHero().NumberOfCardsInHand;
					damageNumeral = damageNumeral <= Game.H ? damageNumeral : Game.H + 1;
					IEnumerator dealDamageCR = DealDamage(
						this.Card,
						theTarget,
						damageNumeral,
						DamageType.Projectile,
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
			}

			yield break;
		}

		private IEnumerator DiscardResponse(MoveCardAction m)
		{
			// ...move it to the bottom of its deck instead.
			if (!m.CardToMove.IsOutOfGame)
			{
				m.SetDestination(m.CardToMove.Owner.Deck);
				m.ToBottom = true;
			}

			yield break;
		}
	}
}
