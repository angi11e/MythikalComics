using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.WhatsHerFace
{
	public class AceUpHerSleeveCardController : WhatsHerFaceBaseCardController
	{
		/*
		 * When this card enters play, one hero other than {WhatsHerFace} may use a power.
		 * 
		 * POWERS
		 *  Draw 1 card.
		 *  Play 1 card.
		 *  Deal 1 target 2 melee damage.
		 */

		public AceUpHerSleeveCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// When this card enters play, one hero other than {WhatsHerFace} may use a power.
			List<SelectCardDecision> cards = new List<SelectCardDecision>();
			IEnumerator selectHeroCR = GameController.SelectCardAndStoreResults(
				DecisionMaker,
				SelectionType.UsePower,
				new LinqCardCriteria(
					(Card c) => c.IsHeroCharacterCard
						&& c != base.CharacterCard
						&& c.IsInPlayAndHasGameText
						&& !c.IsIncapacitatedOrOutOfGame,
					"hero character card other than " + base.CharacterCard.Title
				),
				cards,
				false,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(selectHeroCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(selectHeroCR);
			}

			SelectCardDecision selected = cards.FirstOrDefault();
			if (selected != null && selected.SelectedCard != null)
			{
				IEnumerator grantPowerCR = SelectAndUsePower(
					FindCardController(selected.SelectedCard)
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(grantPowerCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(grantPowerCR);
				}
			}

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			switch (index)
			{
				case 0:
					// Draw 1 card.
					int drawNumeral = GetPowerNumeral(0, 1);
					IEnumerator drawCardCR = DrawCards(base.HeroTurnTakerController, drawNumeral);
					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(drawCardCR);
					}
					else
					{
						base.GameController.ExhaustCoroutine(drawCardCR);
					}
					break;

				case 1:
					// Play 1 card.
					int playNumeral = GetPowerNumeral(0, 1);
					IEnumerator playCardCR = SelectAndPlayCardsFromHand(base.HeroTurnTakerController, playNumeral);
					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(playCardCR);
					}
					else
					{
						base.GameController.ExhaustCoroutine(playCardCR);
					}
					break;

				case 2:
					// Deal 1 target 2 melee damage.
					int targetNumeral = GetPowerNumeral(0, 1);
					int damageNumeral = GetPowerNumeral(1, 2);
					IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
						DecisionMaker,
						new DamageSource(GameController, base.Card),
						damageNumeral,
						DamageType.Melee,
						targetNumeral,
						false,
						targetNumeral,
						cardSource: GetCardSource()
					);
					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(dealDamageCR);
					}
					else
					{
						base.GameController.ExhaustCoroutine(dealDamageCR);
					}
					break;
			}

			yield break;
		}
	}
}