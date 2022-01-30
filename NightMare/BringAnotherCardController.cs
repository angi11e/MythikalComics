using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.NightMare
{
	public class BringAnotherCardController : NightMareBaseCardController
	{
		/*
		 * Whenever {NightMare} destroys a target, she may deal 1 target 2 Melee damage.
		 * Discard the top card of your deck.
		 * 
		 * DISCARD
		 * Reveal the top card of the villain deck.
		 * If it is a target, put it into play and increase the next damage {NightMare} deals it by 3.
		 * Otherwise, discard it.
		 */

		public BringAnotherCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Whenever {NightMare} destroys a target...
			AddTrigger(
				(DestroyCardAction dca) =>
					dca.WasCardDestroyed
					&& dca.CardToDestroy.Card.IsTarget
					&& dca.CardSource != null
					&& dca.CardSource.Card == base.CharacterCard,
				DestroyTargetResponse,
				TriggerType.DealDamage,
				TriggerTiming.After
			);

			base.AddTriggers();
		}

		private IEnumerator DestroyTargetResponse(DestroyCardAction dca)
		{
			// Whenever {NightMare} destroys a target, she may deal 1 target 2 Melee damage.
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, base.CharacterCard),
				2,
				DamageType.Melee,
				1,
				true,
				1,
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

			// Discard the top card of your deck.
			IEnumerator discardTopCR = GameController.DiscardTopCard(
				TurnTaker.Deck,
				null,
				(Card c) => true,
				TurnTaker,
				GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardTopCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardTopCR);
			}

			yield break;
		}

		protected override IEnumerator DiscardResponse(GameAction ga)
		{
			// Reveal the top card of the villain deck.
			List<SelectLocationDecision> villainDecks = new List<SelectLocationDecision>();
			IEnumerator findVillainCR = GameController.SelectADeck(
				DecisionMaker,
				SelectionType.RevealTopCardOfDeck,
				(Location l) => l.IsVillain,
				villainDecks,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(findVillainCR);
			}
			else
			{
				GameController.ExhaustCoroutine(findVillainCR);
			}

			SelectLocationDecision villainDeck = villainDecks.FirstOrDefault();

			if (villainDeck != null && villainDeck.SelectedLocation.Location != null)
			{
				List<Card> storedResults = new List<Card>();
				// If it is a target, put it into play and increase the next damage {NightMare} deals it by 3.
				// Otherwise, discard it.
				IEnumerator revealCR = RevealCards_PutSomeIntoPlay_DiscardRemaining(
					TurnTakerController,
					villainDeck.SelectedLocation.Location,
					1,
					new LinqCardCriteria((Card c) => c.IsTarget, "target"),
					isPutIntoPlay: true,
					playedCards: storedResults
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(revealCR);
				}
				else
				{
					GameController.ExhaustCoroutine(revealCR);
				}

				Card card = storedResults.FirstOrDefault();
				if (card != null && card.IsTarget)
				{
					IncreaseDamageStatusEffect increaseDamageSE = new IncreaseDamageStatusEffect(3);
					increaseDamageSE.NumberOfUses = 1;
					increaseDamageSE.TargetCriteria.IsSpecificCard = card;
					increaseDamageSE.SourceCriteria.IsSpecificCard = base.CharacterCard;
					increaseDamageSE.UntilCardLeavesPlay(card);

					IEnumerator increaseDamageCR = AddStatusEffect(increaseDamageSE);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(increaseDamageCR);
					}
					else
					{
						GameController.ExhaustCoroutine(increaseDamageCR);
					}
				}
			}
			yield break;
		}
	}
}