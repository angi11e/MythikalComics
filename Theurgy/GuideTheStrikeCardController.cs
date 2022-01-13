using System.Collections;
using System.Collections.Generic;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public class GuideTheStrikeCardController : CharmBaseCardController
	{
		// Play this card next to a hero character card.
		// Increase damage dealt by hero targets in that hero's play area by 1.
		// That hero gains the following power:
		// Power: This hero deals 1 target 5 melee or projectile damage.
		//  Destroy this card.

		public GuideTheStrikeCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		protected override string CharmPowerText => "This hero deals 1 target 5 melee or projectile damage. Destroy Guide the Strike.";
		
		public override void AddTriggers()
		{
			base.AddTriggers();

			AddIncreaseDamageTrigger(
				(DealDamageAction dd) =>
					dd.DamageSource.Card.IsHero && dd.DamageSource.Card.IsTarget &&
					dd.DamageSource.IsOneOfTheseCards(GetCardThisCardIsNextTo().Owner.GetPlayAreaCards()),
				1
			);
		}

		protected override IEnumerator CharmPowerResponse(CardController cc)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int damageNumeral = GetPowerNumeral(1, 5);

			HeroTurnTakerController hero = cc.HeroTurnTakerController;
			//Select a damage type
			List<SelectDamageTypeDecision> chosenType = new List<SelectDamageTypeDecision>();
			IEnumerator chooseTypeCR = GameController.SelectDamageType(
				hero,
				chosenType,
				new DamageType[] { DamageType.Melee, DamageType.Projectile },
				cardSource: GetCardSource()
			);
			
			if (UseUnityCoroutines) {
				yield return GameController.StartCoroutine(chooseTypeCR);
			} else {
				GameController.ExhaustCoroutine(chooseTypeCR);
			}

			DamageType? damageType = GetSelectedDamageType(chosenType);
			if (damageType != null)
			{
				// Hit 1 target for 5 damage of chosen type
				IEnumerator strikeCR = GameController.SelectTargetsAndDealDamage(
					hero,
					new DamageSource(GameController, hero.CharacterCard),
					damageNumeral,
					damageType.Value,
					targetNumeral,
					false,
					targetNumeral,
					cardSource: GetCardSource()
				);
				
				if (UseUnityCoroutines) {
					yield return GameController.StartCoroutine(strikeCR);
				} else {
					GameController.ExhaustCoroutine(strikeCR);
				}
			}

			IEnumerator destructionCR = GameController.DestroyCard(
				hero,
				base.Card,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines) {
				yield return GameController.StartCoroutine(destructionCR);
			} else {
				GameController.ExhaustCoroutine(destructionCR);
			}
			yield break;
		}
	}
}