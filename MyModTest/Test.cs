﻿using NUnit.Framework;
using System;
using Workshopping;
using Workshopping.MigrantCoder;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Linq;
using System.Collections;
using Handelabra.Sentinels.UnitTest;
using Workshopping.TheBaddies;
using System.Collections.Generic;

namespace MyModTest
{
    [TestFixture()]
    public class Test : BaseTest
    {
        protected TurnTakerController baddies { get { return FindVillain("TheBaddies"); } }
        protected TurnTakerController hug { get { return FindVillain("TheHugMonsterTeam"); } }
        protected HeroTurnTakerController migrant { get { return FindHero("MigrantCoder"); } }

        [Test()]
        public void TestModWorks()
        {
            SetupGameController("Workshopping.TheBaddies", "Workshopping.MigrantCoder", "Workshopping.DevStream");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(baddies);
            Assert.IsInstanceOf(typeof(TheBaddiesTurnTakerController), baddies);
            Assert.IsInstanceOf(typeof(TheBaddiesCharacterCardController), baddies.CharacterCardController);

            Assert.IsNotNull(migrant);
            Assert.IsInstanceOf(typeof(MigrantCoderTurnTakerController), migrant);
            Assert.IsInstanceOf(typeof(MigrantCoderCharacterCardController), migrant.CharacterCardController);

            Assert.IsNotNull(env);

            Assert.AreEqual(40, baddies.CharacterCard.HitPoints);
            Assert.AreEqual(30, migrant.CharacterCard.HitPoints);
            QuickHPStorage(baddies, migrant);

            StartGame();

            AssertIsInPlay("SmashBackField");

            // Always deals 5 psychic to non villains!
            PlayCard("FireEverything");

            QuickHPCheck(0, -6); // Nemesis!

            PlayTopCard(env);

            // Deals 1 damage
            QuickHPCheck(-1, -1);

            // Heals 1 at the start of the environment turn
            GoToStartOfTurn(env);
            QuickHPCheck(1, 1);
        }

        [Test()]
        public void TestPunchingBag()
        {
            SetupGameController("BaronBlade", "Workshopping.MigrantCoder", "Megalopolis");

            StartGame();

            GoToPlayCardPhase(migrant);

            // Punching Bag does 1 damage! But don't play a card.
            DecisionYesNo = true;
            DecisionDoNotSelectCard = SelectionType.PlayCard;
            QuickHPStorage(migrant);
            PlayCard("PunchingBag");
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestInnatePower()
        {
            SetupGameController("BaronBlade", "Workshopping.MigrantCoder", "Megalopolis");

            StartGame();

            var mdp = GetCardInPlay("MobileDefensePlatform");

            // Base power draws 1 card! Deals 1 target 2 damage!
            QuickHandStorage(migrant.ToHero());
            DecisionSelectTarget = mdp;
            QuickHPStorage(mdp);

            UsePower(migrant.CharacterCard);

            QuickHandCheck(1);
            QuickHPCheck(-2);

        }

        [Test()]
        public void TestHugMonster()
        {
            SetupGameController("Workshopping.TheHugMonsterTeam", "Workshopping.MigrantCoder", "BugbearTeam", "Legacy", "GreazerTeam", "TheWraith", "Megalopolis");

            StartGame();

            var warm = PlayTopCard(hug);

            QuickHPStorage(hug, migrant, bugbearTeam, legacy, greazerTeam, wraith);
            GoToEndOfTurn(hug);
            QuickHPCheck(0, -2, 0, -2, 0, -4);

            GoToPlayCardPhase(migrant);
            DestroyCard(warm);
            GoToEndOfTurn(migrant);
            QuickHPCheckZero();

        }

        [Test()]
        public void TestMigrantCoderLockdown()
        {
            SetupGameController("BaronBlade", "Workshopping.MigrantCoder/MigrantCoderLockdown", "Megalopolis");

            StartGame();

            Assert.IsInstanceOf(typeof(MigrantCoderLockdownCharacterCardController), migrant.CharacterCardController);

            GoToUsePowerPhase(migrant);

            // Use power to reduce damage by 1
            QuickHPStorage(migrant);
            UsePower(migrant);
            PlayCard("PunchingBag");
            QuickHPCheck(0);

            SaveAndLoad();

            Assert.IsInstanceOf(typeof(MigrantCoderLockdownCharacterCardController), migrant.CharacterCardController);
        }

        [Test()]
        public void TestBunkerVariant()
        {
            SetupGameController("BaronBlade", "Bunker/Workshopping.WaywardBunkerCharacter", "Megalopolis");

            StartGame();

            Assert.IsTrue(bunker.CharacterCard.IsPromoCard);
            Assert.AreEqual("WaywardBunkerCharacter", bunker.CharacterCard.PromoIdentifierOrIdentifier);
            Assert.AreEqual(30, bunker.CharacterCard.MaximumHitPoints);

            GoToUsePowerPhase(bunker);

            // Use the power, it draws 2 cards not 1!
            QuickHandStorage(bunker);
            UsePower(bunker);
            QuickHandCheck(2);
        }

        [Test()]
        public void TestTheSentinelsVariant()
        {
            var promos = new Dictionary<string, string>();

            promos.Add("TheSentinelsInstructions", "Workshopping.TheSerpentinelsInstructions");
            promos.Add("DrMedicoCharacter", "Workshopping.DrMedicobraCharacter");
            promos.Add("WritheCharacter", "Workshopping.MainsnakeCharacter");
            promos.Add("MainstayCharacter", "Workshopping.TheIdealizardCharacter");
            promos.Add("TheIdealistCharacter", "Workshopping.TheIdealizardCharacter");
            SetupGameController(new string[] { "BaronBlade", "TheSentinels", "Megalopolis" }, false, promos);

            StartGame();

            var instructions = GetCard("TheSentinelsInstructions");
            Assert.IsTrue(instructions.IsPromoCard);

            var medico = GetCard("DrMedicoCharacter");
            Assert.AreEqual("DrMedicobraCharacter", medico.PromoIdentifierOrIdentifier);
            Assert.AreEqual(15, medico.MaximumHitPoints);

            GoToUsePowerPhase(sentinels);

            // Use the power on Mainstay for 3 damage
            var mainstay = GetCard("MainstayCharacter");

            DecisionSelectTarget = GetMobileDefensePlatform().Card;
            QuickHPStorage(DecisionSelectTarget);
            UsePower(mainstay);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestTheRealBaddies()
        {
            SetupGameController("Workshopping.TheBaddies/Workshopping.TheRealBaddiesCharacter", "Workshopping.MigrantCoder", "Workshopping.DevStream");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(baddies);
            Assert.IsInstanceOf(typeof(TheBaddiesTurnTakerController), baddies);
            Assert.IsInstanceOf(typeof(TheRealBaddiesCharacterCardController), baddies.CharacterCardController);

            Assert.AreEqual(50, baddies.CharacterCard.HitPoints);
            QuickHPStorage(baddies, migrant);

            StartGame();

            AssertNotInPlay("SmashBackField");
            AssertIsInPlay("WrightWeigh");

            GoToEndOfTurn();
        }

        [Test()]
        public void TestSkyScraperVariant()
        {
            var promos = new Dictionary<string, string>();

            promos.Add("SkyScraper", "Workshopping.CentristSkyScraperNormalCharacter");
            SetupGameController(new string[] { "BaronBlade", "SkyScraper", "Megalopolis" }, false, promos);

            StartGame();

            Assert.IsNotNull(sky);
            Assert.IsInstanceOf(typeof(Workshopping.SkyScraper.CentristSkyScraperNormalCharacterCardController), sky.CharacterCardController);
            Assert.AreEqual(30, sky.CharacterCard.HitPoints);

            // Huge and tiny should be off to the side.
            Assert.AreEqual(2, sky.TurnTaker.OffToTheSide.NumberOfCards);
            var tiny = GetCard("SkyScraperTinyCharacter");
            var tinyCC = GetCardController(tiny);
            AssertOffToTheSide(tiny);
            Assert.IsInstanceOf(typeof(Workshopping.SkyScraper.CentristSkyScraperTinyCharacterCardController), tinyCC);

            var huge = GetCard("SkyScraperHugeCharacter");
            var hugeCC = GetCardController(huge);
            AssertOffToTheSide(huge);
            Assert.IsInstanceOf(typeof(Workshopping.SkyScraper.CentristSkyScraperHugeCharacterCardController), hugeCC);

            GoToPlayCardPhase(sky);

            // Normal power draws 3 and goes huge.
            QuickHandStorage(sky);
            UsePower(sky);
            QuickHandCheck(3);
            Assert.IsInstanceOf(typeof(Workshopping.SkyScraper.CentristSkyScraperHugeCharacterCardController), sky.CharacterCardController);

            // Go huge!
            QuickHPStorage(GetMobileDefensePlatform().Card, sky.CharacterCard);
            UsePower(sky);
            QuickHPCheck(-1, -1);
        }

        [Test()]
        public void TestSkyScraperVariant_RepresentativeOfEarth()
        {
            var promos = new Dictionary<string, string>();

            SetupGameController(new string[] { "BaronBlade", "Legacy", "TheCelestialTribunal" }, false, promos);

            StartGame();

            SelectFromBoxForNextDecision("Workshopping.CentristSkyScraperHugeCharacter", "SkyScraper");
            var earth = PlayCard("RepresentativeOfEarth");

            var rep = GetCard("SkyScraperHugeCharacter");
            var repCC = FindCardController(rep);
            AssertIsInPlay(rep);
            AssertNextToCard(rep, earth);
            AssertMaximumHitPoints(rep, 10);
            Assert.IsTrue(rep.IsHeroCharacterCard);
            Assert.IsTrue(rep.IsTarget && rep.IsHero);
            Assert.IsFalse(rep.IsEnvironment);
            Assert.IsInstanceOf(typeof(Workshopping.SkyScraper.CentristSkyScraperHugeCharacterCardController), repCC);

            // Make sure the other sizes loaded too, off to the side
            AssertNumberOfCardsAtLocation(env.TurnTaker.OffToTheSide, 2);
            var tiny = GetCard("SkyScraperTinyCharacter");
            var tinyCC = FindCardController(tiny);
            Assert.IsInstanceOf(typeof(Workshopping.SkyScraper.CentristSkyScraperTinyCharacterCardController), tinyCC);
            var normal = GetCard("SkyScraperNormalCharacter");
            var normalCC = FindCardController(normal);
            Assert.IsInstanceOf(typeof(Workshopping.SkyScraper.CentristSkyScraperNormalCharacterCardController), normalCC);
        }

        [Test]
        public void TestSkyScraperVariant_CompletionistGuiseCharacter()
        {
            var promos = new Dictionary<string, string>();
            promos["Guise"] = "CompletionistGuiseCharacter";
            SetupGameController(new string[] { "BaronBlade", "SkyScraper", "Guise", "Legacy", "TheWraith", "Unity", "Megalopolis" }, false, promos);
            StartGame();
            RemoveVillainCards();

            var mono = PlayCard("ThorathianMonolith");
            DestroyCard(mono);

            // Examine the state of each of the size cards
            // Old cards should be owned by Guise now
            // New cards should be owned by SkyScraper
            var skyHuge = sky.CharacterCard;
            var skyNormal = GetCard("SkyScraperNormalCharacter");
            var skyTiny = GetCard("SkyScraperTinyCharacter");
            SelectCardsForNextDecision(sky.CharacterCard, guise.CharacterCard, legacy.CharacterCard, wraith.CharacterCard, unity.CharacterCard);
            SelectFromBoxForNextDecision("Workshopping.CentristSkyScraperHugeCharacter", "SkyScraper");
            UsePower(guise);
            var variantHuge = sky.CharacterCard;
            var variantNormal = sky.TurnTaker.OffToTheSide.Cards.Where(c => c.Identifier == "SkyScraperNormalCharacter").FirstOrDefault();
            var variantTiny = sky.TurnTaker.OffToTheSide.Cards.Where(c => c.Identifier == "SkyScraperTinyCharacter").FirstOrDefault();
            AssertAtLocation(skyHuge, guise.CharacterCard.UnderLocation);
            AssertAtLocation(skyNormal, guise.TurnTaker.OffToTheSide);
            AssertAtLocation(skyTiny, guise.TurnTaker.OffToTheSide);
            Assert.AreEqual("Workshopping.CentristSkyScraperHugeCharacter", variantHuge.QualifiedPromoIdentifierOrIdentifier);
            Assert.AreEqual("Workshopping.CentristSkyScraperNormalCharacter", variantNormal.QualifiedPromoIdentifierOrIdentifier);
            Assert.AreEqual("Workshopping.CentristSkyScraperTinyCharacter", variantTiny.QualifiedPromoIdentifierOrIdentifier);

            // The new SkyScraper should be the same size and change sizes normally
            Assert.AreEqual("Workshopping.CentristSkyScraperHugeCharacter", sky.CharacterCard.QualifiedPromoIdentifierOrIdentifier);
            PlayCard("UndetectableRelinking");
            Assert.AreEqual(sky.CharacterCard, variantTiny);
            PlayCard("ThorathianMonolith");
            Assert.AreEqual(sky.CharacterCard, variantHuge);

            // If Guise replaces her again, it should switch back to the old variant (without changing size)
            var prop = PlayCard("Proportionist");
            SelectCardsForNextDecision(sky.CharacterCard, guise.CharacterCard, legacy.CharacterCard, wraith.CharacterCard, unity.CharacterCard, baron.CharacterCard);
            SelectFromBoxForNextDecision("SkyScraperHugeCharacter", "SkyScraper");
            QuickHandStorage(sky);
            UsePower(guise);
            QuickHandCheckZero(); // Proportionist should not trigger
            AssertAtLocation(skyHuge, sky.TurnTaker.PlayArea);
            AssertAtLocation(variantHuge, guise.CharacterCard.UnderLocation);
            Assert.AreEqual(skyHuge, sky.CharacterCard); // The old variant should have been switched back in

            // SkyScraper should be able to switch sizes normally
            ResetDecisions();
            PlayCard("UndetectableRelinking");
            Assert.AreEqual("SkyScraperTinyCharacter", sky.CharacterCard.PromoIdentifierOrIdentifier);
        }

        [Test]
        public void TestSkyScraperVariant_CompletionistGuiseCharacter_Extremist()
        {
            var promos = new Dictionary<string, string>();
            promos["Guise"] = "CompletionistGuiseCharacter";
            SetupGameController(new string[] { "BaronBlade", "SkyScraper", "Guise", "Legacy", "TheWraith", "Unity", "Megalopolis" }, false, promos);
            StartGame();
            RemoveVillainCards();

            var mono = PlayCard("ThorathianMonolith");
            DestroyCard(mono);

            // Examine the state of each of the size cards
            // Old cards should be owned by Guise now
            // New cards should be owned by SkyScraper
            var skyHuge = sky.CharacterCard;
            var skyNormal = GetCard("SkyScraperNormalCharacter");
            var skyTiny = GetCard("SkyScraperTinyCharacter");
            SelectCardsForNextDecision(sky.CharacterCard, guise.CharacterCard, legacy.CharacterCard, wraith.CharacterCard, unity.CharacterCard);
            SelectFromBoxForNextDecision("Workshopping.CentristSkyScraperHugeCharacter", "SkyScraper");
            UsePower(guise);
            var variantHuge = sky.CharacterCard;
            var variantNormal = sky.TurnTaker.OffToTheSide.Cards.Where(c => c.Identifier == "SkyScraperNormalCharacter").FirstOrDefault();
            var variantTiny = sky.TurnTaker.OffToTheSide.Cards.Where(c => c.Identifier == "SkyScraperTinyCharacter").FirstOrDefault();
            AssertAtLocation(skyHuge, guise.CharacterCard.UnderLocation);
            AssertAtLocation(skyNormal, guise.TurnTaker.OffToTheSide);
            AssertAtLocation(skyTiny, guise.TurnTaker.OffToTheSide);
            Assert.AreEqual("Workshopping.CentristSkyScraperHugeCharacter", variantHuge.QualifiedPromoIdentifierOrIdentifier);
            Assert.AreEqual("Workshopping.CentristSkyScraperNormalCharacter", variantNormal.QualifiedPromoIdentifierOrIdentifier);
            Assert.AreEqual("Workshopping.CentristSkyScraperTinyCharacter", variantTiny.QualifiedPromoIdentifierOrIdentifier);

            // Now switch to Extremist. Guise should have both regular and centrist huge under him.
            SelectCardsForNextDecision(sky.CharacterCard, guise.CharacterCard, legacy.CharacterCard, wraith.CharacterCard, unity.CharacterCard, baron.CharacterCard);
            SelectFromBoxForNextDecision("ExtremistSkyScraperHugeCharacter", "SkyScraper");
            UsePower(guise);
            AssertAtLocation(skyHuge, guise.CharacterCard.UnderLocation);
            AssertAtLocation(variantHuge, guise.CharacterCard.UnderLocation);
            var extremistHuge = sky.CharacterCard;
            Assert.AreEqual("ExtremistSkyScraperHugeCharacter", extremistHuge.QualifiedPromoIdentifierOrIdentifier);
        }

        [Test]
        public void TestSkyScraperVariant_CompletionistGuiseCharacter_Extremist2()
        {
            var promos = new Dictionary<string, string>();
            promos["Guise"] = "CompletionistGuiseCharacter";
            SetupGameController(new string[] { "BaronBlade", "SkyScraper", "Guise", "Legacy", "TheWraith", "Unity", "Megalopolis" }, false, promos);
            StartGame();
            RemoveVillainCards();

            // Switch out normal sky-scraper.
            // Examine the state of each of the size cards
            // Old cards should be owned by Guise now
            // New cards should be owned by SkyScraper
            var skyNormal = sky.CharacterCard;
            var skyHuge = GetCard("SkyScraperHugeCharacter");
            var skyTiny = GetCard("SkyScraperTinyCharacter");
            SelectCardsForNextDecision(sky.CharacterCard, guise.CharacterCard, legacy.CharacterCard, wraith.CharacterCard, unity.CharacterCard);
            SelectFromBoxForNextDecision("Workshopping.CentristSkyScraperNormalCharacter", "SkyScraper");
            UsePower(guise);
            var variantNormal = sky.CharacterCard;
            var variantHuge = sky.TurnTaker.OffToTheSide.Cards.Where(c => c.Identifier == "SkyScraperHugeCharacter").FirstOrDefault();
            var variantTiny = sky.TurnTaker.OffToTheSide.Cards.Where(c => c.Identifier == "SkyScraperTinyCharacter").FirstOrDefault();
            AssertAtLocation(skyHuge, guise.TurnTaker.OffToTheSide);
            AssertAtLocation(skyNormal, guise.CharacterCard.UnderLocation);
            AssertAtLocation(skyTiny, guise.TurnTaker.OffToTheSide);
            Assert.AreEqual("Workshopping.CentristSkyScraperHugeCharacter", variantHuge.QualifiedPromoIdentifierOrIdentifier);
            Assert.AreEqual("Workshopping.CentristSkyScraperNormalCharacter", variantNormal.QualifiedPromoIdentifierOrIdentifier);
            Assert.AreEqual("Workshopping.CentristSkyScraperTinyCharacter", variantTiny.QualifiedPromoIdentifierOrIdentifier);

            // Switch to variant huge.
            PlayCard("ThorathianMonolith");

            // Now switch to Extremist. Guise should have regular normal size and centrist huge size under him now.
            SelectCardsForNextDecision(sky.CharacterCard, guise.CharacterCard, legacy.CharacterCard, wraith.CharacterCard, unity.CharacterCard, baron.CharacterCard);
            SelectFromBoxForNextDecision("ExtremistSkyScraperHugeCharacter", "SkyScraper");
            UsePower(guise);
            AssertAtLocation(skyNormal, guise.CharacterCard.UnderLocation);
            AssertAtLocation(variantHuge, guise.CharacterCard.UnderLocation);
            var extremistHuge = sky.CharacterCard;
            var extremistNormal = sky.TurnTaker.OffToTheSide.Cards.Where(c => c.Identifier == "SkyScraperNormalCharacter").FirstOrDefault();
            var extremistTiny = sky.TurnTaker.OffToTheSide.Cards.Where(c => c.Identifier == "SkyScraperTinyCharacter").FirstOrDefault();
            Assert.AreEqual("ExtremistSkyScraperHugeCharacter", extremistHuge.QualifiedPromoIdentifierOrIdentifier);
            Assert.AreEqual("ExtremistSkyScraperNormalCharacter", extremistNormal.QualifiedPromoIdentifierOrIdentifier);
            Assert.AreEqual("ExtremistSkyScraperTinyCharacter", extremistTiny.QualifiedPromoIdentifierOrIdentifier);
        }
    }
}
