using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Spoiler
{
	public class EncyclopaediaChronomicaCardController : SpoilerEquipmentCardController
	{
		public EncyclopaediaChronomicaCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			// You may look at the top card of the environment deck at any time.
			SpecialStringMaker.ShowListOfCards(
				new LinqCardCriteria(
					(Card c) => c == FindEnvironment().TurnTaker.Deck.TopCard,
					"top card of environment deck",
					useCardsSuffix: false
				)
			).Condition = () => this.Card.IsInPlayAndHasGameText;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int damageNumeral = GetPowerNumeral(0, 1);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(PowerText(damageNumeral));
			}
			else
			{
				GameController.ExhaustCoroutine(PowerText(damageNumeral));
			}

			yield break;
		}

		private IEnumerator PowerText(int damageNumeral)
		{
			// Move the top card of the environment deck to the bottom.
			IEnumerator buryCardCR = GameController.MoveCard(
				DecisionMaker,
				FindEnvironment().TurnTaker.Deck.TopCard,
				FindEnvironment().TurnTaker.Deck,
				true,
				showMessage: true,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(buryCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(buryCardCR);
			}

			// {Spoiler} may deal himself 1 irreducible energy damage.
			List<DealDamageAction> storedDamage = new List<DealDamageAction>();
			IEnumerator selfDamageCR = DealDamage(
				this.CharacterCard,
				this.CharacterCard,
				damageNumeral,
				DamageType.Energy,
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

			// If he takes damage this way...
			if (DidDealDamage(storedDamage, this.CharacterCard, this.CharacterCard))
			{
				// ...repeat the game text of this power.
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(PowerText(damageNumeral));
				}
				else
				{
					GameController.ExhaustCoroutine(PowerText(damageNumeral));
				}
			}

			yield break;
		}
	}
}