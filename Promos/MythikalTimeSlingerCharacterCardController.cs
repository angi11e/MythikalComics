using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.ChronoRanger
{
	public class MythikalChronoRangerCharacterCardController : HeroCharacterCardController
	{
		public MythikalChronoRangerCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int damageNumeral = GetPowerNumeral(0, 1);

			// Return a bounty card to your hand.
			IEnumerator selectCR = GameController.SelectAndMoveCard(
				DecisionMaker,
				(Card c) => c.IsInPlayAndHasGameText && c.IsBounty,
				this.HeroTurnTaker.Hand,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectCR);
			}

			// Play a bounty card.
			List<PlayCardAction> storedResults = new List<PlayCardAction>();
			IEnumerator playCR = SelectAndPlayCardFromHand(
				DecisionMaker,
				false,
				storedResults,
				new LinqCardCriteria((Card c) => c.IsBounty, "bounty")
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playCR);
			}

			// If you do so...
			if (storedResults.Any() && storedResults.FirstOrDefault().WasCardPlayed)
			{
				Card bounty = storedResults.FirstOrDefault().CardToPlay;
				Card target = bounty.Location.IsNextToCard ? bounty.Location.OwnerCard : null;

				// ...[i]Time-Slinger[/i] deals that bounty's target 1 projectile damage.
				if (target != null && target.IsTarget)
				{
					IEnumerator damageCR = DealDamage(
						this.CharacterCard,
						target,
						damageNumeral,
						DamageType.Projectile,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(damageCR);
					}
					else
					{
						GameController.ExhaustCoroutine(damageCR);
					}
				}
			}

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One player may draw a card.
					IEnumerator drawCardCR = GameController.SelectHeroToDrawCard(
						DecisionMaker,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(drawCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(drawCardCR);
					}
					break;

				case 1:
					// Move a card from a trash to the top of its deck.
					List<SelectLocationDecision> storedResults = new List<SelectLocationDecision>();
					List<SelectCardDecision> selectCardDecision = new List<SelectCardDecision>();

					// select the card
					IEnumerator selectCardCR = GameController.SelectCardAndStoreResults(
						this.HeroTurnTakerController,
						SelectionType.MoveCardOnDeck,
						new LinqCardCriteria(
							(Card c) => c.IsInTrash
							&& GameController.IsLocationVisibleToSource(c.Location, GetCardSource())
						),
						selectCardDecision,
						false
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(selectCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(selectCardCR);
					}

					if (!DidSelectCard(selectCardDecision))
					{
						yield break;
					}

					List<MoveCardDestination> list = new List<MoveCardDestination>
					{
						new MoveCardDestination(GetSelectedCard(selectCardDecision).NativeDeck)
					};

					// move the card
					IEnumerator moveCardCR = GameController.MoveCard(
						this.TurnTakerController,
						selectCardDecision.FirstOrDefault().SelectedCard,
						list.FirstOrDefault().Location,
						doesNotEnterPlay: true,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(moveCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(moveCardCR);
					}
					break;

				case 2:
					// Each player may...
					IEnumerator discardPlayCR = GameController.SelectTurnTakersAndDoAction(
						DecisionMaker,
						new LinqTurnTakerCriteria((TurnTaker tt) =>
							IsHero(tt)
							&& !tt.IsIncapacitatedOrOutOfGame
							&& tt.ToHero().Hand.NumberOfCards >= 2
						),
						SelectionType.DiscardCard,
						ReturnAndPlayResponse,
						requiredDecisions: 0,
						allowAutoDecide: true,
						ignoreBattleZone: false,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(discardPlayCR);
					}
					else
					{
						GameController.ExhaustCoroutine(discardPlayCR);
					}
					break;
			}
			yield break;
		}

		private IEnumerator ReturnAndPlayResponse(TurnTaker tt)
		{
			HeroTurnTakerController httc = FindHeroTurnTakerController(tt.ToHero());
			List<SelectCardDecision> selected = new List<SelectCardDecision>();

			// ...move one of their non-character cards in play to their hand.
			IEnumerator moveCR = GameController.SelectAndMoveCard(
				httc,
				(Card c) => c.Owner == httc.HeroTurnTaker && c.IsInPlayAndNotUnderCard && !c.IsCharacter,
				httc.HeroTurnTaker.Hand,
				optional: true,
				storedResults: selected,
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

			// Any who do...
			if (selected.Any() && GetSelectedCard(selected).Location == httc.HeroTurnTaker.Hand)
			{
				// ...may play a different card.
				IEnumerator playCR = SelectAndPlayCardFromHand(
					httc,
					cardCriteria: new LinqCardCriteria((Card c) => c != GetSelectedCard(selected))
				);
				
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playCR);
				}
			}
		}
	}
}