using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.OrbitalAtlantis
{
	public class BreakAtmoCardController : CardController
	{
		/*
		 * If [i]Emerge From the Sea[/i] is not in play,
		 * play the top card of the environment deck,
		 * then shuffle this card into the environment deck.
		 * otherwise, this card is indestructible.
		 * 
		 * you may look at the top of each hero deck at any time.
		 *
		 * at the start of the environment turn,
		 * add 1 token to each zone card's bias pool.
		 */

		public BreakAtmoCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
			SetCardProperty("indestructible", false);

			// you may look at the top of each hero deck at any time.
			SpecialStringMaker.ShowSpecialString( BuildTopCardsSpecialString )
				.Condition = () => this.Card.IsInPlayAndHasGameText;
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
			IEnumerable<Card> emerge = GameController.FindCardsWhere(
				(Card c) => c.IsInPlayAndHasGameText && c.Identifier == "EmergeFromTheSea",
				visibleToCard: GetCardSource()
			);

			// If [i]Emerge From the Sea[/i] is not in play...
			if (!emerge.Any())
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
				SetCardProperty( "indestructible", true );
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

		private string BuildTopCardsSpecialString()
		{
			var activeHeroes = FindTurnTakersWhere(
				(TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame && IsHero(tt)
			).ToList();
			string topCardsSpecial = "Top cards of hero decks: ";
			if ( activeHeroes.Any() )
			{
				topCardsSpecial += activeHeroes.FirstOrDefault().Deck.TopCard.Title
					+ " (" + activeHeroes.FirstOrDefault().NameRespectingVariant + ")";
				for ( int i = 1; i < activeHeroes.Count(); i++)
				{
					topCardsSpecial += ", " + activeHeroes[i].Deck.TopCard.Title
						+ " (" + activeHeroes[i].NameRespectingVariant + ")";
				}
			}
			else
			{
				topCardsSpecial += "nothing to report";
			}

			return topCardsSpecial;
		}
	}
}