using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Athena
{
	public class PallasCardController : AspectBaseCardController
	{
		/*
		 * When this card enters play, destroy any other [u]aspect[/u] cards.
		 * 
		 * The first time each turn a villain target deals damage to a hero character other than {Athena},
		 *  {Athena} deals that target 2 melee damage.
		 * 
		 * POWER
		 * {Athena} deals 1 target 3 melee damage.
		 *  If that Target dealt a hero target Damage since your last turn, this damage is irreducible.
		 */

		private const string HasDealtRetributionDamage = "HasDealtRetributionDamage";

		public PallasCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// The first time each turn a villain target deals damage to a hero character other than {Athena}...
			AddTrigger(
				(DealDamageAction dd) =>
					dd.Target.IsHeroCharacterCard
					&& dd.Target != base.CharacterCard
					&& dd.DidDealDamage
					&& dd.DamageSource.IsVillainTarget
					&& !IsPropertyTrue(HasDealtRetributionDamage),
				RetributionResponse,
				TriggerType.DealDamage,
				TriggerTiming.After,
				ActionDescription.DamageTaken
			);
			AddAfterLeavesPlayAction(
				(GameAction ga) => ResetFlagAfterLeavesPlay(HasDealtRetributionDamage),
				TriggerType.Hidden
			);

			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// {Athena} deals 1 target 3 melee damage.
			int targetNumeral = GetPowerNumeral(0, 1);
			int damageNumeral = GetPowerNumeral(1, 3);

			for (int i = 0; i < targetNumeral; i++)
			{
				List<SelectTargetDecision> selectedTarget = new List<SelectTargetDecision>();
				IEnumerable<Card> choices = base.FindCardsWhere(
					new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlayAndHasGameText)
				);
				IEnumerator selectTargetCR = base.GameController.SelectTargetAndStoreResults(
					base.HeroTurnTakerController,
					choices,
					selectedTarget,
					selectionType: SelectionType.SelectTarget,
					cardSource: base.GetCardSource()
				);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(selectTargetCR);
				}
				else
				{
					GameController.ExhaustCoroutine(selectTargetCR);
				}

				if (selectedTarget != null && selectedTarget.Any())
				{
					SelectTargetDecision selectedTargetDecision = selectedTarget.FirstOrDefault();
					if (selectedTargetDecision != null && selectedTargetDecision.SelectedCard != null)
					{
						IEnumerator dealDamageCR = DealDamage(
							base.CharacterCard,
							(Card c) => c.IsTarget && c == selectedTargetDecision.SelectedCard,
							damageNumeral,
							DamageType.Melee,
							EligibleForIrreducible(selectedTargetDecision.SelectedCard)
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
				}
			}

			yield break;
		}

		private bool EligibleForIrreducible(Card theTarget)
		{
			// If that Target dealt a hero target Damage since your last turn, this damage is irreducible.
			IEnumerable<DealDamageJournalEntry> entries = GameController.Game.Journal.QueryJournalEntries(
				(DealDamageJournalEntry e) => e.TargetCard.IsHero && e.SourceCard.Equals(theTarget)
			).Where(GameController.Game.Journal.SinceLastTurn<DealDamageJournalEntry>(base.TurnTaker));

			return entries.Any();
		}

		private IEnumerator RetributionResponse(DealDamageAction dd)
		{
			SetCardPropertyToTrueIfRealAction(HasDealtRetributionDamage);

			// {Athena} deals that target 2 melee damage.
			IEnumerator dealDamageCR = DealDamage(
				base.CharacterCard,
				(Card c) => c == dd.DamageSource.Card && c.IsTarget,
				2,
				DamageType.Melee
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