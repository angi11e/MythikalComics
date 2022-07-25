using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Speedrunner
{
	public class GunClubSpeedrunnerCharacterCardController : HeroCharacterCardController
	{
		public GunClubSpeedrunnerCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// Until the start of your next turn,
			// when {Speedrunner} would be dealt damage by a target,
			// ze first deals that target 1 projectile damage.
			int[] powerNumerals = new int[1] { GetPowerNumeral(0, 1) };
			OnDealDamageStatusEffect onDealDamageStatusEffect = new OnDealDamageStatusEffect(
				this.CardWithoutReplacements,
				"AimbotResponse",
				"Whenever a target deals damage to " + this.Card.Title + ", ze first deals that target 1 projectile damage.",
				new TriggerType[1] { TriggerType.DealDamage },
				this.TurnTaker,
				this.Card,
				powerNumerals
			);

			onDealDamageStatusEffect.SourceCriteria.IsTarget = true;
			onDealDamageStatusEffect.TargetCriteria.IsSpecificCard = this.Card;
			onDealDamageStatusEffect.DamageAmountCriteria.GreaterThan = 0;
			onDealDamageStatusEffect.UntilStartOfNextTurn(this.TurnTaker);
			onDealDamageStatusEffect.UntilTargetLeavesPlay(this.Card);
			onDealDamageStatusEffect.BeforeOrAfter = BeforeOrAfter.Before;
			onDealDamageStatusEffect.DoesDealDamage = true;

			IEnumerator addStatusCR = AddStatusEffect(onDealDamageStatusEffect);
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

		public IEnumerator AimbotResponse(DealDamageAction dd, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
		{
			int? num = null;
			if (powerNumerals != null)
			{
				num = powerNumerals.ElementAtOrDefault(0);
			}
			if (!num.HasValue)
			{
				num = 1;
			}

			if (dd.DamageSource.IsCard)
			{
				Card source = this.Card;
				if (hero != null && hero.IsHero)
				{
					source = hero.CharacterCard;
				}

				IEnumerator dealDamageCR = DealDamage(
					source,
					dd.DamageSource.Card,
					num.Value,
					DamageType.Projectile,
					isCounterDamage: true,
					cardSource: GetCardSource(effect)
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(dealDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(dealDamageCR);
				}

				if (dd.DamageSource.Card.IsIncapacitatedOrOutOfGame)
				{
					AddInhibitorException((GameAction ga) => ga is CancelAction);
					IEnumerator cancelCR = CancelAction(dd);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(cancelCR);
					}
					else
					{
						GameController.ExhaustCoroutine(cancelCR);
					}
					RemoveInhibitorException();
				}
			}

			yield break;
		}


		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One hero may use a power.
					IEnumerator playCardCR = GameController.SelectHeroToUsePower(DecisionMaker);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(playCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(playCardCR);
					}
					break;
				case 1:
					// Until the start of your turn, increase all projectile damage by 2.
					break;
				case 2:
					// Put one card from the villain or environment trash into play.
					break;
			}
			yield break;
		}
	}
}