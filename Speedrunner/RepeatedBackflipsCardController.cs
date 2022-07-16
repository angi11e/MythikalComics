using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class RepeatedBackflipsCardController : SpeedrunnerBaseCardController
	{
		/*
		 * Draw a card. Discard a card. Play a card.
		 * {Speedrunner} may deal zirself 2 irreducible melee damage. If ze does, repeat the above text.
		 * One player other than {Speedrunner} may use a power now.
		 */

		public RepeatedBackflipsCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// Draw a card.
			IEnumerator drawCR = DrawCard(HeroTurnTaker);

			// Discard a card.
			IEnumerator discardCR = SelectAndDiscardCards(DecisionMaker, 1);

			// Play a card.
			IEnumerator playCR = GameController.SelectAndPlayCardFromHand(
				DecisionMaker,
				false,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(drawCR);
				yield return GameController.StartCoroutine(discardCR);
				yield return GameController.StartCoroutine(playCR);
			}
			else
			{
				GameController.ExhaustCoroutine(drawCR);
				GameController.ExhaustCoroutine(discardCR);
				GameController.ExhaustCoroutine(playCR);
			}

			// {Speedrunner} may deal zirself 2 irreducible melee damage.
			List<DealDamageAction> storedDamage = new List<DealDamageAction>();
			IEnumerator selfDamageCR = GameController.DealDamageToSelf(
				DecisionMaker,
				(Card c) => c == this.CharacterCard,
				2,
				DamageType.Melee,
				true,
				storedDamage,
				true,
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

			if (storedDamage.Any() && storedDamage.FirstOrDefault().DidDealDamage)
			{
				// If ze does, repeat the above text.
				IEnumerator drawCR2 = DrawCard(HeroTurnTaker);
				IEnumerator discardCR2 = SelectAndDiscardCards(DecisionMaker, 1);
				IEnumerator playCR2 = GameController.SelectAndPlayCardFromHand(
					DecisionMaker,
					false,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(drawCR2);
					yield return GameController.StartCoroutine(discardCR2);
					yield return GameController.StartCoroutine(playCR2);
				}
				else
				{
					GameController.ExhaustCoroutine(drawCR2);
					GameController.ExhaustCoroutine(discardCR2);
					GameController.ExhaustCoroutine(playCR2);
				}
			}

			// One player other than {Speedrunner} may use a power now.
			IEnumerator powerCR = GameController.SelectHeroToUsePower(
				DecisionMaker,
				additionalCriteria: new LinqTurnTakerCriteria((TurnTaker tt) => tt != this.TurnTaker),
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(powerCR);
			}
			else
			{
				GameController.ExhaustCoroutine(powerCR);
			}

			yield break;
		}
	}
}