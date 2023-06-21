using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.CaptainCain
{
	public class CaptainCainBaseCharacterCardController : HeroCharacterCardController
	{
		protected override int? DamagedCutoutThreshold => null;
		private const string FistCutoutSuffix = "Fist";
		private const string BloodCutoutSuffix = "Blood";
		private const string BothCutoutSuffix = "Both";

		public CaptainCainBaseCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		protected bool IsBloodActive => HeroTurnTaker.GetCardsWhere(
			(Card c) => c.IsInPlayAndNotUnderCard && IsBlood(c) && c.Owner == this.Card.Owner
		).Any();

		protected bool IsFistActive => HeroTurnTaker.GetCardsWhere(
			(Card c) => c.IsInPlayAndNotUnderCard && IsFist(c) && c.Owner == this.Card.Owner
		).Any();

		protected LinqCardCriteria IsBloodCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsBlood(c), "blood", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}

			return result;
		}

		protected bool IsBlood(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && GameController.DoesCardContainKeyword(card, "blood", evenIfUnderCard, evenIfFaceDown);
		}

		protected LinqCardCriteria IsFistCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsFist(c), "fist", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}

			return result;
		}

		protected bool IsFist(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && GameController.DoesCardContainKeyword(card, "fist", evenIfUnderCard, evenIfFaceDown);
		}

		protected bool GetChangedCutoutInfo(
			CutoutInfo currentInfo,
			TurnTakerController ttc,
			out CutoutInfo changedInfo
		)
		{
			changedInfo = currentInfo;
			List<CutoutInfo> list = new List<CutoutInfo>();
			CutoutInfo effects = default(CutoutInfo);
			effects.IsEffect = true;

			if (!IsBloodActive && !IsFistActive && currentInfo.HeroTurnSuffix != "")
			{
				changedInfo.HeroTurnSuffix = "";
				changedInfo.VillainTurnSuffix = "";
				effects.Identifier = "Effects";
				list.Add(effects);
				changedInfo.ExtraCutouts = list;
			}
			else if (!IsBloodActive && IsFistActive && currentInfo.HeroTurnSuffix != FistCutoutSuffix)
			{
				changedInfo.HeroTurnSuffix = FistCutoutSuffix;
				changedInfo.VillainTurnSuffix = FistCutoutSuffix;
				effects.Identifier = FistCutoutSuffix + "Effects";
				list.Add(effects);
				changedInfo.ExtraCutouts = list;
			}
			else if (IsBloodActive && !IsFistActive && currentInfo.HeroTurnSuffix != BloodCutoutSuffix)
			{
				changedInfo.HeroTurnSuffix = BloodCutoutSuffix;
				changedInfo.VillainTurnSuffix = BloodCutoutSuffix;
				effects.Identifier = BloodCutoutSuffix + "Effects";
				list.Add(effects);
				changedInfo.ExtraCutouts = list;
			}
			else if (IsBloodActive && IsFistActive && currentInfo.HeroTurnSuffix != BothCutoutSuffix)
			{
				changedInfo.HeroTurnSuffix = BothCutoutSuffix;
				changedInfo.VillainTurnSuffix = BothCutoutSuffix;
				effects.Identifier = BothCutoutSuffix + "Effects";
				list.Add(effects);
				changedInfo.ExtraCutouts = list;
			}

			return changedInfo.HeroTurnSuffix != currentInfo.HeroTurnSuffix;
		}

		public override bool ShouldChangeCutout(
			CutoutInfo currentInfo,
			GameAction action,
			ActionTiming timing,
			out CutoutInfo changedInfo,
			out CutoutAnimation animation
		)
		{
			bool baseFlag = base.ShouldChangeCutout(currentInfo, action, timing, out changedInfo, out animation);
			bool formFlag = false;
			animation = CutoutAnimation.Fade;

			if (
				action == null || (
					action is MoveCardAction mca
					&& timing == ActionTiming.DidPerform
					&& mca.CardToMove.DoKeywordsContain(new string[2] { "fist", "blood" }, true, true)
				) || (
					action is PlayCardAction pca
					&& timing == ActionTiming.DidPerform
					&& pca.CardToPlay.DoKeywordsContain(new string[2] { "fist", "blood" }, true, true)
				)
			)
			{
				formFlag = GetChangedCutoutInfo(currentInfo, TurnTakerControllerWithoutReplacements, out changedInfo);
			}
			return baseFlag || formFlag;
		}
	}
}