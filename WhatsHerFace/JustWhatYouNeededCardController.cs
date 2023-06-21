using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.WhatsHerFace
{
	public class JustWhatYouNeededCardController : WhatsHerFaceBaseCardController
	{
		/*
		 * 1 player selects a keyword.
		 * That player reveals cards from the top of their deck until they reveal 2 cards with that keyword.
		 * Put one of them either into play or into their hand.
		 * Shuffle the rest of the revealed cards into their deck.
		 */

		public JustWhatYouNeededCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// NOTE: reference Wager Master's Pick a Card, Pick a Fate
			// 1 player selects a keyword.
			List<SelectTurnTakerDecision> storedResults = new List<SelectTurnTakerDecision>();
			IEnumerator selectTurnTakerCR = GameController.SelectHeroTurnTaker(
				DecisionMaker,
				SelectionType.RevealCardsFromDeck,
				false,
				false,
				storedResults,
				new LinqTurnTakerCriteria((TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame),
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectTurnTakerCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectTurnTakerCR);
			}

			if (!DidSelectTurnTaker(storedResults))
			{
				yield break;
			}

			HeroTurnTakerController httc = FindHeroTurnTakerController(
				storedResults.FirstOrDefault().SelectedTurnTaker.ToHero()
			);
			IOrderedEnumerable<string> words = from s in httc.TurnTaker.Deck.Cards.SelectMany(
				(Card c) => GameController.GetAllKeywords(c)
			).Distinct() orderby s select s;

			List<SelectWordDecision> wordResults = new List<SelectWordDecision>();
			IEnumerator selectWordCR = GameController.SelectWord(
				httc,
				words,
				SelectionType.SelectKeyword,
				wordResults,
				optional: false,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectWordCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectWordCR);
			}

			if (!DidSelectWord(wordResults))
			{
				yield break;
			}

			string keyword = GetSelectedWord(wordResults);

			// NOTE: reference Expatriette's Arsenal Access
			// That player reveals cards from the top of their deck until they reveal 2 cards with that keyword.
			// Put one of them either into play or into their hand.
			// Shuffle the rest of the revealed cards into their deck.
			IEnumerator discoverCR = RevealCards_SelectSome_MoveThem_ReturnTheRest(
				httc,
				httc,
				httc.TurnTaker.Deck,
				(Card c) => GameController.GetAllKeywords(c).Contains(keyword),
				2,
				1,
				canPutInHand: true,
				canPlayCard: true,
				isPutIntoPlay: true,
				"cards with the keyword " + keyword
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discoverCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discoverCR);
			}

			yield break;
		}
	}
}