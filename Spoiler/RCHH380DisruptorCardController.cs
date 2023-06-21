using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Spoiler
{
	public class RCHH380DisruptorCardController : SpoilerEquipmentCardController
	{
		public RCHH380DisruptorCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// increase energy damage dealt by 1.
			AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageType == DamageType.Energy, 1);

			// when this card is destroyed, you may draw a card.
			AddWhenDestroyedTrigger(
				(DestroyCardAction dca) => DrawCard(this.HeroTurnTaker, true),
				TriggerType.DrawCard
			);

			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 2);
			int projectileNumeral = GetPowerNumeral(1, 1);
			int energyNumeral = GetPowerNumeral(2, 2);

			// {Spoiler} deals up to 2 targets 1 projectile damage each.
			List<DealDamageAction> storedResults = new List<DealDamageAction>();
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				projectileNumeral,
				DamageType.Projectile,
				targetNumeral,
				false,
				0,
				storedResultsDamage: storedResults,
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

			// Any target that takes damage this way deals themself 2 energy damage.
			List<Card> retargets = (
				from dd
				in storedResults
				where dd.DidDealDamage
				select dd.Target
			).Distinct().ToList();

			IEnumerator selfDamageCR = GameController.DealDamageToSelf(
				DecisionMaker,
				(Card c) => retargets.Contains(c),
				energyNumeral,
				DamageType.Energy,
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
	}
}