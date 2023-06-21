using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Spoiler
{
	public class IveSeenTheMomentsCardController : SpoilerOngoingCardController
	{
		public IveSeenTheMomentsCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// when this card enters play, each player may draw a card.
			IEnumerator drawCR = EachPlayerDrawsACard(
				(HeroTurnTaker htt) => !htt.IsIncapacitatedOrOutOfGame && IsHero(htt),
				true
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(drawCR);
			}
			else
			{
				GameController.ExhaustCoroutine(drawCR);
			}

			yield break;
		}

		public override IEnumerator ActivateRewind()
		{
			// One hero deals themself 2 energy damage.
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
					DamageType.Energy,
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
					// ...they may play a card.
					IEnumerator playCR = GameController.SelectAndPlayCardFromHand(
						httc,
						optional: true,
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
				}
			}

			yield break;
		}
	}
}