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
	public class GrueCharacterCardController : TheUndersidersVillainCardController
	{
		public GrueCharacterCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			SpecialStringMaker.ShowHeroWithMostCards(false).Condition = () => !this.Card.IsFlipped;
			SpecialStringMaker.ShowVillainTargetWithLowestHP().Condition = () => this.Card.IsFlipped;
		}

		public override IEnumerator Play()
		{
			// When this card enters play, if it's not already in play, search the villain trash and deck for the card Tenebrous Cloud and put it into play. if the villain deck was searched, shuffle it.
			IEnumerator getCloudCR = PlayCardFromLocations(
				new Location[2]
				{
					this.TurnTaker.Trash,
					this.TurnTaker.Deck
				},
				"TenebrousCloud"
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(getCloudCR);
			}
			else
			{
				GameController.ExhaustCoroutine(getCloudCR);
			}

			yield break;
		}

		public override void AddSideTriggers()
		{
			if (!this.Card.IsFlipped)
			{
				// At the end of the villain turn, {Grue} deals the hero character with the most cards in their play area 1 melee damage and 1 infernal damage.
				AddSideTrigger(AddEndOfTurnTrigger(
					(TurnTaker tt) => tt == this.TurnTaker,
					InfernalStrikeResponse,
					TriggerType.DealDamage
				));

				// Treat {Skull} effects as active. (this is done by the cards)
			}
			else
			{
				// At the end of the villain turn, the villain target with the lowest HP deals the {H - 1} hero targets with the lowest HP 1 infernal damage.
				AddSideTrigger(AddEndOfTurnTrigger(
					(TurnTaker tt) => tt == this.TurnTaker,
					FromDarknessResponse,
					TriggerType.DealDamage
				));
			}
			base.AddSideTriggers();
		}

		private IEnumerator InfernalStrikeResponse(PhaseChangeAction p)
		{
			List<DealDamageAction> damageInfo = new List<DealDamageAction>
			{
				new DealDamageAction(
					GetCardSource(),
					new DamageSource(GameController, this.Card),
					null,
					1,
					DamageType.Melee
				),
				new DealDamageAction(
					GetCardSource(),
					new DamageSource(GameController, this.Card),
					null,
					1,
					DamageType.Infernal
				)
			};

			List<Card> storedResults = new List<Card>();
			IEnumerator findTargetCR = FindHeroTargetWithMostCardsInPlay(
				storedResults,
				SelectionType.SelectTarget,
				damageInfo: damageInfo.First()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(findTargetCR);
			}
			else
			{
				GameController.ExhaustCoroutine(findTargetCR);
			}

			IEnumerator infernalStrikeCR = DealMultipleInstancesOfDamage(
				damageInfo,
				(Card c) => storedResults.Contains(c),
				numberOfTargets: 1
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(infernalStrikeCR);
			}
			else
			{
				GameController.ExhaustCoroutine(infernalStrikeCR);
			}

			yield break;
		}

		private IEnumerator FromDarknessResponse(PhaseChangeAction p)
		{
			IEnumerator dealDamageCR = DealDamageToLowestHP(
				null,
				1,
				(Card c) => IsHeroTarget(c),
				(Card c) => 1,
				DamageType.Infernal,
				numberOfTargets: H - 1,
				damageSourceInfo: new TargetInfo(
					HighestLowestHP.LowestHP,
					1,
					1,
					new LinqCardCriteria((Card c) => IsVillainTarget(c), "The villain target with the lowest HP")
				)
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(dealDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(dealDamageCR);
			}

			yield break;
		}
	}
}
