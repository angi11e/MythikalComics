using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.NightMare
{
	public class MoonStalkerCharacterCardController : HeroCharacterCardController
	{
		public MoonStalkerCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 3);
			int damageNumeral = GetPowerNumeral(1, 1);

			// Each time a target is destroyed this way, draw or discard a card.
			ITrigger destroyTrigger = AddTrigger(
				(DestroyCardAction d) => d.WasCardDestroyed && d.CardSource != null && d.CardSource.CardController == this,
				(DestroyCardAction d) => DrawDiscardResponse(),
				new List<TriggerType> { TriggerType.DrawCard, TriggerType.DiscardCard },
				TriggerTiming.After
			);

			// [i]Moon Stalker[/i] deals up to 3 targets 1 melee damage each.
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				damageNumeral,
				DamageType.Melee,
				targetNumeral,
				false,
				0,
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

			RemoveTrigger(destroyTrigger);
			yield break;
		}

		private IEnumerator DrawDiscardResponse()
		{
			List<Function> choices = new List<Function>();
			choices.Add(new Function(
				this.HeroTurnTakerController,
				"Draw a card",
				SelectionType.DrawCard,
				() => DrawCard(this.HeroTurnTaker),
				CanDrawCards(this.HeroTurnTakerController),
				$"{this.Card.Title} has no cards to discard, so must draw a card."
			));
			choices.Add(new Function(
				this.HeroTurnTakerController,
				"Discard a card",
				SelectionType.DiscardCard,
				() => SelectAndDiscardCards(this.HeroTurnTakerController, 1),
				this.HeroTurnTaker.HasCardsInHand,
				$"{this.Card.Title} cannot draw cards, so must discard a card."
			));

			SelectFunctionDecision drawOrDiscard = new SelectFunctionDecision(
				GameController,
				this.HeroTurnTakerController,
				choices,
				false,
				null,
				$"{this.Card.Title} cannot draw nor discard cards.",
				cardSource: GetCardSource()
			);
			IEnumerator drawDiscardCR = GameController.SelectAndPerformFunction(drawOrDiscard);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(drawDiscardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(drawDiscardCR);
			}

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One player may play a card.
					IEnumerator playCR = SelectHeroToPlayCard(DecisionMaker);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(playCR);
					}
					else
					{
						GameController.ExhaustCoroutine(playCR);
					}
					break;

				case 1:
					// Destroy an ongoing card.
					IEnumerator destroyOngoingCR = GameController.SelectAndDestroyCard(
						DecisionMaker,
						new LinqCardCriteria((Card c) => c.IsInPlay && IsOngoing(c), "ongoing"),
						false,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(destroyOngoingCR);
					}
					else
					{
						GameController.ExhaustCoroutine(destroyOngoingCR);
					}
					break;

				case 2:
					// Move a card from a trash pile to the bottom of its deck.
					List<SelectLocationDecision> storedResults = new List<SelectLocationDecision>();
					IEnumerator selectTrashCR = GameController.SelectATrash(
						DecisionMaker,
						SelectionType.MoveTrashToDeck,
						(Location l) => true,
						storedResults,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(selectTrashCR);
					}
					else
					{
						GameController.ExhaustCoroutine(selectTrashCR);
					}

					if (storedResults.FirstOrDefault() != null)
					{
						Location selectedTrash = storedResults.FirstOrDefault().SelectedLocation.Location;
						Location selectedDeck = (
							(!selectedTrash.IsSubLocation) ?
							selectedTrash.OwnerTurnTaker.Deck :
							selectedTrash.OwnerTurnTaker.FindSubDeck(selectedTrash.Identifier)
						);
						List<MoveCardDestination> deckBottom = new List<MoveCardDestination>
						{
							new MoveCardDestination(selectedDeck, true)
						};

						IEnumerator moveTrashCR = GameController.SelectCardFromLocationAndMoveIt(
							DecisionMaker,
							selectedTrash,
							new LinqCardCriteria((Card c) => true),
							deckBottom,
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(moveTrashCR);
						}
						else
						{
							GameController.ExhaustCoroutine(moveTrashCR);
						}
					}
					break;
			}
			yield break;
		}
	}
}