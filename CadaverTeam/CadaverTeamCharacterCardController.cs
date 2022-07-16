using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;
using Handelabra;

namespace Angille.CadaverTeam
{
	public class CadaverTeamCharacterCardController : VillainTeamCharacterCardController
	{
		public CadaverTeamCharacterCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			if (this.TurnTaker.IsAdvanced)
			{
				GameController.AddCardControllerToList(CardControllerListType.MakesIndestructible, this);

				SpecialStringMaker.ShowHighestHP(cardCriteria: new LinqCardCriteria(
					(Card c) => c.IsHero
				)).Condition = () => this.Card.IsFlipped;
				SpecialStringMaker.ShowLowestHP(cardCriteria: new LinqCardCriteria(
					(Card c) => c.IsHero
				)).Condition = () => this.Card.IsFlipped;

				SpecialStringMaker.ShowHeroWithMostCards(false).Condition = () => !this.Card.IsFlipped;
			}
		}

		public override void AddSideTriggers()
		{
			if (!this.Card.IsFlipped)
			{
				// At the end of {CadaverTeam}'s turn...
				AddSideTrigger(AddEndOfTurnTrigger(
					(TurnTaker tt) => tt == this.TurnTaker,
					EndOfTurnResponse,
					new TriggerType[] { TriggerType.DealDamage, TriggerType.PlayCard }
				));

				// CHALLENGE
				// Villain Haunt cards are immune to melee and projectile damage.
				if (this.TurnTaker.IsChallenge)
				{
					AddSideTrigger(AddImmuneToDamageTrigger(
						(DealDamageAction dd) =>
							dd.Target.DoKeywordsContain("haunt")
							&& dd.Target.IsVillain
							&& (dd.DamageType == DamageType.Melee || dd.DamageType == DamageType.Projectile)
					));
				}
			}
			else
			{
				// At the start of {CadaverTeam}'s turn...
				AddSideTrigger(AddStartOfTurnTrigger(
					(TurnTaker tt) => tt == this.TurnTaker,
					StartTurnIncapResponse,
					TriggerType.DealDamage
				));
			}

			base.AddSideTriggers();
		}

		public override bool AskIfCardIsIndestructible(Card card)
		{
			// ADVANCED
			// Villain Prop cards are indestructible.
			if (this.TurnTaker.IsAdvanced && card.DoKeywordsContain("prop") && card.IsVillain)
			{
				return true;
			}

			return base.AskIfCardIsIndestructible(card);
		}

		private IEnumerator EndOfTurnResponse(PhaseChangeAction p)
		{
			// ...this card deals the hero target with the most cards in play 2 infernal damage.
			IEnumerator dealDamageCR = DealDamageToMostCardsInPlay(
				this.CharacterCard,
				1,
				null,
				2,
				DamageType.Infernal
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(dealDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(dealDamageCR);
			}

			// Then, if there are no non-character targets in {CadaverTeam}'s play area...
			if (FindCardsWhere((Card c) =>
				c.IsAtLocationRecursive(this.TurnTaker.PlayArea)
				&& c.IsTarget
				&& !c.IsCharacter
				&& c.IsInPlayAndNotUnderCard
			).Count() == 0)
			{
				// ...play the top card of {CadaverTeam}'s deck.
				IEnumerator playCardCR = GameController.PlayTopCard(
					DecisionMaker,
					this.TurnTakerController,
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

		private IEnumerator StartTurnIncapResponse(PhaseChangeAction p)
		{
			// ...the hero target with the highest HP deals themself 2 psychic damage...
			List<Card> storeHighest = new List<Card>();
			IEnumerator findTopCR = GameController.FindTargetWithHighestHitPoints(
				1,
				(Card c) => c.IsHero,
				storeHighest,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(findTopCR);
			}
			else
			{
				GameController.ExhaustCoroutine(findTopCR);
			}

			if (storeHighest.FirstOrDefault() == null)
			{
				yield break;
			}

			Card highestTarget = storeHighest.FirstOrDefault();
			IEnumerator selfDamageCR = GameController.DealDamageToSelf(
				DecisionMaker,
				(Card c) => c == highestTarget,
				2,
				DamageType.Psychic,
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

			// ...then deals the hero target with the lowest HP 1 infernal damage.
			if (highestTarget.IsTarget)
			{
				IEnumerator lowestDamageCR = DealDamageToLowestHP(
					highestTarget,
					1,
					(Card c) => c.IsHero,
					(Card c) => 1,
					DamageType.Infernal
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(lowestDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(lowestDamageCR);
				}
			}

			yield break;
		}
	}
}
