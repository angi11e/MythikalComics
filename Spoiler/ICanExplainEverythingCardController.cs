using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Spoiler
{
	public class ICanExplainEverythingCardController : SpoilerOngoingCardController
	{
		public ICanExplainEverythingCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// when this card enters play each player may...
			IEnumerator selectPlayersCR = GameController.SelectTurnTakersAndDoAction(
				DecisionMaker,
				new LinqTurnTakerCriteria((TurnTaker tt) => IsHero(tt) && !tt.ToHero().IsIncapacitatedOrOutOfGame),
				SelectionType.MoveCardToHandFromTrash,
				MoveCardToHandResponse,
				allowAutoDecide: true,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectPlayersCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectPlayersCR);
			}

			yield break;
		}

		private IEnumerator MoveCardToHandResponse(TurnTaker tt)
		{
			// ...move a card with a power on it from their trash to their hand.
			IEnumerator getPowerCR = GameController.SelectCardsFromLocationAndMoveThem(
				FindHeroTurnTakerController(tt.ToHero()),
				tt.Trash,
				0,
				1,
				new LinqCardCriteria((Card c) => c.Location == tt.Trash && c.HasPowers),
				new MoveCardDestination[] { new MoveCardDestination(tt.ToHero().Hand) },
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(getPowerCR);
			}
			else
			{
				GameController.ExhaustCoroutine(getPowerCR);
			}

			yield break;
		}

		public override IEnumerator ActivateRewind()
		{
			// One hero deals themself 2 psychic damage.
			List<SelectCardDecision> storedHero = new List<SelectCardDecision>();
			IEnumerator selectCR = GameController.SelectCardAndStoreResults(
				DecisionMaker,
				SelectionType.DealDamageSelf,
				new LinqCardCriteria(
					(Card c) => c.IsInPlayAndHasGameText
						&& IsHeroCharacterCard(c)
						&& !c.IsIncapacitatedOrOutOfGame
						&& c.Owner.IsHero,
					"heroes to select",
					useCardsSuffix: false
				),
				storedHero,
				optional: false,
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

			if (storedHero.Any() && storedHero.FirstOrDefault().SelectedCard != null)
			{
				Card heroCard = storedHero.FirstOrDefault().SelectedCard;
				HeroTurnTakerController httc = FindHeroTurnTakerController(heroCard.Owner.ToHero());

				List<DealDamageAction> storedDamage = new List<DealDamageAction>();
				IEnumerator selfDamageCR = DealDamage(
					heroCard,
					heroCard,
					2,
					DamageType.Psychic,
					storedResults: storedDamage,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(selfDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(selfDamageCR);
				}

				// If they take damage this way...
				if (DidDealDamage(storedDamage, heroCard, heroCard))
				{
					// ...they may use a power.
					IEnumerator powerCR = GameController.SelectAndUsePower(
						httc,
						cardSource: GetCardSource()
					);

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

			yield break;
		}
	}
}