using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Patina
{
	public class MaerynianImmunityCardController : PatinaBaseCardController
	{
		/*
		 * After {Patina} deals Damage, she becomes immune to that type of Damage.
		 * Whenever {Patina} deals a different type of Damage, her Damage immunity changes to that type.
		 * 
		 * Whenever a hero target would be dealt damage {Patina} is immune to, you may
		 *  redirect it to {Patina}. If you do so, destroy a water card or this card.
		 */

		public MaerynianImmunityCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			this.AllowFastCoroutinesDuringPretend = false;

			SpecialStringMaker.ShowSpecialString(
				() => "Patina has not dealt damage since " + this.Card.Title + " entered play.",
				IsFirstOrOnlyCopyOfThisCardInPlay
			).Condition = () => this.Card.IsInPlayAndHasGameText && !GetDamageTypeThatPatinaIsImmuneTo().HasValue;

			SpecialStringMaker.ShowSpecialString(
				() => "Patina is currently immune to " + GetDamageTypeThatPatinaIsImmuneTo().ToString() + " damage.",
				IsFirstOrOnlyCopyOfThisCardInPlay,
				() => new Card[1] { this.CharacterCard }
			).Condition = () => this.Card.IsInPlayAndHasGameText && GetDamageTypeThatPatinaIsImmuneTo().HasValue;
		}

		public override void AddTriggers()
		{
			// After {Patina} deals Damage, she becomes immune to that type of Damage.
			AddTrigger(
				(DealDamageAction dealDamage) => dealDamage.Target == this.CharacterCard,
				ImmuneResponse,
				TriggerType.ImmuneToDamage,
				TriggerTiming.Before,
				ActionDescription.DamageTaken
			);

			// Whenever {Patina} deals a different type of Damage, her Damage immunity changes to that type.
			AddTrigger(
				(DealDamageAction dealDamage) =>
					this.CharacterCard.IsInPlayAndHasGameText
					&& dealDamage.DamageSource.Card == this.CharacterCard
					&& dealDamage.DidDealDamage,
				WarningMessageResponse,
				TriggerType.Hidden,
				TriggerTiming.After
			);

			// Whenever a hero target would be dealt damage {Patina} is immune to...
			AddTrigger(
				(DealDamageAction dd) =>
					IsHeroTarget(dd.Target)
					&& dd.Target != this.CharacterCard
					&& dd.Amount > 0
					&& dd.DamageType == GetDamageTypeThatPatinaIsImmuneTo(),
				RedirectResponse,
				TriggerType.RedirectDamage,
				TriggerTiming.Before,
				ActionDescription.DamageTaken
			);

			base.AddTriggers();
		}

		private IEnumerator RedirectResponse(DealDamageAction dd)
		{
			// you may redirect it to {Patina}.
			var storedYesNo = new List<YesNoCardDecision> { };
			IEnumerator yesOrNoCR = GameController.MakeYesNoCardDecision(
				DecisionMaker,
				SelectionType.RedirectDamage,
				this.CharacterCard,
				action: dd,
				storedResults: storedYesNo,
				cardSource: GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(yesOrNoCR);
			}
			else
			{
				GameController.ExhaustCoroutine(yesOrNoCR);
			}

			// If you do so, destroy a water card or this card.
			if (DidPlayerAnswerYes(storedYesNo))
			{
				IEnumerator redirectCR = GameController.RedirectDamage(
					dd,
					this.CharacterCard,
					false,
					GetCardSource()
				);
				IEnumerator destroyCR = GameController.SelectAndDestroyCard(
					DecisionMaker,
					new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && (IsWater(c) || c == this.Card)),
					false,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(redirectCR);
					yield return GameController.StartCoroutine(destroyCR);
				}
				else
				{
					GameController.ExhaustCoroutine(redirectCR);
					GameController.ExhaustCoroutine(destroyCR);
				}
			}

			yield break;
		}

		private IEnumerator WarningMessageResponse(DealDamageAction dealDamage)
		{
			if (IsFirstOrOnlyCopyOfThisCardInPlay() && dealDamage.DamageType != GetDamageTypeThatPatinaIsImmuneTo())
			{
				Log.Debug("Patina used Maerynian Immunity to become immune to " + dealDamage.DamageType.ToString() + " damage. ");
				yield return GameController.SendMessageAction(
					"Patina used Maerynian Immunity to become immune to " + dealDamage.DamageType.ToString() + " damage.",
					Priority.High,
					GetCardSource()
				);
			}
			yield return null;
		}

		private IEnumerator ImmuneResponse(DealDamageAction dealDamage)
		{
			DamageType? damageTypeThatPatinaIsImmuneTo = GetDamageTypeThatPatinaIsImmuneTo();
			if (
				damageTypeThatPatinaIsImmuneTo.HasValue
				&& IsFirstOrOnlyCopyOfThisCardInPlay()
				&& dealDamage.DamageType == damageTypeThatPatinaIsImmuneTo.Value
			)
			{
				yield return GameController.ImmuneToDamage(dealDamage, GetCardSource());
			}
		}

		private DamageType? GetDamageTypeThatPatinaIsImmuneTo()
		{
			DealDamageJournalEntry dealDamageJournalEntry = GameController.Game.Journal.MostRecentDealDamageEntry(
				(DealDamageJournalEntry e) => e.SourceCard == this.CharacterCard && e.Amount > 0
			);
			PlayCardJournalEntry playCardJournalEntry = GameController.Game.Journal.QueryJournalEntries(
				(PlayCardJournalEntry e) => e.CardPlayed == this.Card
			).LastOrDefault();
			
			if (playCardJournalEntry != null)
			{
				int? entryIndex = GameController.Game.Journal.GetEntryIndex(dealDamageJournalEntry);
				int? entryIndex2 = GameController.Game.Journal.GetEntryIndex(playCardJournalEntry);
				if (entryIndex.HasValue && entryIndex2.HasValue && entryIndex.Value > entryIndex2.Value)
				{
					return dealDamageJournalEntry.DamageType;
				}
			}
			return null;
		}
	}
}