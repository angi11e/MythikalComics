using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Starblade
{
	public class EstrellaPacificaCardController : CardController
	{
		private Card _selectedTarget { get; set; }
		private DamageType? _selectedDamageType { get; set; }

		/*
		* whenever {Starblade} would deal damage, you may change its type to melee or energy.
		* 
		* at the start of your turn, each construct card regains 1 hp.
		* 
		* POWER
		* move all postura cards from your trash into your hand.
		* you may play a postura card.
		*/

		public EstrellaPacificaCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			AllowFastCoroutinesDuringPretend = false;
		}

		public override void AddTriggers()
		{
			// whenever {Starblade} would deal damage...
			AddTrigger(new ChangeDamageTypeTrigger(
				GameController,
				(DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.Card == this.CharacterCard,
				TypeChangeResponse,
				new TriggerType[1] { TriggerType.ChangeDamageType },
				null, // new DamageType[2] { DamageType.Melee, DamageType.Energy },
				GetCardSource()
			));

			// at the start of your turn...
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				(PhaseChangeAction pca) => GameController.GainHP(
					// ...each construct card regains 1 hp.
					DecisionMaker,
					(Card c) => c.IsConstruct,
					1,
					cardSource: GetCardSource()
				),
				TriggerType.GainHP
			);

			base.AddTriggers();
		}

		private IEnumerator TypeChangeResponse(DealDamageAction dda)
		{
			// ...you may change its type to melee or energy.
			if (GameController.PretendMode || dda.Target != _selectedTarget)
			{
				List<SelectDamageTypeDecision> selectDamageType = new List<SelectDamageTypeDecision>();
				IEnumerator selectCR = GameController.SelectDamageType(
					DecisionMaker,
					selectDamageType,
					new DamageType[2] { DamageType.Melee, DamageType.Energy },
					dda,
					SelectionType.DamageType,
					GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(selectCR);
				}
				else
				{
					GameController.ExhaustCoroutine(selectCR);
				}

				if (selectDamageType.Any((SelectDamageTypeDecision d) => d.Completed && d.SelectedDamageType.HasValue))
				{
					_selectedDamageType = selectDamageType.FirstOrDefault().SelectedDamageType.Value;
				}
				_selectedTarget = dda.Target;
			}

			if (_selectedDamageType.HasValue)
			{
				IEnumerator changeTypeCR = GameController.ChangeDamageType(
					dda,
					_selectedDamageType.Value,
					GetCardSource()
				);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(changeTypeCR);
				}
				else
				{
					GameController.ExhaustCoroutine(changeTypeCR);
				}
			}

			if (!GameController.PretendMode)
			{
				_selectedDamageType = null;
			}

			yield break;
		}


		public override IEnumerator UsePower(int index = 0)
		{
			// move all postura cards from your trash into your hand.
			IEnumerator moveCardsCR = GameController.MoveCards(
				DecisionMaker,
				new LinqCardCriteria(
					(Card c) => c.DoKeywordsContain("postura") && c.IsInTrash,
					"postura cards in trash"
				),
				(Card c) => this.HeroTurnTaker.Hand,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(moveCardsCR);
			}
			else
			{
				GameController.ExhaustCoroutine(moveCardsCR);
			}

			// you may play a postura card.
			IEnumerator playPosturaCR = SelectAndPlayCardFromHand(
				this.HeroTurnTakerController,
				cardCriteria: new LinqCardCriteria(
					(Card c) => c.DoKeywordsContain("postura"),
					"postura"
				)
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playPosturaCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playPosturaCR);
			}

			yield break;
		}
	}
}