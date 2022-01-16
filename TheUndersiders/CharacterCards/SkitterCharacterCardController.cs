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
	public class SkitterCharacterCardController : TheUndersidersVillainCardController
	{
		public SkitterCharacterCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			base.SpecialStringMaker.ShowNumberOfCardsInPlay(
				new LinqCardCriteria((Card c) => c.DoKeywordsContain("swarm") && c.IsVillain, "swarm")
			);
		}

		public override IEnumerator Play()
		{
			// When this card enters play, reveal cards from the top of the villain deck until {H - 2} swarm cards are revealed. Put them into play. Shuffle the other revealed cards back into the villain deck.
			IEnumerator getSwarmsCR = RevealCards_MoveMatching_ReturnNonMatchingCards(
				base.TurnTakerController,
				base.TurnTaker.Deck,
				playMatchingCards: false,
				putMatchingCardsIntoPlay: true,
				moveMatchingCardsToHand: false,
				new LinqCardCriteria((Card c) => c.DoKeywordsContain("swarm") && c.IsVillain, "swarm"),
				base.H - 2
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(getSwarmsCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(getSwarmsCR);
			}

			yield break;
		}

		public override void AddSideTriggers()
		{
			if (!base.Card.IsFlipped)
			{
				// At the start of the villain turn, restore all villain swarm targets to 3 HP.
				AddSideTrigger(AddStartOfTurnTrigger(
					(TurnTaker tt) => tt == base.TurnTaker,
					(PhaseChangeAction p) => GameController.GainHP(
						DecisionMaker,
						(Card c) => c.DoKeywordsContain("swarm") && c.IsVillainTarget,
						(Card c) => c.MaximumHitPoints.Value - c.HitPoints.Value,
						cardSource: GetCardSource()
					),
					TriggerType.GainHP
				));

				// Treat {Spider} effects as active. (taken care of by the cards)
			}
			else
			{
				// At the start of the villain turn, each villain swarm target regains 1 HP.
				AddSideTrigger(AddStartOfTurnTrigger(
					(TurnTaker tt) => tt == base.TurnTaker,
					(PhaseChangeAction p) => GameController.GainHP(
						DecisionMaker,
						(Card c) => c.DoKeywordsContain("swarm") && c.IsVillainTarget,
						(Card c) => 1,
						cardSource: GetCardSource()
					),
					TriggerType.GainHP
				));

				// At the end of the villain turn, if there are any swarm cards in play, the hero target with the highest HP deals themself 1 psychic damage.
				AddSideTrigger(AddEndOfTurnTrigger(
					(TurnTaker tt) => tt == base.TurnTaker && FindCardsWhere(
						(Card c) => c.DoKeywordsContain("swarm") && c.IsInPlayAndHasGameText
					).Count() > 0,
					TerrifyResponse,
					TriggerType.DealDamage
				));
			}
			base.AddSideTriggers();
		}

		private IEnumerator TerrifyResponse(PhaseChangeAction p)
		{
			List<Card> storedResults = new List<Card>();
			IEnumerator getHighestCR = GameController.FindTargetWithHighestHitPoints(
				1,
				(Card c) => c.IsHeroCharacterCard,
				storedResults,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(getHighestCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(getHighestCR);
			}

			Card poorSap = storedResults.FirstOrDefault();
			if (poorSap != null)
			{
				IEnumerator terrifyCR = DealDamage(
					poorSap,
					poorSap,
					1,
					DamageType.Psychic
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(terrifyCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(terrifyCR);
				}
			}

			yield break;
		}
	}
}
