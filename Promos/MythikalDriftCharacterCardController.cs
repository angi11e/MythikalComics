using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cauldron.Drift;
using Handelabra;

namespace Angille.Drift
{
	public class MythikalDriftCharacterCardController : DriftSubCharacterCardController
	{
		private const string EchoNumerals = "EchoNumerals";
		private const string EchoesThisRound = "EchoesThisRound";
		private const string ErrorMessage = "Cannot retrieve echo information. Please let angille#2846 know what happened.";

		public MythikalDriftCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowSpecialString(
				MakeSpecialString,
				() => true
			).Condition = () => GameController.GetCardPropertyJournalEntryStringList(this.Card, EchoNumerals) != null
				&& GameController.GetCardPropertyJournalEntryStringList(this.Card, EchoNumerals).Any();
		}

		private string MakeSpecialString()
		{
			IEnumerable<string> powerNumerals = GameController.GetCardPropertyJournalEntryStringList(
				this.Card,
				EchoNumerals
			);
			string specialString = "";
			string separator = "";

			if (powerNumerals.Any())
			{
				for (int i = 0; i < powerNumerals.Count(); i++)
				{
					int[] numerals = powerNumerals.ElementAt(i).Where(
						Char.IsDigit
					).Select((char c) => c.ToString().ToInt()).ToArray();

					specialString += separator + GetDescription(numerals);
					separator = "{BR}{BR}";
				}
			}
			else
			{
				specialString = ErrorMessage;
			}

			return specialString;
		}

		public override void AddTriggers()
		{
			// ...whenever you shift {Cauldron.ShiftL}...
			AddTrigger<RemoveTokensFromPoolAction>(
				(RemoveTokensFromPoolAction a) => a.IsSuccessful && a.TokenPool.Identifier == ShiftPoolIdentifier,
				EchoResponse,
				TriggerType.DealDamage,
				TriggerTiming.After
			);

			// ...whenever you shift {Cauldron.ShiftR}...
			AddTrigger<AddTokensToPoolAction>(
				(AddTokensToPoolAction a) => a.IsSuccessful && a.TokenPool.Identifier == ShiftPoolIdentifier,
				EchoResponse,
				TriggerType.GainHP,
				TriggerTiming.After
			);

			// Until the end of your next turn...
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				EndOfTurnResponse,
				TriggerType.RemoveTrigger
			);

			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				StartOfTurnResponse,
				TriggerType.RemoveTrigger
			);

			base.AddTriggers();
		}

		private IEnumerator EchoResponse(ModifyTokensAction action)
		{
			IEnumerable<string> powerNumerals = GameController.GetCardPropertyJournalEntryStringList(
				this.Card,
				EchoNumerals
			);

			if (powerNumerals.Any())
			{
				bool leftShift = action is RemoveTokensFromPoolAction;

				for (int i = 0; i < powerNumerals.Count(); i++)
				{
					int[] numerals = powerNumerals.ElementAt(i).Where(
						Char.IsDigit
					).Select((char c) => c.ToString().ToInt()).ToArray();

					if (leftShift)
					{
						// ...{Drift} deals 1 target 1 radiant damage...
						IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
							DecisionMaker,
							new DamageSource(GameController, this.CharacterCard),
							numerals[1],
							DamageType.Radiant,
							numerals[0],
							false,
							numerals[0],
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
					}
					else
					{
						// ...1 target regains 1 HP.
						IEnumerator gainHPCR = GameController.SelectAndGainHP(
							DecisionMaker,
							numerals[3],
							numberOfTargets: numerals[2],
							requiredDecisions: numerals[2],
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(gainHPCR);
						}
						else
						{
							GameController.ExhaustCoroutine(gainHPCR);
						}
					}
				}
			}

			yield break;
		}

		private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
		{
			// we're eliminating the instances not made this round
			List<string> powerNumerals = GameController.GetCardPropertyJournalEntryStringList(
				this.Card,
				EchoNumerals
			).ToList();
			int echoesThisRound = GetCardPropertyJournalEntryInteger(EchoesThisRound) ?? 0;

			if (echoesThisRound < powerNumerals.Count)
			{
				int removeNumeral = powerNumerals.Count - echoesThisRound;

				IEnumerator messageCR = GameController.SendMessageAction(
					"removing amplified echoes effect." + (removeNumeral > 1 ? $" (x{removeNumeral})" : ""),
					Priority.High,
					GetCardSource()
				);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(messageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(messageCR);
				}

				powerNumerals.RemoveRange(0, removeNumeral);
				GameController.AddCardPropertyJournalEntry(this.Card, EchoNumerals, powerNumerals);
			}

			yield break;
		}

		private IEnumerator StartOfTurnResponse(PhaseChangeAction pca)
		{
			SetCardProperty(EchoesThisRound, 0);
			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int attackNumeral = GetPowerNumeral(0, 1);
			int damageNumeral = GetPowerNumeral(1, 1);
			int healNumeral = GetPowerNumeral(2, 1);
			int hpNumeral = GetPowerNumeral(3, 1);

			// Until the end of your next turn...
			IEnumerator messageCR = GameController.SendMessageAction(
				GetDescription(new int[] { attackNumeral, damageNumeral, healNumeral, hpNumeral }),
				Priority.High,
				GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(messageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(messageCR);
			}

			IEnumerable<string> journalEntry = GameController.GetCardPropertyJournalEntryStringList(
				this.Card,
				EchoNumerals
			);
			List<string> powerNumerals = journalEntry != null ? journalEntry.ToList() : new List<string>();

			powerNumerals.Add($"{attackNumeral}{damageNumeral}{healNumeral}{hpNumeral}");
			GameController.AddCardPropertyJournalEntry(this.Card, EchoNumerals, powerNumerals);

			int echoesThisRound = GetCardPropertyJournalEntryInteger(EchoesThisRound) ?? 0;
			SetCardProperty(EchoesThisRound, ++echoesThisRound);

			yield break;
		}

		private string GetDescription(int[] powerNumerals)
		{
			if (powerNumerals.Length != 4)
			{
				return ErrorMessage;
			}

			return "Whenever you shift {ShiftL}, " + CharacterCard.Title
				+ " deals " + powerNumerals[0] + " " + powerNumerals[0].ToString_TargetOrTargets()
				+ " " + powerNumerals[1] + " radiant damage. Whenever you shift {ShiftR} "
				+ powerNumerals[2] + " " + powerNumerals[2].ToString_TargetOrTargets() + " "
				+ powerNumerals[2].ToString_SingularOrPlural("regains", "regain") + " " + powerNumerals[3] + " HP.";
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One Hero may use a power.
					IEnumerator usePowerCR = GameController.SelectHeroToUsePower(DecisionMaker);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(usePowerCR);
					}
					else
					{
						GameController.ExhaustCoroutine(usePowerCR);
					}
					break;

				case 1:
					// Select a hero target.
					List<SelectCardDecision> heroSelection = new List<SelectCardDecision>();
					IEnumerator selectHeroTargetCR = GameController.SelectCardAndStoreResults(
						DecisionMaker,
						SelectionType.SelectTargetFriendly,
						new LinqCardCriteria(
							(Card c) => c.IsInPlay && IsHeroTarget(c),
							"hero target",
							useCardsSuffix: false,
							plural: "hero targets"
						),
						heroSelection,
						optional: false,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(selectHeroTargetCR);
					}
					else
					{
						GameController.ExhaustCoroutine(selectHeroTargetCR);
					}

					SelectCardDecision selectedHero = heroSelection.FirstOrDefault();
					if (selectedHero != null && selectedHero.SelectedCard != null)
					{
						// Until the start of your next turn, whenever that target gains HP...
						OnGainHPStatusEffect healEffect = new OnGainHPStatusEffect(
							CardWithoutReplacements,
							"RadiantResponse",
							$"Whenever{selectedHero.SelectedCard.Title} gains HP, they deal 1 target 1 radiant damage.",
							new TriggerType[] { TriggerType.DealDamage },
							DecisionMaker.TurnTaker,
							this.Card
						);
						healEffect.TargetCriteria.IsSpecificCard = selectedHero.SelectedCard;
						healEffect.TargetLeavesPlayExpiryCriteria.Card = selectedHero.SelectedCard;
						healEffect.ToTurnPhaseExpiryCriteria.Phase = Phase.Start;
						healEffect.ToTurnPhaseExpiryCriteria.TurnTaker = this.TurnTaker;
						healEffect.AmountCriteria.GreaterThan = 0;
						healEffect.BeforeOrAfter = BeforeOrAfter.After;

						IEnumerator addStatusCR = AddStatusEffect(healEffect);
						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(addStatusCR);
						}
						else
						{
							GameController.ExhaustCoroutine(addStatusCR);
						}
					}
					break;

				case 2:
					// Select a target.
					List<SelectCardDecision> cardSelection = new List<SelectCardDecision>();
					IEnumerator selectTargetCR = GameController.SelectCardAndStoreResults(
						DecisionMaker,
						SelectionType.SelectTargetFriendly,
						new LinqCardCriteria(
							(Card c) => c.IsInPlay && c.IsTarget,
							"target",
							useCardsSuffix: false,
							plural: "targets"
						),
						cardSelection,
						optional: false,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(selectTargetCR);
					}
					else
					{
						GameController.ExhaustCoroutine(selectTargetCR);
					}

					SelectCardDecision selected = cardSelection.FirstOrDefault();
					if (selected != null && selected.SelectedCard != null)
					{
						// Until the start of your next turn, whenever that target is dealt damage...
						OnDealDamageStatusEffect damageEffect = new OnDealDamageStatusEffect(
							CardWithoutReplacements,
							"HealResponse",
							$"Whenever {selected.SelectedCard.Title} is dealt damage, they regain 1 HP.",
							new TriggerType[] { TriggerType.GainHP },
							DecisionMaker.TurnTaker,
							this.Card
						);
						damageEffect.TargetCriteria.IsSpecificCard = selected.SelectedCard;
						damageEffect.TargetLeavesPlayExpiryCriteria.Card = selected.SelectedCard;
						damageEffect.ToTurnPhaseExpiryCriteria.Phase = Phase.Start;
						damageEffect.ToTurnPhaseExpiryCriteria.TurnTaker = this.TurnTaker;
						damageEffect.DamageAmountCriteria.GreaterThan = 0;
						damageEffect.DoesDealDamage = true;
						damageEffect.BeforeOrAfter = BeforeOrAfter.After;

						IEnumerator addStatusCR = AddStatusEffect(damageEffect);
						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(addStatusCR);
						}
						else
						{
							GameController.ExhaustCoroutine(addStatusCR);
						}
					}
					break;
			}
			yield break;
		}

		public IEnumerator HealResponse(DealDamageAction dd, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
		{
			// ...it regains 1 HP.
			IEnumerator healingCR = GameController.GainHP(
				dd.Target,
				1,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(healingCR);
			}
			else
			{
				GameController.ExhaustCoroutine(healingCR);
			}
			yield break;
		}

		public IEnumerator RadiantResponse(GainHPAction gh, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
		{
			// ...it deals 1 target 1 radiant damage.
			IEnumerator damageCR = GameController.SelectTargetsAndDealDamage(
				FindHeroTurnTakerController(gh.HpGainer.Owner.ToHero()),
				new DamageSource(GameController, gh.HpGainer),
				1,
				DamageType.Radiant,
				1,
				false,
				1,
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
			yield break;
		}
	}
}