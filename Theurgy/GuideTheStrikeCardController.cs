using System.Collections;
using System.Collections.Generic;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public class GuideTheStrikeCardController : CharmBaseCardController
	{
		// Play this card next to a hero character card.
		// increase the next damage dealt by that hero by 3.
		// That hero gains the [b]power:[/b] destroy this card.
		// Before this card is destroyed, the hero it's next to deals 1 target 5 melee or projectile damage.

		public GuideTheStrikeCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			IncreaseDamageStatusEffect increaseDamageSE = new IncreaseDamageStatusEffect(3);
			increaseDamageSE.NumberOfUses = 1;
			increaseDamageSE.SourceCriteria.IsSpecificCard = CharmedHero();
			increaseDamageSE.CardDestroyedExpiryCriteria.Card = CharmedHero();

			return AddStatusEffect(increaseDamageSE);
		}

		protected override IEnumerator CharmDestroyResponse(GameAction ga)
		{
			HeroTurnTakerController httc = FindHeroTurnTakerController(CharmedHero().Owner.ToHero());

			//Select a damage type
			List<SelectDamageTypeDecision> chosenType = new List<SelectDamageTypeDecision>();
			IEnumerator chooseTypeCR = GameController.SelectDamageType(
				httc,
				chosenType,
				new DamageType[] { DamageType.Melee, DamageType.Projectile },
				cardSource: GetCardSource()
			);
			
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(chooseTypeCR);
			}
			else
			{
				GameController.ExhaustCoroutine(chooseTypeCR);
			}

			DamageType? damageType = GetSelectedDamageType(chosenType);
			if (damageType != null)
			{
				// Hit 1 target for 5 damage of chosen type
				IEnumerator strikeCR = GameController.SelectTargetsAndDealDamage(
					httc,
					new DamageSource(GameController, CharmedHero()),
					5,
					damageType.Value,
					1,
					false,
					1,
					cardSource: GetCardSource()
				);
				
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(strikeCR);
				}
				else
				{
					GameController.ExhaustCoroutine(strikeCR);
				}
			}

			yield break;
		}
	}
}