using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.CaptainCain
{
	public abstract class CaptainCainEquipmentCardController : CaptainCainBaseCardController
	{
		protected CaptainCainEquipmentCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
		{
		}

		protected abstract IEnumerator FistPower();
		protected abstract IEnumerator BloodPower();

		public override IEnumerator UsePower(int index = 0)
		{
			IEnumerator powerCR = null;
			switch (index)
			{
				case 0:
					if (IsFistActive)
					{
						powerCR = FistPower();
					}
					else
					{
						powerCR = GameController.SendMessageAction(
							"{Fist} effects are not active, so this power does nothing",
							Priority.Medium,
							GetCardSource(),
							showCardSource: true
						);
					}
					break;

				case 1:
					if (IsBloodActive)
					{
						powerCR = BloodPower();
					}
					else
					{
						powerCR = GameController.SendMessageAction(
							"{Blood} effects are not active, so this power does nothing",
							Priority.Medium,
							GetCardSource(),
							showCardSource: true
						);
					}
					break;
			}

			if (powerCR != null)
			{
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(powerCR);
				}
				else
				{
					GameController.ExhaustCoroutine(powerCR);
				}
			}

			yield break;
		}

		/* ok, maybe this stuff isn't necessary
		protected abstract string FistText { get; }
		protected abstract string BloodText { get; }

		public override IEnumerable<Power> AskIfContributesPowersToCardController(CardController cc)
		{
			if (cc != this)
			{
				return null;
			}

			List<Power> powers = new List<Power>();
			if (IsFistActive)
			{
				powers.Add(new Power(
					cc.HeroTurnTakerController,
					cc,
					FistText,
					FistPower(),
					0,
					null,
					GetCardSource()
				));
			}
			if (IsBloodActive)
			{
				powers.Add(new Power(
					cc.HeroTurnTakerController,
					cc,
					BloodText,
					BloodPower(),
					1,
					null,
					GetCardSource()
				));
			}
			return powers;
		}
		*/
	}
}