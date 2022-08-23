using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.CaptainCain
{
	public class NoMoreTricksCardController : CaptainCainBaseCardController
	{
		/*
		 * {CaptainCainCharacter} deals himself and 1 other target 2 irreducible psychic damage each.
		 * 
		 * 👊: {CaptainCainCharacter} deals that target 2 melee damage.
		 * 
		 * 💧: {CaptainCainCharacter} deals that target 1 infernal damage, then regains 2 HP.
		 */

		public NoMoreTricksCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// {CaptainCainCharacter} deals himself and 1 other target 2 irreducible psychic damage each.
			IEnumerator selfDamageCR = DealDamage(
				this.CharacterCard,
				this.CharacterCard,
				2,
				DamageType.Psychic,
				true,
				cardSource: GetCardSource()
			);

			List<DealDamageAction> storedDamage = new List<DealDamageAction>();
			IEnumerator otherDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				2,
				DamageType.Psychic,
				1,
				false,
				1,
				true,
				additionalCriteria: (Card c) => c != this.CharacterCard,
				storedResultsDamage: storedDamage,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selfDamageCR);
				yield return GameController.StartCoroutine(otherDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selfDamageCR);
				GameController.ExhaustCoroutine(otherDamageCR);
			}

			DealDamageAction theDamage = storedDamage.FirstOrDefault();
			if (IsFistActive && theDamage != null && !theDamage.Target.IsIncapacitatedOrOutOfGame)
			{
				// 👊: {CaptainCainCharacter} deals that target 2 melee damage.
				IEnumerator fistDamageCR = DealDamage(
					this.CharacterCard,
					theDamage.Target,
					2,
					DamageType.Melee,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(fistDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(fistDamageCR);
				}
			}

			if (IsBloodActive && theDamage != null && !theDamage.Target.IsIncapacitatedOrOutOfGame)
			{
				// 💧: {CaptainCainCharacter} deals that target 1 infernal damage, then regains 2 HP.
				IEnumerator bloodDamageCR = DealDamage(
					this.CharacterCard,
					theDamage.Target,
					1,
					DamageType.Infernal,
					cardSource: GetCardSource()
				);
				IEnumerator healingCR = GameController.GainHP(
					this.CharacterCard,
					2,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(bloodDamageCR);
					yield return GameController.StartCoroutine(healingCR);
				}
				else
				{
					GameController.ExhaustCoroutine(bloodDamageCR);
					GameController.ExhaustCoroutine(healingCR);
				}
			}

			yield break;
		}
	}
}