using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Patina
{
	public class TempestuousTossCardController : PatinaBaseCardController
	{
		/*
		 * Discard the top card of each deck.
		 * Each player may discard a card. Those who do may draw a card.
		 * 
		 * You may destroy a hero ongoing or equipment card.
		 *  If you do so, {Patina} deals up to X targets 3 projectile damage each,
		 *  where X = the number of water cards in play plus 1.
		 */

		public TempestuousTossCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsInPlay(IsWaterCriteria((Card c) => !c.IsOneShot));
		}

		public override IEnumerator Play()
		{
			// Discard the top card of each deck.
			IEnumerator discardTopsCR = GameController.DiscardTopCardsOfDecks(
				DecisionMaker,
				(Location l) => !l.OwnerTurnTaker.IsIncapacitatedOrOutOfGame,
				1,
				responsibleTurnTaker: this.TurnTaker,
				cardSource: GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardTopsCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardTopsCR);
			}

			// Each player may discard a card.
			List<DiscardCardAction> discardResults = new List<DiscardCardAction>();
			IEnumerator discardToDrawCR = GameController.EachPlayerDiscardsCards(
				0,
				1,
				discardResults,
				cardSource: GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardToDrawCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardToDrawCR);
			}

			foreach (DiscardCardAction item in discardResults)
			{
				// Those who do may draw a card.
				if (item.WasCardDiscarded)
				{
					IEnumerator drawCR = DrawCards(item.HeroTurnTakerController, 1, true);
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

			// You may destroy a hero ongoing or equipment card.
			List<DestroyCardAction> destroyResults = new List<DestroyCardAction>();
			IEnumerator destructionCR = GameController.SelectAndDestroyCard(
				DecisionMaker,
				new LinqCardCriteria((Card c) => IsHero(c) && (IsOngoing(c) || IsEquipment(c)), "hero ongoing or equipment"),
				optional: true,
				destroyResults,
				null,
				GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destructionCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destructionCR);
			}

			// If you do so...
			if (DidDestroyCards(destroyResults, 1))
			{
				// ...{Patina} deals up to X targets 3 projectile damage each...
				IEnumerator damageCR = GameController.SelectTargetsAndDealDamage(
					DecisionMaker,
					new DamageSource(GameController, this.CharacterCard),
					(Card c) => 3,
					DamageType.Projectile,
					// ...where X = the number of water cards in play plus 1.
					() => WaterCardsInPlay + 1,
					optional: false,
					0,
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

			yield break;
		}
	}
}