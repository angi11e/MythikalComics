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
					(Card c) => c.IsInPlayAndHasGameText
						&& c != this.CharacterCard
						&& IsHeroCharacterCard(c)
						&& !c.IsIncapacitatedOrOutOfGame,
					"hero character card other than " + this.CharacterCard.Title
				),
				cards,
				false,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectHeroCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectHeroCR);
			}

			SelectCardDecision selected = cards.FirstOrDefault();
			if (selected != null && selected.SelectedCard != null)
			{
				IEnumerator grantPowerCR = SelectAndUsePower(
					FindCardController(selected.SelectedCard)
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(grantPowerCR);
				}
				else
				{
					GameController.ExhaustCoroutine(grantPowerCR);
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
					IEnumerator drawCardCR = DrawCards(DecisionMaker, drawNumeral);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(drawCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(drawCardCR);
					}
					break;

				case 1:
					// Play 1 card.
					int playNumeral = GetPowerNumeral(0, 1);
					IEnumerator playCardCR = SelectAndPlayCardsFromHand(DecisionMaker, playNumeral);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(playCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(playCardCR);
					}
					break;

				case 2:
					// Deal 1 target 2 melee damage.
					int targetNumeral = GetPowerNumeral(0, 1);
					int damageNumeral = GetPowerNumeral(1, 2);
					IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
						DecisionMaker,
						new DamageSource(GameController, this.CharacterCard),
						damageNumeral,
						DamageType.Melee,
						targetNumeral,
						false,
						targetNumeral,
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
					break;
			}

			yield break;
		}
	}
}