using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;
using Handelabra;

namespace Angille.CadaverTeam
{
	public class CadaverousWandCardController : CardController
	{
		public CadaverousWandCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			SpecialStringMaker.ShowHighestHP(cardCriteria: new LinqCardCriteria(
				(Card c) => IsHero(c)
			));
		}

		public override void AddTriggers()
		{
			// Increase damage dealt by haunt cards by 1.
			AddIncreaseDamageTrigger(
				(DealDamageAction dda) => dda.DamageSource.Card.DoKeywordsContain("haunt"),
				1
			);

			// Whenever a villain target is destroyed...
			AddTrigger(
				(DestroyCardAction d) => IsVillainTarget(d.CardToDestroy.Card)
					&& d.WasCardDestroyed,
				DestructionResponse,
				TriggerType.DealDamage,
				TriggerTiming.After
			);

			// Whenever a villain character is incapacitated, destroy this card.
			AddTrigger(
				(FlipCardAction fc) => fc.CardToFlip.Card.IsIncapacitated && fc.CardToFlip.Card.IsVillainTeamCharacter,
				(FlipCardAction fc) => GameController.DestroyCard(
					DecisionMaker,
					this.Card,
					cardSource: GetCardSource()
				),
				TriggerType.DestroySelf,
				TriggerTiming.After
			);

			base.AddTriggers();
		}

		private IEnumerator DestructionResponse(DestroyCardAction dca)
		{
			// ...{CadaverTeam} deals the hero target with the highest HP 1 infernal damage.
			IEnumerator dealDamageCR = DealDamageToHighestHP(
				this.CharacterCard,
				1,
				(Card c) => IsHero(c),
				(Card c) => 1,
				DamageType.Infernal
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
