using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.CaptainCain
{
	public class DoubleEnhancerGlovesCardController : CaptainCainEquipmentCardController
	{
		/*
		 * increase damage dealt by {CaptainCainCharacter} by 1.
		 * 
		 * 👊: increase the next damage dealt by {CaptainCainCharacter} by 2.
		 *     Play 1 card.
		 * 
		 * 💧: the next time {CaptainCainCharacter} would deal damage to a hero target,
		 *    that target regains that many HP instead.
		 */

		public DoubleEnhancerGlovesCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// increase damage dealt by {CaptainCainCharacter} by 1.
			AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource.IsSameCard(base.CharacterCard), 1);

			base.AddTriggers();
		}

		/* UsePower override, in case we need to go that direction again
		public override IEnumerator UsePower(int index = 0)
		{
			IEnumerator messageCR = null;
			switch (index)
			{
				case 0:
					if (IsFistActive)
					{
						int damageNumeral = GetPowerNumeral(0, 2);
						int playNumeral = GetPowerNumeral(1, 1);

						// increase the next damage dealt by {CaptainCainCharacter} by 2.
						IncreaseDamageStatusEffect increaseDamageSE = new IncreaseDamageStatusEffect(damageNumeral);
						increaseDamageSE.SourceCriteria.IsSpecificCard = this.CharacterCard;
						increaseDamageSE.NumberOfUses = 1;
						increaseDamageSE.CreateImplicitExpiryConditions();
						// increaseDamageSE.CardDestroyedExpiryCriteria.Card = this.CharacterCard;
						// increaseDamageSE.CardSource = this.Card;
						IEnumerator increaseDamageCR = GameController.AddStatusEffect(increaseDamageSE, false, GetCardSource());

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(increaseDamageCR);
						}
						else
						{
							GameController.ExhaustCoroutine(increaseDamageCR);
						}

						// Play 1 card.
						IEnumerator playCardsCR = GameController.SelectAndPlayCardsFromHand(
							DecisionMaker,
							playNumeral,
							false,
							cardSource: GetCardSource()
						);
						
						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(playCardsCR);
						}
						else
						{
							GameController.ExhaustCoroutine(playCardsCR);
						}
					}
					else
					{
						messageCR = GameController.SendMessageAction(
							"{Fist} effects are not active, so this power does nothing",
							Priority.Medium,
							GetCardSource(),
							showCardSource: true
						);
					}
					break;

				case 1:
					if (IsBloodActive)
					{
						// the next time {CaptainCainCharacter} would deal damage to a hero target,
						OnDealDamageStatusEffect effect = new OnDealDamageStatusEffect(
							this.CardWithoutReplacements,
							"HealInsteadResponse",
							$"the next hero target dealt damage by {this.CharacterCard.Title} regains that many hp instead.",
							new TriggerType[] { TriggerType.WouldBeDealtDamage },
							null,
							this.Card
						);
						effect.BeforeOrAfter = BeforeOrAfter.Before;
						effect.NumberOfUses = 1;
						effect.TargetCriteria.IsHero = true;
						effect.SourceCriteria.IsSpecificCard = this.CharacterCard;
						effect.UntilTargetLeavesPlay(this.CharacterCard);
						IEnumerator addEffectCR = AddStatusEffect(effect);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(addEffectCR);
						}
						else
						{
							GameController.ExhaustCoroutine(addEffectCR);
						}
					}
					else
					{
						messageCR = GameController.SendMessageAction(
							"{Blood} effects are not active, so this power does nothing",
							Priority.Medium,
							GetCardSource(),
							showCardSource: true
						);
					}
					break;
			}

			if (messageCR != null)
			{
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(messageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(messageCR);
				}
			}

			yield break;
		}
		*/

		protected override IEnumerator FistPower()
		{
			int damageNumeral = GetPowerNumeral(0, 2);
			int playNumeral = GetPowerNumeral(1, 1);

			// increase the next damage dealt by {CaptainCainCharacter} by 2.
			IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(damageNumeral);
			effect.SourceCriteria.IsSpecificCard = this.CharacterCard;
			effect.NumberOfUses = 1;
			effect.UntilTargetLeavesPlay(this.CharacterCard);
			// effect.CardDestroyedExpiryCriteria.Card = this.CharacterCard;
			IEnumerator increaseDamageCR = AddStatusEffect(effect);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(increaseDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(increaseDamageCR);
			}

			// Play 1 card.
			IEnumerator playCardsCR = GameController.SelectAndPlayCardsFromHand(
				DecisionMaker,
				playNumeral,
				false,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playCardsCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playCardsCR);
			}

			yield break;
		}

		protected override IEnumerator BloodPower()
		{
			// the next time {CaptainCainCharacter} would deal damage to a hero target,
			OnDealDamageStatusEffect effect = new OnDealDamageStatusEffect(
				this.CardWithoutReplacements,
				"HealInsteadResponse",
				$"the next hero target dealt damage by {this.CharacterCard.Title} regains that many hp instead.",
				new TriggerType[] { TriggerType.WouldBeDealtDamage },
				null,
				this.Card
			);
			effect.BeforeOrAfter = BeforeOrAfter.Before;
			effect.NumberOfUses = 1;
			effect.TargetCriteria.IsHero = true;
			effect.SourceCriteria.IsSpecificCard = this.CharacterCard;
			effect.UntilTargetLeavesPlay(this.CharacterCard);
			IEnumerator addEffectCR = AddStatusEffect(effect);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(addEffectCR);
			}
			else
			{
				GameController.ExhaustCoroutine(addEffectCR);
			}

			yield break;
		}

		public IEnumerator HealInsteadResponse(
			DealDamageAction dd,
			HeroTurnTaker hero,
			StatusEffect effect,
			int[] powerNumerals = null
		)
		{
			// that target regains that many HP instead.
			IEnumerator cancelCR = CancelAction(
				dd,
				isPreventEffect: true
			);
			IEnumerator healingCR = GameController.GainHP(
				dd.Target,
				dd.Amount,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(cancelCR);
				yield return GameController.StartCoroutine(healingCR);
			}
			else
			{
				GameController.ExhaustCoroutine(cancelCR);
				GameController.ExhaustCoroutine(healingCR);
			}

			yield break;
		}
	}
}