using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.OrbitalAtlantis
{
	public class EmergeFromTheSeaCardController : CardController
	{
		/*
		 * If [i]Ignition[/i] is not in play,
		 * play the top card of the environment deck, 
		 * then shuffle this card into the environment deck. 
		 * otherwise, this card is indestructible.
		 * 
		 * increase all damage dealt by 1.
		 * 
		 * at the start of the environment turn,
		 * add 1 token to each zone card's bias pool.
		 */

		public EmergeFromTheSeaCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
			SetCardProperty("indestructible", false);
		}

		public override bool AskIfCardIsIndestructible(Card card)
		{
			if (card == this.Card)
			{
				bool? indestructible = GameController.GetCardPropertyJournalEntryBoolean(this.Card, "indestructible");
				if (indestructible != null)
				{
					return indestructible == true;
				}
			}
			return false;
		}

		public override void AddTriggers()
		{
			// increase all damage dealt by 1.
			AddIncreaseDamageTrigger((DealDamageAction dda) => true, 1);

			// at the start of the environment turn...
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				FeedZonesResponse,
				TriggerType.AddTokensToPool
			);

			base.AddTriggers();
		}

		public override IEnumerator Play()
		{
			IEnumerable<Card> ignition = GameController.FindCardsWhere(
				(Card c) => c.IsInPlayAndHasGameText && c.Identifier == "Ignition",
				visibleToCard: GetCardSource()
			);

			// If [i]Ignition[/i] is not in play...
			if (!ignition.Any())
			{
				// ...play the top card of the environment deck...
				IEnumerator playEnvCR = GameController.PlayTopCard(DecisionMaker, this.TurnTakerController);

				// ...then shuffle this card into the environment deck.
				IEnumerator shuffleMeCR = GameController.ShuffleCardIntoLocation(
					DecisionMaker,
					this.Card,
					this.Card.NativeDeck,
					false,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playEnvCR);
					yield return GameController.StartCoroutine(shuffleMeCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playEnvCR);
					GameController.ExhaustCoroutine(shuffleMeCR);
				}
			}
			else
			{
				// Otherwise, this card is indestructible.
				SetCardProperty("indestructible", true);
			}

			yield break;
		}

		private IEnumerator FeedZonesResponse(PhaseChangeAction p)
		{
			// add 1 token to each zone card's bias pool.
			var zoneCards = FindCardsWhere(
				new LinqCardCriteria((Card c) => c.DoKeywordsContain("zone") && c.IsInPlayAndHasGameText)
			).ToList();

			for (var i = 0; i < zoneCards.Count(); i++)
			{
				TokenPool biasPool = zoneCards[i].FindTokenPool("bias");
				if (biasPool != null)
				{
					IEnumerator addTokenCR = GameController.AddTokensToPool(
						biasPool,
						1,
						GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(addTokenCR);
					}
					else
					{
						GameController.ExhaustCoroutine(addTokenCR);
					}
				}
			}

			yield break;
		}
	}
}