using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class HalfAPressCardController : SpeedrunnerBaseCardController
	{
		/*
		 * When you move a card to your trash, place it under this card instead.
		 * When you use a power or draw a card, add 1 token to this card.
		 * When this card is destroyed,
		 *  {Speedrunner} deals 1 target X projectile damage,
		 *  and that target deals {Speedrunner} Y radiant damage,
		 *  where X = the number of cards under this one,
		 *  and Y = the number of tokens on this card.
		 *  
		 * POWER
		 * Destroy this card.
		 */

		public HalfAPressCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		private ITrigger _hooverCards;

		public override void AddTriggers()
		{
			// When you move a card to your trash, place it under this card instead.
			_hooverCards = AddTrigger(
				(MoveCardAction mca) =>
					mca.Destination == this.TurnTaker.Trash
					&& mca.ResponsibleTurnTaker == this.TurnTaker
					&& mca.CanChangeDestination
					&& mca.CardToMove != this.Card,
				TrashResponse,
				TriggerType.MoveCard,
				TriggerTiming.Before
			);

			// When you use a power or draw a card, add 1 token to this card.
			AddTrigger(
				(UsePowerAction upa) => upa.HeroUsingPower == this.HeroTurnTakerController,
				(UsePowerAction upa) => GameController.AddTokensToPool(
					this.Card.FindTokenPool("HalfAPressPool"),
					1,
					GetCardSource()
				),
				TriggerType.AddTokensToPool,
				TriggerTiming.After
			);
			AddTrigger(
				(DrawCardAction dca) => dca.HeroTurnTaker == this.HeroTurnTaker,
				(DrawCardAction dca) => GameController.AddTokensToPool(
					this.Card.FindTokenPool("HalfAPressPool"),
					1,
					GetCardSource()
				),
				TriggerType.AddTokensToPool,
				TriggerTiming.After
			);

			// When this card is destroyed...
			AddBeforeDestroyAction(DestructionResponse);
			AddBeforeLeavesPlayActions(ReturnCardsToOwnersTrashResponse);

			base.AddTriggers();
		}

		private IEnumerator TrashResponse(MoveCardAction mca)
		{
			mca.SetDestination(this.Card.UnderLocation);

			return GameController.SendMessageAction(
				this.CharacterCard.Title + " is building speed!",
				Priority.Medium,
				GetCardSource(),
				showCardSource: true
			);
		}

		private IEnumerator DestructionResponse(GameAction ga)
		{
			// where X = the number of cards under this one,
			int buriedNumeral = this.Card.UnderLocation.NumberOfCards;

			// and Y = the number of tokens on this card.
			int tokenNumeral = this.Card.FindTokenPool("HalfAPressPool").CurrentValue;

			// {Speedrunner} deals 1 target X projectile damage,
			List<DealDamageAction> theTarget = new List<DealDamageAction>();
			IEnumerator damageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				buriedNumeral,
				DamageType.Projectile,
				1,
				false,
				1,
				storedResultsDamage: theTarget,
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

			// and that target deals {Speedrunner} Y radiant damage,
			if (theTarget.Any() && !theTarget.FirstOrDefault().DidDestroyTarget)
			{
				Card damageSource = theTarget.FirstOrDefault().Target;
				IEnumerator reflectDamageCR = DealDamage(
					damageSource,
					this.CharacterCard,
					tokenNumeral,
					DamageType.Radiant,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(reflectDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(reflectDamageCR);
				}
			}

			yield break;
		}

		private IEnumerator ReturnCardsToOwnersTrashResponse(GameAction ga)
		{
			RemoveTrigger(_hooverCards);

			while (this.Card.UnderLocation.Cards.Count() > 0)
			{
				Card topCard = this.Card.UnderLocation.TopCard;
				MoveCardDestination trashDestination = FindCardController(topCard).GetTrashDestination();
				IEnumerator returnCR = GameController.MoveCard(
					TurnTakerController,
					topCard,
					trashDestination.Location,
					trashDestination.ToBottom,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(returnCR);
				}
				else
				{
					GameController.ExhaustCoroutine(returnCR);
				}
			}

			TokenPool localPool = this.Card.FindTokenPool("HalfAPressPool");
			if (localPool != null)
			{
				IEnumerator clearTokensCR = GameController.RemoveTokensFromPool(
					localPool,
					localPool.CurrentValue,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(clearTokensCR);
				}
				else
				{
					GameController.ExhaustCoroutine(clearTokensCR);
				}
			}
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// Destroy this card.
			return GameController.DestroyCard(
				DecisionMaker,
				this.Card,
				optional: false,
				cardSource: GetCardSource()
			);
		}
	}
}