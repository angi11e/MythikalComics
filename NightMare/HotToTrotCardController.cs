using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.NightMare
{
	public class HotToTrotCardController : NightMareBaseCardController
	{
		/*
		 * When this card enters play, {NightMare} deals 1 target 1 Melee damage.
		 * Damage dealt to {NightMare} cannot be increased.
		 * 
		 * POWER
		 * Draw 2 cards. Discard 1.
		 * 
		 * DISCARD
		 * Choose a hero target. Reduce the next damage that would be dealt to that target by 2.
		 */

		public HotToTrotCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// When this card enters play, {NightMare} deals 1 target 1 Melee damage.
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, base.CharacterCard),
				1,
				DamageType.Melee,
				1,
				false,
				1,
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

			yield break;
		}

		public override void AddTriggers()
		{
			/* old version
			// If {NightMare} would be dealt irreducible damage...
			AddTrigger(
				(DealDamageAction dda) =>
					dda.IsIrreducible
					&& dda.Target == base.CharacterCard
					&& dda.DamageSource.Card != base.CharacterCard,
				ConvertDamageResponse,
				new TriggerType[3] {
					TriggerType.WouldBeDealtDamage,
					TriggerType.CancelAction,
					TriggerType.DealDamage
				},
				TriggerTiming.Before
			);
			*/

			// Damage dealt to {NightMare} cannot be increased.
			AddTrigger(
				(DealDamageAction dd) => dd.Target == this.CharacterCard,
				(DealDamageAction dd) => GameController.MakeDamageUnincreasable(dd, GetCardSource()),
				TriggerType.MakeDamageUnincreasable,
				TriggerTiming.Before
			);

			base.AddTriggers();
		}

		/* old version
		private IEnumerator ConvertDamageResponse(DealDamageAction dda)
		{
			int damage = dda.Amount;

			// ...prevent that damage...
			IEnumerator cancelCR = CancelAction(dda, isPreventEffect: true);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(cancelCR);
			}
			else
			{
				GameController.ExhaustCoroutine(cancelCR);
			}
			if (dda.IsPretend)
			{
				yield break;
			}

			// ...then {NightMare} deals herself that much infernal damage.
			IEnumerator selfDamageCR = DealDamage(
				base.CharacterCard,
				base.CharacterCard,
				damage,
				DamageType.Infernal,
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

			yield break;
		}
		*/

		public override IEnumerator UsePower(int index = 0)
		{
			// Draw 2 cards. Discard 1.
			int drawNumeral = GetPowerNumeral(0, 2);
			int discardNumeral = GetPowerNumeral(1, 1);

			IEnumerator drawCardsCR = DrawCards(DecisionMaker, drawNumeral);
			IEnumerator discardCR = SelectAndDiscardCards(DecisionMaker, discardNumeral);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(drawCardsCR);
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(drawCardsCR);
				GameController.ExhaustCoroutine(discardCR);
			}
			yield break;
		}

		protected override IEnumerator DiscardResponse(GameAction ga)
		{
			// Choose a hero target. Reduce the next damage that would be dealt to that target by 2.
			List<SelectTargetDecision> selectedTarget = new List<SelectTargetDecision>();
			IEnumerable<Card> choices = FindCardsWhere(new LinqCardCriteria((Card c) =>
				c.IsTarget && c.IsInPlayAndHasGameText && c.IsHero
			));
			IEnumerator selectTargetCR = GameController.SelectTargetAndStoreResults(
				DecisionMaker,
				choices,
				selectedTarget,
				selectionType: SelectionType.ReduceNextDamageTaken,
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

			if (selectedTarget != null && selectedTarget.Any())
			{
				Card theTarget = selectedTarget.FirstOrDefault().SelectedCard;
				ReduceDamageStatusEffect reduceDamageSE = new ReduceDamageStatusEffect(2);
				reduceDamageSE.TargetCriteria.IsSpecificCard = theTarget;
				reduceDamageSE.NumberOfUses = 1;
				reduceDamageSE.CardDestroyedExpiryCriteria.Card = theTarget;

				IEnumerator addStatusCR = AddStatusEffect(reduceDamageSE);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(addStatusCR);
				}
				else
				{
					GameController.ExhaustCoroutine(addStatusCR);
				}
			}

			yield break;
		}
	}
}