using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Spoiler
{
	public class ParadoxProtocolsEngageCardController : SpoilerOneshotCardController
	{
		public ParadoxProtocolsEngageCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// You may discard a card. If you do, activate a [u]rewind[/u] text.
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(DiscardToRewind());
			}
			else
			{
				GameController.ExhaustCoroutine(DiscardToRewind());
			}

			// For each card destroyed this way...
			ITrigger destroyTrigger = AddTrigger(
				(DestroyCardAction d) => d.WasCardDestroyed
					&& d.CardSource != null
					&& d.CardSource.CardController == this,
				DestructionResponse,
				TriggerType.AddTokensToPool,
				TriggerTiming.After
			);

			// Destroy any number of your ongoing and equipment cards.
			IEnumerator destroyCR = GameController.SelectAndDestroyCards(
				DecisionMaker,
				new LinqCardCriteria(
					(Card c) => c.Owner == this.Card.Owner && (IsOngoing(c) || IsEquipment(c)),
					"ongoing or equipment"
				),
				null,
				requiredDecisions: 0,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destroyCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destroyCR);
			}

			RemoveTrigger(destroyTrigger);
			yield break;
		}

		private IEnumerator DestructionResponse(DestroyCardAction d)
		{
			// ...{Spoiler} deals 1 target 2 energy damage...
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				2,
				DamageType.Energy,
				1,
				false,
				1,
				cardSource: GetCardSource()
			);

			// ...then regains 2 HP.
			IEnumerator healingCR = GameController.GainHP(
				this.CharacterCard,
				2,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(dealDamageCR);
				yield return GameController.StartCoroutine(healingCR);
			}
			else
			{
				GameController.ExhaustCoroutine(dealDamageCR);
				GameController.ExhaustCoroutine(healingCR);
			}

			yield break;
		}
	}
}