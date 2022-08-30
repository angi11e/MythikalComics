using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.NightMare
{
	public class ReinedCombatCardController : NightMareBaseCardController
	{
		/*
		 * When this card enters play, Draw a card then Discard a card.
		 * 
		 * POWER
		 * {NightMare} deals 1 target 4 Melee damage.
		 * increase the next damage dealt by that target by 5.
		 * 
		 * DISCARD
		 * increase damage dealt by {NightMare} by 1 until the start of your next turn.
		 */

		public ReinedCombatCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// When this card enters play, Draw a card...
			IEnumerator drawCardsCR = DrawCards(DecisionMaker, 1);
			// ...then Discard a card.
			IEnumerator discardCR = SelectAndDiscardCards(DecisionMaker, 1);

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

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int damageNumeral = GetPowerNumeral(1, 4);
			int bonusNumeral = GetPowerNumeral(2, 5);

			// {NightMare} deals 1 target 4 Melee damage.
			List<DealDamageAction> attacks = new List<DealDamageAction>();
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				damageNumeral,
				DamageType.Melee,
				targetNumeral,
				false,
				targetNumeral,
				storedResultsDamage: attacks,
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

			// increase the next damage dealt by that target by 5.
			List<Card> attacked = (from dd in attacks where dd.DidDealDamage select dd.Target).Distinct().ToList();
			if (attacked.Count() > 0)
			{
				IncreaseDamageStatusEffect increaseDamageSE = new IncreaseDamageStatusEffect(bonusNumeral);
				increaseDamageSE.NumberOfUses = 1;
				increaseDamageSE.SourceCriteria.IsOneOfTheseCards = attacked;
				increaseDamageSE.UntilCardLeavesPlay(attacked.FirstOrDefault());

				IEnumerator addStatusCR = AddStatusEffect(increaseDamageSE);
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

		protected override IEnumerator DiscardResponse(GameAction ga)
		{
			// increase damage dealt by {NightMare} by 1 until the start of your next turn.
			IncreaseDamageStatusEffect increaseDamageSE = new IncreaseDamageStatusEffect(1);
			increaseDamageSE.UntilStartOfNextTurn(TurnTaker);
			increaseDamageSE.SourceCriteria.IsSpecificCard = this.CharacterCard;
			increaseDamageSE.UntilTargetLeavesPlay(this.CharacterCard);

			IEnumerator addStatusCR = AddStatusEffect(increaseDamageSE);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(addStatusCR);
			}
			else
			{
				GameController.ExhaustCoroutine(addStatusCR);
			}

			yield break;
		}
	}
}