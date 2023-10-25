using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Nexus
{
	public class NexusCharacterCardController : HeroCharacterCardController
	{
		public NexusCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController) {
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int damageNumeral = GetPowerNumeral(1, 1);

			// Select a damage type.
			List<SelectDamageTypeDecision> storedResults = new List<SelectDamageTypeDecision>();
			IEnumerator selectCR = GameController.SelectDamageType(
				HeroTurnTakerController,
				storedResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectCR);
			}
			DamageType damageType = GetSelectedDamageType(storedResults).Value;

			// {Nexus} deals 1 target 1 damage of that type.
			IEnumerator damageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				damageNumeral,
				damageType,
				new int?(targetNumeral),
				false,
				new int?(targetNumeral),
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

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One Player may Play a card now.
					IEnumerator playCardCR = SelectHeroToPlayCard(
						this.HeroTurnTakerController,
						heroCriteria: new LinqTurnTakerCriteria(
							(TurnTaker tt) => IsHero(tt) && !tt.IsIncapacitated
						)
					);
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
					// Select a damage type.
					List<SelectDamageTypeDecision> stored1 = new List<SelectDamageTypeDecision>();
					IEnumerator select1CR = GameController.SelectDamageType(
						DecisionMaker,
						stored1,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(select1CR);
					}
					else
					{
						GameController.ExhaustCoroutine(select1CR);
					}
					DamageType damageType1 = GetSelectedDamageType(stored1).Value;

					// one hero deals 1 target 1 damage of that type.
					IEnumerator dealDamageCR = GameController.SelectHeroToSelectTargetAndDealDamage(
						DecisionMaker,
						1,
						damageType1,
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
					break;

				case 2:
					// Select a damage type.
					List<SelectDamageTypeDecision> stored2 = new List<SelectDamageTypeDecision>();
					IEnumerator select2CR = GameController.SelectDamageType(
						DecisionMaker,
						stored2,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(select2CR);
					}
					else
					{
						GameController.ExhaustCoroutine(select2CR);
					}
					DamageType damageType2 = GetSelectedDamageType(stored2).Value;

					// until the start of your turn, increase all damage of that type by 1.
					IncreaseDamageStatusEffect increaseDamageStatusEffect = new IncreaseDamageStatusEffect(1);
					increaseDamageStatusEffect.UntilStartOfNextTurn(this.TurnTaker);
					increaseDamageStatusEffect.DamageTypeCriteria.AddType(damageType2);
					IEnumerator effectCR = AddStatusEffect(increaseDamageStatusEffect);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(effectCR);
					}
					else
					{
						GameController.ExhaustCoroutine(effectCR);
					}
					break;
			}
			yield break;
		}
	}
}