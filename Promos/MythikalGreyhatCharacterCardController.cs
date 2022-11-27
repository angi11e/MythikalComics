using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LazyFanComix.Greyhat;

namespace Angille.Greyhat
{
	public class MythikalGreyhatCharacterCardController : HeroCharacterCardController
	{
		public MythikalGreyhatCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int playNumeral = GetPowerNumeral(0, 1);

			for (int i = 0; i < playNumeral; i++)
			{
				// Play 1 card.
				List<PlayCardAction> cards = new List<PlayCardAction>();
				IEnumerator playCR = GameController.SelectAndPlayCardsFromHand(
					HeroTurnTakerController,
					1,
					optional: false,
					1,
					isPutIntoPlay: false,
					storedResults: cards,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playCR);
				}

				// Treat any card played this way as if it were the first card played this turn.
				if (cards.Any() && cards.FirstOrDefault().WasCardPlayed && GameController.Game.Journal.PlayCardEntriesThisTurn().Count() > 1)
				{
					Card played = cards.FirstOrDefault().CardToPlay;
					if (played.DoKeywordsContain("link"))
					{
						IEnumerator linkPlayCR = GameController.SelectAndPlayCardsFromHand(
							HeroTurnTakerController,
							1,
							optional: false,
							0,
							null,
							isPutIntoPlay: false,
							null,
							GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(linkPlayCR);
						}
						else
						{
							GameController.ExhaustCoroutine(linkPlayCR);
						}
					}
					else if (played.DoKeywordsContain("network"))
					{
						IEnumerator drawCR = GameController.DrawCards(
							HeroTurnTakerController,
							2,
							optional: true,
							upTo: false,
							null,
							allowAutoDraw: true,
							null,
							GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(drawCR);
						}
						else
						{
							GameController.ExhaustCoroutine(drawCR);
						}
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
					// One Player may Play a card now.
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
					// One hero may deal 1 target 1 energy damage.
					IEnumerator dealDamageCR = GameController.SelectHeroToSelectTargetAndDealDamage(
						DecisionMaker,
						1,
						DamageType.Energy,
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
					break;
				case 2:
					// One player may destroy one of their ongoing or equipment cards.
					List<DestroyCardAction> storedDestroy = new List<DestroyCardAction>();
					List<SelectTurnTakerDecision> storedHero = new List<SelectTurnTakerDecision>();

					IEnumerator destroySelectCR = GameController.SelectHeroToDestroyTheirCard(
						DecisionMaker,
						(httc) => new LinqCardCriteria(
							c => c.Owner == httc.TurnTaker && c.IsInPlayAndHasGameText && (IsEquipment(c) || c.IsOngoing),
							"equipment"
						),
						optionalSelectHero: true,
						additionalCriteria: new LinqTurnTakerCriteria(
							tt => tt.GetCardsWhere(
								(Card c) => c.IsInPlayAndHasGameText && (IsEquipment(c) || c.IsOngoing)
							).Any()
						),
						storedResultsTurnTaker: storedHero,
						storedResultsAction: storedDestroy,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(destroySelectCR);
					}
					else
					{
						GameController.ExhaustCoroutine(destroySelectCR);
					}

					if (DidDestroyCard(storedDestroy))
					{
						// If they do, they may play a different card from their trash.
						Card sacrifice = storedDestroy.FirstOrDefault().CardToDestroy.Card;
						HeroTurnTaker martyr = sacrifice.Owner.ToHero();

						if (martyr.Trash.Cards.Any((Card c) => c.Identifier != sacrifice.Identifier))
						{
							IEnumerator recoverCR = GameController.SelectAndMoveCard(
								FindHeroTurnTakerController(martyr),
								(Card c) => c.Location == martyr.Trash && c.Identifier != sacrifice.Identifier,
								martyr.PlayArea,
								optional: true,
								isPutIntoPlay: false,
								cardSource: GetCardSource()
							);
							if (UseUnityCoroutines)
							{
								yield return GameController.StartCoroutine(recoverCR);
							}
							else
							{
								GameController.ExhaustCoroutine(recoverCR);
							}
						}
					}
					break;
			}
			yield break;
		}
	}
}