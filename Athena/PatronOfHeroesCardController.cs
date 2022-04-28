using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Athena
{
	public class PatronOfHeroesCardController : AthenaBaseCardController
	{
		/*
		 * You may discard a card.
		 *  If you do, one player other than you may draw 2 cards.
		 * 
		 * {Athena} may deal herself 2 irreducible psychic damage.
		 *  If she does, one hero target other than {Athena} may regain 4 HP.
		 *  
		 * {Athena} may deal 1 hero character target 2 irreducible radiant damage.
		 *  If she does, that hero may play a card or use a power now.
		 */

		public PatronOfHeroesCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// You may discard a card.
			List<DiscardCardAction> storedCards = new List<DiscardCardAction>();
			IEnumerator discardCR = SelectAndDiscardCards(
				HeroTurnTakerController,
				1,
				optional: true,
				null,
				storedCards
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			// If you do, one player other than you may draw 2 cards.
			if (DidDiscardCards(storedCards, 1))
			{
				IEnumerator drawCR = GameController.SelectHeroToDrawCards(
					DecisionMaker,
					2,
					additionalCriteria: new LinqTurnTakerCriteria((TurnTaker tt) => tt != base.TurnTaker),
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(drawCR);
				}
				else
				{
					GameController.ExhaustCoroutine(drawCR);
				}
			}

			// {Athena} may deal herself 2 irreducible psychic damage.
			List<DealDamageAction> storedDamage = new List<DealDamageAction>();
			IEnumerator selfDamageCR = DealDamage(
				base.CharacterCard,
				base.CharacterCard,
				2,
				DamageType.Psychic,
				isIrreducible: true,
				optional: true,
				storedResults: storedDamage
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selfDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selfDamageCR);
			}

			// If she does, one hero target other than {Athena} may regain 4 HP.
			if (DidDealDamage(storedDamage, base.CharacterCard, base.CharacterCard))
			{
				IEnumerator healCR = GameController.SelectAndGainHP(
					DecisionMaker,
					4,
					optional: false,
					(Card c) => c.IsInPlay && c.IsHero && c.IsTarget && c != base.CharacterCard,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(healCR);
				}
				else
				{
					GameController.ExhaustCoroutine(healCR);
				}
			}

			// {Athena} may deal 1 hero character target 2 irreducible radiant damage.
			storedDamage = new List<DealDamageAction>();
			IEnumerator friendlyDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, base.CharacterCard),
				2,
				DamageType.Radiant,
				1,
				optional: true,
				0,
				isIrreducible: true,
				additionalCriteria: (Card c) => c.IsHeroCharacterCard && c.IsInPlayAndHasGameText && c.IsTarget,
				storedResultsDamage: storedDamage,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(friendlyDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(friendlyDamageCR);
			}

			// If she does, that hero may play a card or use a power now.
			DealDamageAction dealDamageAction = storedDamage.FirstOrDefault();
			if (dealDamageAction != null && dealDamageAction.DidDealDamage && dealDamageAction.Target.IsHeroCharacterCard)
			{
				HeroTurnTakerController httc = FindHeroTurnTakerController(dealDamageAction.Target.Owner.ToHero());
				List<Function> list = new List<Function>();

				list.Add(new Function(
					httc,
					"Play a card",
					SelectionType.PlayCard,
					() => SelectAndPlayCardFromHand(httc),
					GameController.CanPlayCards(httc, GetCardSource())
				));

				list.Add(new Function(
					httc,
					"Use a power",
					SelectionType.UsePower,
					() => GameController.SelectAndUsePower(
						httc,
						optional: true,
						cardSource: GetCardSource()
					),
					GameController.CanUsePowers(httc, GetCardSource())
				));

				if (list.Count() > 0)
				{
					SelectFunctionDecision selectFunction = new SelectFunctionDecision(
						GameController,
						httc,
						list,
						optional: true,
						cardSource: GetCardSource()
					);

					IEnumerator selectCR = GameController.SelectAndPerformFunction(selectFunction);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(selectCR);
					}
					else
					{
						GameController.ExhaustCoroutine(selectCR);
					}
				}
			}

			yield break;
		}
	}
}