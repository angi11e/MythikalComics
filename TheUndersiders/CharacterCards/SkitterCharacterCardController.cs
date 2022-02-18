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

				// Reduce damage taken by villain targets by 1.
				AddSideTrigger(AddReduceDamageTrigger(
					(Card c) => c.IsVillainTarget && c.IsCharacter,
					1
				));

				// The first time each turn a villain swarm target enters play, play the top card of the villain deck.
				AddSideTrigger(AddTrigger(
					(CardEntersPlayAction p) => IsFirstTimeCardPlayedThisTurn(
						p.CardEnteringPlay,
						(Card c) => c.IsVillain && c.DoKeywordsContain("swarm"),
						TriggerTiming.After
					),
					PlayTheTopCardOfTheVillainDeckWithMessageResponse,
					TriggerType.PlayCard,
					TriggerTiming.After
				));

				// At the end of the villain turn, destroy X hero equipment cards,
				// where X = the number of villain swarm cards in play.
				AddSideTrigger(AddEndOfTurnTrigger(
					(TurnTaker tt) => tt == base.TurnTaker,
					RuinToysResponse,
					TriggerType.DestroyCard
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

		private IEnumerator RuinToysResponse(PhaseChangeAction p)
		{
			return GameController.SelectAndDestroyCards(
				DecisionMaker,
				new LinqCardCriteria((Card c) =>
					c.IsHero && IsEquipment(c),
					"hero equipment"
				),
				FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain("swarm")).Count(),
				cardSource: GetCardSource()
			);
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

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(getHighestCR);
			}
			else
			{
				GameController.ExhaustCoroutine(getHighestCR);
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

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(terrifyCR);
				}
				else
				{
					GameController.ExhaustCoroutine(terrifyCR);
				}
			}

			yield break;
		}
	}
}
