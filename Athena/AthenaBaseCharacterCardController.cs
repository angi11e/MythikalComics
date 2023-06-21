using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Athena
{
	public class AthenaBaseCharacterCardController : HeroCharacterCardController
	{
		protected override int? DamagedCutoutThreshold => null;
		private const string ManifestCutoutSuffix = "Manifest";
		private const string GlaukopisCutoutSuffix = "Glaukopis";
		private const string PallasCutoutSuffix = "Pallas";
		private const string ParthenosCutoutSuffix = "Parthenos";
		private const string PromachosCutoutSuffix = "Promachos";
		private const string TheonosisCutoutSuffix = "Theonosis";

		public AthenaBaseCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController) {
		}

		public override void AddStartOfGameTriggers()
		{
			AddTrigger(
				(GameAction ga) => TurnTakerController is AthenaTurnTakerController ttc && !ttc.ArePromosSetup,
				SetupPromos,
				TriggerType.Hidden,
				TriggerTiming.Before,
				priority: TriggerPriority.High
			);
		}

		public IEnumerator SetupPromos(GameAction ga)
		{
			if (TurnTakerController is AthenaTurnTakerController ttc && !ttc.ArePromosSetup)
			{
				ttc.SetupPromos(ttc.availablePromos);
				ttc.ArePromosSetup = true;
			}
			return DoNothing();
		}

		protected bool ManifestInPlay => HeroTurnTaker.GetCardsWhere(
			(Card c) => c.IsInPlayAndNotUnderCard && IsManifest(c)
		).Any();

		protected LinqCardCriteria IsManifestCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsManifest(c), "manifest", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}

			return result;
		}

		protected bool IsManifest(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && GameController.DoesCardContainKeyword(card, "manifest", evenIfUnderCard, evenIfFaceDown);
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

			if (ManifestInPlay)
			{
				changedInfo.HeroTurnSuffix = ManifestCutoutSuffix;
				changedInfo.VillainTurnSuffix = ManifestCutoutSuffix;

				if (FindCard(TheonosisCutoutSuffix).IsInPlayAndHasGameText)
				{
					effects.Identifier = TheonosisCutoutSuffix;
					list.Add(effects);
				}
				if (FindCard(GlaukopisCutoutSuffix).IsInPlayAndHasGameText)
				{
					effects.Identifier = GlaukopisCutoutSuffix;
					list.Add(effects);
				}
				if (FindCard(PallasCutoutSuffix).IsInPlayAndHasGameText)
				{
					effects.Identifier = PallasCutoutSuffix;
					list.Add(effects);
				}
				if (FindCard(ParthenosCutoutSuffix).IsInPlayAndHasGameText)
				{
					effects.Identifier = ParthenosCutoutSuffix;
					list.Add(effects);
				}
				if (FindCard(PromachosCutoutSuffix).IsInPlayAndHasGameText)
				{
					effects.Identifier = PromachosCutoutSuffix;
					list.Add(effects);
				}

				changedInfo.ExtraCutouts = list;
			}
			else
			{
				changedInfo.HeroTurnSuffix = "";
				changedInfo.VillainTurnSuffix = "";
				changedInfo.ExtraCutouts = list;
			}

			if (changedInfo.HeroTurnSuffix != currentInfo.HeroTurnSuffix)
			{
				return true;
			}
			if (changedInfo.HeroTurnSuffix == ManifestCutoutSuffix && changedInfo.ExtraCutouts != currentInfo.ExtraCutouts)
			{
				return true;
			}
			return false;
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
					&& IsManifest(mca.CardToMove, true, true)
				) || (
					action is PlayCardAction pca
					&& timing == ActionTiming.DidPerform
					&& IsManifest(pca.CardToPlay, true, true)
				)
			)
			{
				formFlag = GetChangedCutoutInfo(currentInfo, TurnTakerControllerWithoutReplacements, out changedInfo);
			}
			return baseFlag || formFlag;
		}
	}
}