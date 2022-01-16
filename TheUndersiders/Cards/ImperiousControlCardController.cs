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
	public class ImperiousControlCardController : TheUndersidersBaseCardController
	{
		public ImperiousControlCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Whenever that hero uses a power, they must discard a card.
			AddTrigger(
				delegate (UsePowerAction p)
				{
					Card cardThisCardIsNextTo = GetCardThisCardIsNextTo();
					TurnTakerController turnTakerController = p.Power.TurnTakerController;
					if (cardThisCardIsNextTo != null && cardThisCardIsNextTo.Owner != null && cardThisCardIsNextTo.Owner.IsHero)
					{
						TurnTakerController turnTakerController2 = FindTurnTakerController(cardThisCardIsNextTo.Owner);
						if (turnTakerController == turnTakerController2)
						{
							if (turnTakerController.HasMultipleCharacterCards)
							{
								return p.Power.CardController.Card == cardThisCardIsNextTo;
							}
							return true;
						}
					}
					return false;
				},
				(UsePowerAction p) => SelectAndDiscardCards(
					FindHeroTurnTakerController(GetCardThisCardIsNextTo().Owner.ToHero()),
					1
				),
				TriggerType.DiscardCard,
				TriggerTiming.After
			);

			// Crown: Whenever that hero plays a card, they deal themself 2 psychic damage.
			AddTrigger(
				delegate (PlayCardAction p)
				{
					if (!IsEnabled("crown"))
					{
						return false;
					}
					Card cardThisCardIsNextTo = GetCardThisCardIsNextTo();
					TurnTakerController turnTakerController = p.TurnTakerController;
					if (cardThisCardIsNextTo != null && cardThisCardIsNextTo.Owner != null && cardThisCardIsNextTo.Owner.IsHero)
					{
						TurnTakerController turnTakerController2 = FindTurnTakerController(cardThisCardIsNextTo.Owner);
						if (turnTakerController == turnTakerController2)
						{
							return true;
						}
					}
					return false;
				},
				(PlayCardAction p) => GameController.SelectTargetsToDealDamageToSelf(
					FindHeroTurnTakerController(GetCardThisCardIsNextTo().Owner.ToHero()),
					2,
					DamageType.Psychic,
					1,
					optional: false,
					1,
					cardSource: GetCardSource()
				),
				TriggerType.DealDamage,
				TriggerTiming.After
			);

			// Mask: Whenever that hero draws a card, {ImpCharacter} deals them 2 melee damage.
			AddTrigger(
				delegate (DrawCardAction d)
				{
					if (!IsEnabled("mask"))
					{
						return false;
					}
					Card cardThisCardIsNextTo = GetCardThisCardIsNextTo();
					TurnTakerController turnTakerController = FindTurnTakerController(d.HeroTurnTaker);
					if (cardThisCardIsNextTo != null && cardThisCardIsNextTo.Owner != null && cardThisCardIsNextTo.Owner.IsHero)
					{
						TurnTakerController turnTakerController2 = FindTurnTakerController(cardThisCardIsNextTo.Owner);
						if (turnTakerController == turnTakerController2)
						{
							return true;
						}
					}
					return false;
				},
				(DrawCardAction d) => ImpHurtsThemResponse(d),
				TriggerType.DealDamage,
				TriggerTiming.After
			);

			AddIfTheTargetThatThisCardIsNextToLeavesPlayDestroyThisCardTrigger();
			base.AddTriggers();
		}

		private IEnumerator ImpHurtsThemResponse(DrawCardAction d)
		{
			Card maybeImp = ImpCharacter;
			Card heroTarget = GetCardThisCardIsNextTo();
			if (maybeImp.IsFlipped)
			{
				List<Card> villainList = new List<Card>();
				IEnumerator findVillainCR = GameController.FindTargetWithHighestHitPoints(
					1,
					(Card c) => c.IsVillainCharacterCard,
					villainList,
					cardSource: GetCardSource()
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(findVillainCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(findVillainCR);
				}

				maybeImp = villainList.FirstOrDefault();
			}

			if (maybeImp.IsTarget && heroTarget.IsTarget)
			{
				IEnumerator impHurtsThemCR = DealDamage(
					maybeImp,
					heroTarget,
					2,
					DamageType.Melee,
					cardSource: GetCardSource()
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(impHurtsThemCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(impHurtsThemCR);
				}
			}

			yield break;
		}

		public override IEnumerator DeterminePlayLocation(
			List<MoveCardDestination> destination,
			bool isPutIntoPlay,
			List<IDecision> decisionSources,
			Location overridePlayArea = null,
			LinqTurnTakerCriteria additionalTurnTakerCriteria = null
		)
		{
			// When this card enters play, move it next to the hero character with the fewest cards in play.
			List<TurnTaker> storedResults = new List<TurnTaker>();
			IEnumerator findHeroCR = FindHeroWithFewestCardsInPlay(storedResults);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(findHeroCR);
			}
			else
			{
				GameController.ExhaustCoroutine(findHeroCR);
			}

			if (storedResults.Count > 0 && destination != null)
			{
				TurnTaker tt = storedResults.First();
				List<Card> storedCharacter = new List<Card>();
				IEnumerator getCardCR = FindCharacterCard(tt, SelectionType.MoveCardNextToCard, storedCharacter);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(getCardCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(getCardCR);
				}

				Card card = storedCharacter.FirstOrDefault();
				if (card != null)
				{
					destination.Add(new MoveCardDestination(card.NextToLocation));
				}
			}
			yield break;
		}

		public override IEnumerator Play()
		{
			yield break;
		}
	}
}
