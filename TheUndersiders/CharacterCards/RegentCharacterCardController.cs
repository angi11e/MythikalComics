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
	public class RegentCharacterCardController : TheUndersidersVillainCardController
	{
		private const string HasBeenDealtDamage = "HasBeenDealtDamage";

		public RegentCharacterCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			SpecialStringMaker.ShowHasBeenUsedThisTurn(
				HasBeenDealtDamage,
				"{0} has already dealt counter damage this turn.",
				"{0} has not yet dealt counter damage this turn."
			).Condition = () => this.Card.IsInPlayAndNotUnderCard && !this.Card.IsFlipped;

			SpecialStringMaker.ShowSpecialString(
				ShowHeroWithFewestCardsInPlay
			).Condition = () => this.Card.IsInPlayAndNotUnderCard && !this.Card.IsFlipped;
		}

		public string ShowHeroWithFewestCardsInPlay()
		{
			IEnumerable<TurnTaker> enumerable = GameController.FindTurnTakersWhere(
				(TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame && IsHero(tt),
				BattleZone
			);
			List<string> list = new List<string>();
			int num = 999;
			
			foreach (HeroTurnTaker hero in enumerable)
			{
				IEnumerable<Card> cardsWhere = hero.GetCardsWhere((Card c) => c.IsInPlay && c.Location.OwnerTurnTaker == hero);
				List<Card> source = cardsWhere.ToList();
				if (source.Count() < num)
				{
					list.RemoveAll((string htt) => true);
					list.Add(hero.Name);
					num = source.Count();
				}
				else if (source.Count() == num)
				{
					list.Add(hero.Name);
				}
			}
			string text = list.Count().ToString_SingularOrPlural("Hero", "Heroes");
			string text2 = " in play";
			string text3 = " cards";

			return (list.Count() > 0) ? string.Format(
				"{0} with the fewest{3}{2}: {1}.",
				text,
				list.ToRecursiveString(),
				text2,
				text3
			) : "Warning: No heroes found";
		}

		public override void AddSideTriggers()
		{
			if (!this.Card.IsFlipped)
			{
				// The first time {RegentCharacter} is dealt damage by a target each turn, he deals that target 2 lightning damage.
				AddSideTrigger(AddCounterDamageTrigger(
					(DealDamageAction dda) => dda.Target == this.Card && dda.DidDealDamage,
					() => this.Card,
					() => this.Card,
					oncePerTargetPerTurn: true,
					2,
					DamageType.Lightning
				));

				// At the end of the villain turn, the hero with the least cards in their play area deals themself 2 melee damage.
				AddSideTrigger(AddEndOfTurnTrigger(
					(TurnTaker tt) => tt == this.TurnTaker,
					StopHittingYourselfResponse,
					TriggerType.DealDamage
				));

				// Treat {Crown} effects as active. (taken care of by the cards)
			}
			else
			{
				// At the end of each hero's turn, they may discard a card. If that hero does not do so they deal themself 1 melee damage.
				AddSideTrigger(AddEndOfTurnTrigger(
					(TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame && IsHero(tt),
					DiscardOrPainResponse,
					new TriggerType[] {
						TriggerType.DiscardCard,
						TriggerType.DealDamage
					}
				));

			}
			base.AddSideTriggers();
		}

		/*
		private IEnumerator TaserSceptreResponse(DealDamageAction dd)
		{
			SetCardPropertyToTrueIfRealAction(HasBeenDealtDamage);
			IEnumerator retaliationCR = DealDamage(
				this.Card,
				(Card c) => c.IsTarget && c == dd.DamageSource.Card,
				2,
				DamageType.Lightning
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(retaliationCR);
			}
			else
			{
				GameController.ExhaustCoroutine(retaliationCR);
			}

			yield break;
		}
		*/

		private IEnumerator StopHittingYourselfResponse(PhaseChangeAction p)
		{
			List<TurnTaker> storedResults = new List<TurnTaker>();
			IEnumerator findHeroCR = FindHeroWithFewestCardsInPlayArea(storedResults);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(findHeroCR);
			}
			else
			{
				GameController.ExhaustCoroutine(findHeroCR);
			}

			TurnTaker poorSap = storedResults.FirstOrDefault();
			if (poorSap != null)
			{
				IEnumerator stopHittingCR = DealDamage(
					poorSap.CharacterCard,
					poorSap.CharacterCard,
					2,
					DamageType.Melee
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(stopHittingCR);
				}
				else
				{
					GameController.ExhaustCoroutine(stopHittingCR);
				}
			}

			yield break;
		}

		private IEnumerator DiscardOrPainResponse(PhaseChangeAction p)
		{
			List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
			IEnumerator discardCR = SelectAndDiscardCards(
				GameController.FindHeroTurnTakerController(p.ToPhase.TurnTaker.ToHero()),
				1,
				optional: true,
				storedResults: storedResults
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			if (!DidDiscardCards(storedResults))
			{
				IEnumerator stopHittingCR = DealDamage(
					p.ToPhase.TurnTaker.CharacterCard,
					p.ToPhase.TurnTaker.CharacterCard,
					1,
					DamageType.Melee
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(stopHittingCR);
				}
				else
				{
					GameController.ExhaustCoroutine(stopHittingCR);
				}
			}

			yield break;
		}
	}
}
