using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Supplicate
{
	public class MultiversalKoansCardController : SupplicateBaseCardController
	{
		/*
		 * each player may destroy one of their ongoing or equipment cards.
		 * any who do may play a card.
		 * 
		 * each hero character may deal themself 2 psychic damage.
		 * any who take damage this way may use a power.
		 * 
		 * each player may discard a card.
		 * any who do may draw a card.
		 */

		public MultiversalKoansCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// each player may destroy one of their ongoing or equipment cards.
			IEnumerator destroyToPlayCR = GameController.SelectTurnTakersAndDoAction(
				DecisionMaker,
				new LinqTurnTakerCriteria(
					(TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame && IsHero(tt)
				),
				SelectionType.DestroyCard,
				DestroyToPlayResponse,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destroyToPlayCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destroyToPlayCR);
			}

			// each hero character may deal themself 2 psychic damage.
			IEnumerator selectHeroesCR = GameController.DealDamageToSelf(
				DecisionMaker,
				(Card c) => IsHeroCharacterCard(c),
				2,
				DamageType.Psychic,
				requiredDecisions: 0,
				addStatusEffect: MayUsePowerResponse,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectHeroesCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectHeroesCR);
			}

			// each player may discard a card.
			List<DiscardCardAction> discardedCards = new List<DiscardCardAction>();
			IEnumerator discardCR = GameController.EachPlayerDiscardsCards(
				0,
				1,
				discardedCards,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			// any who do may draw a card.
			foreach (DiscardCardAction item in discardedCards)
			{
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

			yield break;
		}

		private IEnumerator DestroyToPlayResponse(TurnTaker tt)
		{
			HeroTurnTakerController httc = FindHeroTurnTakerController(tt.ToHero());

			// any who do may play a card.
			List<DestroyCardAction> destroyedCards = new List<DestroyCardAction>();
			IEnumerator destroyCR = GameController.SelectAndDestroyCard(
				httc,
				new LinqCardCriteria((Card c) => c.Owner == tt && (IsOngoing(c) || IsEquipment(c))),
				true,
				destroyedCards,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destroyCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destroyCR);
			}

			if (DidDestroyCard(destroyedCards))
			{
				IEnumerator playCardCR = GameController.SelectAndPlayCardFromHand(
					httc,
					true,
					cardSource: GetCardSource()
				);

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

		private IEnumerator MayUsePowerResponse(DealDamageAction dd)
		{
			// any who take damage this way may use a power.
			if (
				dd.DidDealDamage
				&& !dd.DidDestroyTarget
				&& (!dd.Target.WillBeDestroyed || GameController.IsCardIndestructible(dd.Target))
				&& !dd.Target.IsIncapacitatedOrOutOfGame
			)
			{
				IEnumerator powerCR = SelectAndUsePower(FindCardController(dd.Target));
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(powerCR);
				}
				else
				{
					GameController.ExhaustCoroutine(powerCR);
				}
			}
		}
	}
}