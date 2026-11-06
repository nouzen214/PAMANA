using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TopDownGame.Editor
{
    public static class BattleSceneGenerator
    {
        [MenuItem("Tools/Setup/Generate All Battle Scenes (Rooms 1 to 12) and Redirect Alterworld")]
        public static void GenerateAndRedirectAll()
        {
            GenerateBattleScene(); // Room 1
            GenerateBattleScenesForRooms2To6();
            GenerateBattleScenesForRooms7To11();
            GenerateBattleSceneForRoom12();
            RedirectAlterworldRooms();
            UpdateBuildSettings();
            Debug.Log("[BattleSceneGenerator] Successfully generated all 12 Alter World Battle Scenes and updated hallway doors!");
        }

        [MenuItem("Tools/Setup/Generate Battle Scenes for Rooms 2 to 6")]
        public static void GenerateBattleScenesForRooms2To6()
        {
            // Room 2: Duwende
            CreateBattleSceneFromTemplate(
                roomNumber: 2,
                roomTitle: "Room 2 (Alter Realm)",
                monsterName: "Duwende",
                monsterSpritePath: "Assets/Sprites/Enemy_Room2.png",
                monsterAvatarPath: "Assets/UI/Avatar_Enemy_Room2.png",
                slotData: new List<ArtifactBattleSlot>
                {
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room2_GalleyProofSheets",
                        artifactName = "Galley Proof Sheets",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_GalleyProofSheets.png"),
                        artifactDescription = "These proof sheets represent the challenges Rizal faced in publishing his first novel and the support he received from fellow Filipinos who believed in his work.",
                        questionText = "Who was the generous friend who provided Rizal the financial funds needed to print the first edition of Noli Me Tangere in Berlin?",
                        choices = new string[] {
                            "Maximo Viola",
                            "Valentin Ventura",
                            "Ferdinand Blumentritt",
                            "Juan Luna"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room2_FeatherQuillPen",
                        artifactName = "Feather Quill Pen",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_FeatherQuillPen.png"),
                        artifactDescription = "This quill pen symbolizes Rizal's use of literature to expose the suffering of Filipinos under Spanish colonial rule through the characters in his novels.",
                        questionText = "Which character in Noli Me Tangere symbolized the tragic plight of an oppressed Filipino mother driven to insanity?",
                        choices = new string[] {
                            "Sisa",
                            "Maria Clara",
                            "Doña Victorina",
                            "Tiya Isabel"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room2_PrintingPressMatrix",
                        artifactName = "Printing Press Matrix",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_PrintingPressMatrix.png"),
                        artifactDescription = "This printing matrix represents the process of producing Rizal's second novel, which continued his criticism of colonial abuses and social injustice.",
                        questionText = "In which European city was Jose Rizal's second novel, El Filibusterismo, published in September 1891?",
                        choices = new string[] {
                            "Ghent, Belgium",
                            "Berlin, Germany",
                            "Madrid, Spain",
                            "Paris, France"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room2_FirstEditionNovelCopy",
                        artifactName = "First Edition Novel Copy",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_FirstEditionNovelCopy.png"),
                        artifactDescription = "This first-edition copy symbolizes the determination and financial support that helped bring Rizal's second novel to publication despite difficulties.",
                        questionText = "Who provided the crucial financial loan that rescued the printing of El Filibusterismo from being suspended?",
                        choices = new string[] {
                            "Valentin Ventura",
                            "Maximo Viola",
                            "Graciano Lopez Jaena",
                            "Marcelo del Pilar"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room2_AlcoholCookingStove",
                        artifactName = "Alcohol Cooking Stove",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_AlcoholCookingStove.png"),
                        artifactDescription = "This stove represents the final days of Rizal's life and the farewell poem he left behind as an expression of his love for the Philippines.",
                        questionText = "Inside which everyday household object did Rizal hide the folded manuscript of his farewell poem, Mi Último Adiós?",
                        choices = new string[] {
                            "Alcohol cooking stove",
                            "Leather boot heel",
                            "Hollow walking cane",
                            "Bible book spine"
                        },
                        correctChoiceIndex = 0
                    }
                },
                victoryMessage: "You defeated the Duwende and recovered all 5 artifacts in Room 2!\nTake the portal back and bring them inside Room 2 in the Real World to unlock Alterworld Room 3!",
                outputScenePath: "Assets/Scenes/BattleScene_2.unity"
            );

            // Room 3: Diwata
            CreateBattleSceneFromTemplate(
                roomNumber: 3,
                roomTitle: "Room 3 (Alter Realm)",
                monsterName: "Diwata",
                monsterSpritePath: "Assets/Sprites/Enemy_Room3.png",
                monsterAvatarPath: "Assets/UI/Avatar_Enemy_Room3.png",
                slotData: new List<ArtifactBattleSlot>
                {
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room3_DiariongTagalog",
                        artifactName = "Diariong Tagalog Broadsheet",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_DiariongTagalog.png"),
                        artifactDescription = "This broadsheet represents the role of early Filipino newspapers in spreading patriotic ideas and giving reformists a platform to express their views.",
                        questionText = "In which bilingual Manila newspaper was Jose Rizal's first patriotic essay, \"El Amor Patrio\" (Love of Country), published in 1882?",
                        choices = new string[] {
                            "Diariong Tagalog",
                            "La Opinion",
                            "El Comercio",
                            "El Pasig"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room3_LaSolidaridad",
                        artifactName = "La Solidaridad Newspaper Issue",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_LaSolidaridad.png"),
                        artifactDescription = "This newspaper symbolizes the reform movement that sought political and social changes in the Philippines during Spanish colonial rule.",
                        questionText = "What was the primary political goal promoted by the reformist propaganda newspaper La Solidaridad?",
                        choices = new string[] {
                            "Assimilation with Spain",
                            "Return to isolationism",
                            "British colonial annexation",
                            "Immediate violent revolution"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room3_MalolosLetter",
                        artifactName = "Malolos Letter Leaflet",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_LetterToWomenOfMalolos.png"),
                        artifactDescription = "This leaflet represents Rizal's writings on education, moral values, and the important role of Filipino women in shaping the nation's future.",
                        questionText = "In his famous 1889 letter to the young women of Malolos, what civic virtue did Rizal urge Filipino mothers to instill in their children?",
                        choices = new string[] {
                            "Love for country",
                            "Complete political silence",
                            "Blind clerical obedience",
                            "Desire for wealth"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room3_CenturyHence",
                        artifactName = "Century Hence Serial Article",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_PhilippinesCenturyHence.png"),
                        artifactDescription = "This article represents Rizal's reflections on the Philippines' future and the possible effects of colonial rule on the nation's destiny.",
                        questionText = "In the serialized article \"Filipinas dentro de cien años,\" which foreign power did Rizal prophetically foresee intervening in the Philippines?",
                        choices = new string[] {
                            "United States",
                            "Great Britain",
                            "Tsarist Russia",
                            "Imperial Japan"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room3_GomburzaFolio",
                        artifactName = "GOMBURZA Memorial Leaflet",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_GomburzaFolio.png"),
                        artifactDescription = "This leaflet symbolizes the lasting impact of the execution of three Filipino priests on the growth of Filipino nationalism and the fight against colonial injustice.",
                        questionText = "Which tragic event in 1872 deeply awakened young Rizal's nationalist fervor and inspired his lifelong struggle against colonial abuse?",
                        choices = new string[] {
                            "Execution of GOMBURZA",
                            "Manila earthquake disaster",
                            "Opening of Suez",
                            "Cavite trade boycott"
                        },
                        correctChoiceIndex = 0
                    }
                },
                victoryMessage: "You overcame the Diwata's trial and recovered the artifacts in Room 3!\nTake the portal back and bring them inside Room 3 in the Real World to unlock Alterworld Room 4!",
                outputScenePath: "Assets/Scenes/BattleScene_3.unity"
            );

            // Room 4: Bakunawa
            CreateBattleSceneFromTemplate(
                roomNumber: 4,
                roomTitle: "Room 4 (Alter Realm)",
                monsterName: "Bakunawa",
                monsterSpritePath: "Assets/Sprites/Enemy_Room4.png",
                monsterAvatarPath: "Assets/UI/Avatar_Enemy_Room4.png",
                slotData: new List<ArtifactBattleSlot>
                {
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room4_MembershipOathCertificate",
                        artifactName = "Membership Oath Certificate",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_MembershipOathCertificate.png"),
                        artifactDescription = "This certificate represents La Liga Filipina's commitment to unity, mutual support, and the protection of its members against injustice.",
                        questionText = "What was one of the primary civic objectives outlined in the constitution of La Liga Filipina?",
                        choices = new string[] {
                            "Mutual protection of members",
                            "Violent armed rebellion",
                            "Total Spanish expulsion",
                            "Forced religious conversion"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room4_ArrestWarrantDecree",
                        artifactName = "Arrest Warrant Decree",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_ArrestWarrantDecree.png"),
                        artifactDescription = "This decree symbolizes the colonial authorities' efforts to suppress organizations and individuals who sought change under Spanish rule.",
                        questionText = "Which Spanish Governor-General ordered the sudden arrest and deportation of Jose Rizal shortly after founding La Liga?",
                        choices = new string[] {
                            "Eulogio Despujol",
                            "Camilo de Polavieja",
                            "Emilio Terrero",
                            "Ramon Blanco"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room4_SecretMemberLedger",
                        artifactName = "Secret Member Ledger",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_SecretMemberLedger.png"),
                        artifactDescription = "This ledger represents the secret networks formed by Filipino patriots who organized to pursue the country's freedom from Spanish colonial rule.",
                        questionText = "Which founding member of La Liga Filipina co-founded the revolutionary Katipunan just days after Rizal's arrest?",
                        choices = new string[] {
                            "Andres Bonifacio",
                            "Emilio Aguinaldo",
                            "Marcelo del Pilar",
                            "Antonio Luna"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room4_ReorganizedCouncilCharter",
                        artifactName = "Reorganized Council Charter",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_ReorganizedCouncilCharter.png"),
                        artifactDescription = "This charter represents the efforts to revive La Liga Filipina after Rizal's exile and continue its work toward civic reform and national unity.",
                        questionText = "Who served as the secretary of the Supreme Council when La Liga Filipina was briefly reorganized in 1893?",
                        choices = new string[] {
                            "Apolinario Mabini",
                            "Emilio Jacinto",
                            "Graciano Lopez Jaena",
                            "Mariano Ponce"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room4_SplitResolutionManifesto",
                        artifactName = "Split Resolution Manifesto",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_SplitResolutionManifesto.png"),
                        artifactDescription = "This manifesto symbolizes the division within the nationalist movement between those who continued supporting peaceful reform and those who pursued independence through revolution.",
                        questionText = "Following the dissolution of La Liga Filipina, which moderate group broke away to continue funding La Solidaridad?",
                        choices = new string[] {
                            "Cuerpo de Compromisarios",
                            "Circulo Hispano-Filipino",
                            "Katipunan",
                            "Los Indios Bravos"
                        },
                        correctChoiceIndex = 0
                    }
                },
                victoryMessage: "You defeated the mighty Bakunawa and recovered the artifacts in Room 4!\nTake the portal back and bring them inside Room 4 in the Real World to unlock Alterworld Room 5!",
                outputScenePath: "Assets/Scenes/BattleScene_4.unity"
            );

            // Room 5: Kapre
            CreateBattleSceneFromTemplate(
                roomNumber: 5,
                roomTitle: "Room 5 (Alter Realm)",
                monsterName: "Kapre",
                monsterSpritePath: "Assets/Sprites/Enemy_Room5.png",
                monsterAvatarPath: "Assets/UI/Avatar_Enemy_Room5.png",
                slotData: new List<ArtifactBattleSlot>
                {
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room5_SurgicalEyeForceps",
                        artifactName = "Surgical Eye Forceps",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_SurgicalEyeForceps.png"),
                        artifactDescription = "These forceps represent Rizal's work as an ophthalmologist during his exile in Dapitan, where he continued providing medical care to patients.",
                        questionText = "Whom did Rizal successfully operate on for cataract removal during his exile in Dapitan?",
                        choices = new string[] {
                            "Teodora Alonso Realonda",
                            "Josephine Bracken",
                            "Father Obach",
                            "Governor Despujol"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room5_SchoolRollBook",
                        artifactName = "School Roll Book",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_SchoolRollBook.png"),
                        artifactDescription = "This roll book represents the small community school Rizal established in Talisay, where he educated boys and taught practical skills alongside academic subjects.",
                        questionText = "How many young male students did Rizal personally tutor tuition-free in his community school in Talisay?",
                        choices = new string[] {
                            "Around sixteen students",
                            "Over one hundred",
                            "Only five boys",
                            "Fifty foreign scholars"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room5_SpecimenJarWithBeetle",
                        artifactName = "Specimen Jar with Beetle",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_SpecimenJarWithBeetle.png"),
                        artifactDescription = "This specimen jar represents Rizal's interest in natural history and his collection and study of insects during his stay in Dapitan.",
                        questionText = "Which horned beetle species discovered by Rizal in Dapitan was scientifically named in his honor?",
                        choices = new string[] {
                            "Apogonia rizali",
                            "Rhacophorus rizali",
                            "Scolopendra rizali",
                            "Draco rizali"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room5_CoconutOilLamp",
                        artifactName = "Coconut Oil Lamp",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_CoconutOilLamp.png"),
                        artifactDescription = "This lamp represents Rizal's community improvement projects in Dapitan, where he applied practical knowledge to help improve local living conditions.",
                        questionText = "What innovative municipal project did Rizal introduce to illuminate the dark public streets of Dapitan at night?",
                        choices = new string[] {
                            "Coconut oil streetlamps",
                            "Whale oil lanterns",
                            "Kerosene gas torches",
                            "Electric battery bulbs"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room5_MilitaryDoctorApplication",
                        artifactName = "Military Doctor Application",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_MilitaryDoctorApplication.png"),
                        artifactDescription = "This application represents Rizal's willingness to offer his medical expertise to help people suffering during a public health crisis overseas.",
                        questionText = "For which humanitarian mission abroad did Rizal request and receive permission to volunteer as a military surgeon?",
                        choices = new string[] {
                            "Army hospital in Cuba",
                            "Quarantine station in Japan",
                            "Field clinic in France",
                            "Red Cross in Spain"
                        },
                        correctChoiceIndex = 0
                    }
                },
                victoryMessage: "You defeated the Kapre and recovered the artifacts in Room 5!\nTake the portal back and bring them inside Room 5 in the Real World to unlock Alterworld Room 6!",
                outputScenePath: "Assets/Scenes/BattleScene_5.unity"
            );

            // Room 6: Manananggal
            CreateBattleSceneFromTemplate(
                roomNumber: 6,
                roomTitle: "Room 6 (Alter Realm)",
                monsterName: "Manananggal",
                monsterSpritePath: "Assets/Sprites/Enemy_Room6.png",
                monsterAvatarPath: "Assets/UI/Avatar_Enemy_Room6.png",
                slotData: new List<ArtifactBattleSlot>
                {
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room6_RoyalGovernorBaton",
                        artifactName = "Royal Governor Baton",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_RoyalGovernorBaton.png"),
                        artifactDescription = "This baton represents the authority held by the highest colonial official, who governed the Philippines in the name of the Spanish Crown.",
                        questionText = "Who served as the supreme executive leader and personal representative of the Spanish Monarch in the Philippine colony?",
                        choices = new string[] {
                            "Governor-General",
                            "Cabeza de Barangay",
                            "Gobernadorcillo",
                            "Alcalde Mayor"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room6_TribunalCourtGavel",
                        artifactName = "Tribunal Court Gavel",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_TribunalCourtGavel.png"),
                        artifactDescription = "This gavel symbolizes the colonial judicial institutions responsible for administering justice and advising the highest government authorities.",
                        questionText = "What was the highest judicial court and advisory council to the Governor-General established in colonial Manila?",
                        choices = new string[] {
                            "Real Audiencia",
                            "Casa de Contratacion",
                            "Cabildo secular",
                            "Tribunal de Cuentas"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room6_BandalaQuotaScale",
                        artifactName = "Bandala Quota Scale",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_BandalaQuotaScale.png"),
                        artifactDescription = "This scale represents the compulsory agricultural purchasing system that burdened Filipino farmers by requiring them to sell their produce to colonial authorities under government-set terms.",
                        questionText = "Under which compulsory system were Filipino farmers forced to sell their agricultural produce to the government at fixed low prices?",
                        choices = new string[] {
                            "Bandala system",
                            "Real situado",
                            "Tobacco monopoly",
                            "Galleon trade"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room6_ParishRegistryBook",
                        artifactName = "Parish Registry Book",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_ParishRegistryBook.png"),
                        artifactDescription = "This registry book represents the extensive influence of Spanish religious orders over local communities and colonial affairs in the Philippines.",
                        questionText = "What term was coined to describe the overwhelming political and social dominance of Spanish friars over colonial administration?",
                        choices = new string[] {
                            "Frailocracia",
                            "Teocracia Real",
                            "Feudalismo",
                            "Burocracia"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room6_SilverColumnarCoin",
                        artifactName = "Silver Columnar Coin",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_SilverColumnarCoin.png"),
                        artifactDescription = "This coin represents the financial support provided by the Spanish Crown's colonial treasury in Mexico to help fund the administration of the Philippines.",
                        questionText = "What annual royal financial grant was sent from the Viceroyalty of New Spain (Mexico) to subsidize the Philippine colony?",
                        choices = new string[] {
                            "Real Situado",
                            "Vinta tax",
                            "Diezmos prediales",
                            "Sanctorum fee"
                        },
                        correctChoiceIndex = 0
                    }
                },
                victoryMessage: "You defeated the Manananggal and recovered all artifacts in Room 6!\nTake the portal back and bring them inside Room 6 in the Real World to restore balance to the realm!",
                outputScenePath: "Assets/Scenes/BattleScene_6.unity"
            );

            RedirectAlterworldRooms();
            UpdateBuildSettings();
            Debug.Log("[BattleSceneGenerator] Generated Battle Scenes for Rooms 2 to 6 successfully!");
        }

        [MenuItem("Tools/Setup/Generate Battle Scenes for Rooms 7 to 11")]
        public static void GenerateBattleScenesForRooms7To11()
        {
            EnsureSpriteImport("Assets/Sprites/Enemy_Room7.png");
            EnsureSpriteImport("Assets/Sprites/Enemy_Room8.png");
            EnsureSpriteImport("Assets/Sprites/Enemy_Room9.png");
            EnsureSpriteImport("Assets/Sprites/Enemy_Room10.png");
            EnsureSpriteImport("Assets/Sprites/Enemy_Room11.png");

            EnsureSpriteImport("Assets/UI/Avatar_Enemy_Room7.png");
            EnsureSpriteImport("Assets/UI/Avatar_Enemy_Room8.png");
            EnsureSpriteImport("Assets/UI/Avatar_Enemy_Room9.png");
            EnsureSpriteImport("Assets/UI/Avatar_Enemy_Room10.png");
            EnsureSpriteImport("Assets/UI/Avatar_Enemy_Room11.png");

            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_LaSolidaridadInaugural.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_FrayBotodBooklet.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_EditorialOfficeBadge.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_SpoliariumGoldMedal.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_PropagandistFundReceipt.png");

            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_TornCedulaCertificate.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_RevolutionaryKatipunanBolo.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_BloodOathSigningKnife.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_ActaDeLaProclamacion.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_RemingtonRollingBlockRifle.png");

            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_WovenTnalakCloth.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_BoneTattooChisel.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_BaybayinBambooStrip.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_AncestralBululCarving.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_AgungBrassGong.png");

            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_BakunawaTerracottaFigurine.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_TikbalangHorseHoofTalisman.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_ClayTobaccoPipeOfGiant.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_MorningStarPendant.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_SarimanokBrassFigurine.png");

            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_ManunggulBurialJar.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_BaroquePaoayChurchModel.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_RiceTerracesHandSpade.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_LasPinasBambooPipe.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_MaitumAnthropomorphicPot.png");

            // Room 7: Tikbalang
            CreateBattleSceneFromTemplate(
                roomNumber: 7,
                roomTitle: "Room 7 (Alter Realm)",
                monsterName: "Tikbalang",
                monsterSpritePath: "Assets/Sprites/Enemy_Room7.png",
                monsterAvatarPath: "Assets/UI/Avatar_Enemy_Room7.png",
                slotData: new List<ArtifactBattleSlot>
                {
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room7_LaSolidaridad",
                        artifactName = "La Solidaridad Inaugural Issue",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_LaSolidaridadInaugural.png"),
                        artifactDescription = "This newspaper represents the efforts of Filipino reformists in Spain to spread their ideas and advocate for political and social changes in the Philippines.",
                        questionText = "Who served as the founding editor-in-chief of the reformist organ La Solidaridad when it was first published in Barcelona in 1889?",
                        choices = new string[] {
                            "Graciano Lopez Jaena",
                            "Marcelo del Pilar",
                            "Jose Rizal",
                            "Mariano Ponce"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room7_FrayBotod",
                        artifactName = "Fray Botod Satirical Booklet",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_FrayBotodBooklet.png"),
                        artifactDescription = "This booklet represents the use of satire and literature to criticize the abuses and misconduct of some Spanish friars during the colonial period.",
                        questionText = "Which biting satirical work written by Graciano López Jaena ruthlessly depicted the abusive greed and gluttony of a Spanish friar?",
                        choices = new string[] {
                            "Fray Botod",
                            "La Soberania Monacal",
                            "Dasalan at Tocohan",
                            "Sagot ng Espanya"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room7_EditorialBadge",
                        artifactName = "Editorial Office Press Badge",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_EditorialOfficeBadge.png"),
                        artifactDescription = "This badge represents the work of Filipino reformists who organized their publication activities abroad to bring attention to the concerns of their homeland.",
                        questionText = "To which Spanish city did Marcelo H. del Pilar move the headquarters and editorial management of La Solidaridad in late 1889?",
                        choices = new string[] {
                            "Madrid",
                            "Barcelona",
                            "Seville",
                            "Cadiz"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room7_SpoliariumMedal",
                        artifactName = "Spoliarium Gold Medal",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_SpoliariumGoldMedal.png"),
                        artifactDescription = "This medal symbolizes the achievements of Filipino artists abroad and the recognition of Filipino talent during the Spanish colonial period.",
                        questionText = "Which master Filipino painter won a prestigious first-class gold medal at the 1884 Madrid Fine Arts Exposition for \"Spoliarium\"?",
                        choices = new string[] {
                            "Juan Luna",
                            "Felix Resurreccion Hidalgo",
                            "Damian Domingo",
                            "Antonio Malantic"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room7_FundReceipt",
                        artifactName = "Propagandist Fund Receipt",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_PropagandistFundReceipt.png"),
                        artifactDescription = "This receipt represents the financial contributions of Filipino supporters who helped sustain reformist activities and publications in Spain.",
                        questionText = "Which Manila-based patriotic committee secretly collected funds from wealthy Filipinos to support propagandists in Spain?",
                        choices = new string[] {
                            "Junta de Propaganda",
                            "Katipunan",
                            "Real Audiencia",
                            "Guardias de Honor"
                        },
                        correctChoiceIndex = 0
                    }
                },
                victoryMessage: "You defeated the Tikbalang and recovered all artifacts in Room 7!\nTake the portal back and bring them inside Room 7 in the Real World to unlock Alterworld Room 8!",
                outputScenePath: "Assets/Scenes/BattleScene_7.unity"
            );

            // Room 8: Sigbin
            CreateBattleSceneFromTemplate(
                roomNumber: 8,
                roomTitle: "Room 8 (Alter Realm)",
                monsterName: "Sigbin",
                monsterSpritePath: "Assets/Sprites/Enemy_Room8.png",
                monsterAvatarPath: "Assets/UI/Avatar_Enemy_Room8.png",
                slotData: new List<ArtifactBattleSlot>
                {
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room8_TornCedula",
                        artifactName = "Torn Cédula Certificate",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_TornCedulaCertificate.png"),
                        artifactDescription = "This certificate represents the act of defiance by Filipino revolutionaries who rejected a symbol of Spanish colonial authority during the uprising of 1896.",
                        questionText = "What symbolic act of rebellion did Andres Bonifacio and the Katipuneros perform during the Cry of Pugad Lawin in August 1896?",
                        choices = new string[] {
                            "Tearing tax cédulas",
                            "Burning Spanish flags",
                            "Breaking iron swords",
                            "Sinking Spanish galleons"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room8_KatipunanBolo",
                        artifactName = "Revolutionary Katipunan Bolo",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_RevolutionaryKatipunanBolo.png"),
                        artifactDescription = "This bolo symbolizes the courage of Katipunan members who took up arms against Spanish colonial rule during the Philippine Revolution.",
                        questionText = "Where did the first major armed battle between Katipunan forces and the Spanish army take place on August 30, 1896?",
                        choices = new string[] {
                            "San Juan del Monte",
                            "Binakayan, Cavite",
                            "Caloocan",
                            "Pasig"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room8_SigningKnife",
                        artifactName = "Blood Oath Signing Knife",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_BloodOathSigningKnife.png"),
                        artifactDescription = "This knife represents the initiation practices of the Katipunan, through which members demonstrated their loyalty and commitment to the revolutionary cause.",
                        questionText = "What sacred initiation ritual did new recruits undergo to seal their absolute commitment to the Katipunan?",
                        choices = new string[] {
                            "Blood compact signing",
                            "Religious baptism",
                            "Golden ring exchange",
                            "Sword kneeling vow"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room8_ActaProclamacion",
                        artifactName = "Acta de la Proclamación",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_ActaDeLaProclamacion.png"),
                        artifactDescription = "This document represents the historic declaration of Philippine independence in 1898 after years of resistance against Spanish colonial rule.",
                        questionText = "From the window of whose ancestral home in Kawit, Cavite was Philippine independence formally proclaimed on June 12, 1898?",
                        choices = new string[] {
                            "Emilio Aguinaldo",
                            "Ambrosio Bautista",
                            "Baldomero Aguinaldo",
                            "Felipe Agoncillo"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room8_RollingBlockRifle",
                        artifactName = "Remington Rolling-Block Rifle",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_RemingtonRollingBlockRifle.png"),
                        artifactDescription = "This rifle symbolizes the sacrifices of Filipino soldiers who defended their homeland during the struggle against American forces following the Spanish colonial period.",
                        questionText = "Which courageous Filipino general heroically held off advancing American troops at the Battle of Tirad Pass in December 1899?",
                        choices = new string[] {
                            "Gregorio del Pilar",
                            "Antonio Luna",
                            "Miguel Malvar",
                            "Licerio Geronimo"
                        },
                        correctChoiceIndex = 0
                    }
                },
                victoryMessage: "You defeated the Sigbin and recovered all artifacts in Room 8!\nTake the portal back and bring them inside Room 8 in the Real World to unlock Alterworld Room 9!",
                outputScenePath: "Assets/Scenes/BattleScene_8.unity"
            );

            // Room 9: Santelmo
            CreateBattleSceneFromTemplate(
                roomNumber: 9,
                roomTitle: "Room 9 (Alter Realm)",
                monsterName: "Santelmo",
                monsterSpritePath: "Assets/Sprites/Enemy_Room9.png",
                monsterAvatarPath: "Assets/UI/Avatar_Enemy_Room9.png",
                slotData: new List<ArtifactBattleSlot>
                {
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room9_TnalakCloth",
                        artifactName = "Woven T'nalak Cloth",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_WovenTnalakCloth.png"),
                        artifactDescription = "This cloth represents the T'boli people's traditional weaving practices, in which abaca fibers are transformed into intricate patterns connected to cultural beliefs and dream-inspired designs.",
                        questionText = "Which indigenous community in South Cotabato is famous for weaving sacred abaca T'nalak textiles inspired by dreams?",
                        choices = new string[] {
                            "T'boli tribe",
                            "Ifugao people",
                            "Mangyan community",
                            "Badjao sea nomads"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room9_TattooChisel",
                        artifactName = "Bone Tattoo Chisel",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_BoneTattooChisel.png"),
                        artifactDescription = "This chisel represents the traditional tattooing practices of Visayan communities, whose decorated bodies led Spanish chroniclers to refer to them as the \"Pintados,\" or painted ones.",
                        questionText = "Why did early Spanish conquistadors bestow the moniker \"Pintados\" upon the native warriors of the Visayan islands?",
                        choices = new string[] {
                            "Painted body tattoos",
                            "Colorful woven garments",
                            "Bright face paint",
                            "Polished silver shields"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room9_BambooStrip",
                        artifactName = "Baybayin Bamboo Strip",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_BaybayinBambooStrip.png"),
                        artifactDescription = "This strip represents the indigenous writing traditions of the Philippines, including a syllabic script used by Tagalog communities before and during the early Spanish colonial period.",
                        questionText = "What ancient indigenous syllabic script was widely utilized by precolonial Tagalogs and Visayans prior to Spanish colonization?",
                        choices = new string[] {
                            "Baybayin script",
                            "Surat Mangyan",
                            "Kulitan script",
                            "Jawi script"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room9_BululCarving",
                        artifactName = "Ancestral Bulul Carving",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_AncestralBululCarving.png"),
                        artifactDescription = "This carving represents the Ifugao people's spiritual traditions and their use of wooden figures associated with protecting rice and ensuring a plentiful harvest.",
                        questionText = "What sacred carved wooden granary idol is traditionally venerated by the Ifugao people to protect the harvest?",
                        choices = new string[] {
                            "Bulul figure",
                            "Manunggul spirit",
                            "Likha idol",
                            "Diwata talisman"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room9_AgungGong",
                        artifactName = "Agung Brass Gong",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_AgungBrassGong.png"),
                        artifactDescription = "This gong represents the musical traditions of communities in the southern Philippines, where gongs and other instruments are played together in ensembles during cultural and social occasions.",
                        questionText = "What traditional gong chime musical ensemble featuring graduated bronze pots is indigenous to the southern Philippines?",
                        choices = new string[] {
                            "Kulintang ensemble",
                            "Rondalla orchestra",
                            "Angklung choir",
                            "Musikang bumbong"
                        },
                        correctChoiceIndex = 0
                    }
                },
                victoryMessage: "You defeated the Santelmo and recovered all artifacts in Room 9!\nTake the portal back and bring them inside Room 9 in the Real World to unlock Alterworld Room 10!",
                outputScenePath: "Assets/Scenes/BattleScene_9.unity"
            );

            // Room 10: Nuno
            CreateBattleSceneFromTemplate(
                roomNumber: 10,
                roomTitle: "Room 10 (Alter Realm)",
                monsterName: "Nuno",
                monsterSpritePath: "Assets/Sprites/Enemy_Room10.png",
                monsterAvatarPath: "Assets/UI/Avatar_Enemy_Room10.png",
                slotData: new List<ArtifactBattleSlot>
                {
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room10_BakunawaFigurine",
                        artifactName = "Bakunawa Terracotta Figurine",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_BakunawaTerracottaFigurine.png"),
                        artifactDescription = "This figurine represents the powerful serpent-like creature in Philippine mythology associated with eclipses and ancient explanations for changes in the appearance of the moon.",
                        questionText = "In Visayan mythology, which gigantic serpent dragon was believed to cause solar and lunar eclipses by swallowing celestial bodies?",
                        choices = new string[] {
                            "Bakunawa",
                            "Minokawa",
                            "Tambanokano",
                            "Kurita"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room10_TikbalangTalisman",
                        artifactName = "Tikbalang Horse-Hoof Talisman",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_TikbalangHorseHoofTalisman.png"),
                        artifactDescription = "This talisman represents a creature in Philippine folklore known for its unusual combination of human and horse-like features and its association with forests and mountain paths.",
                        questionText = "Which creature from Philippine folklore is described as a tall humanoid with the head, mane, and legs of a horse?",
                        choices = new string[] {
                            "Tikbalang",
                            "Kapre",
                            "Nuno sa punso",
                            "Pugot"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room10_GiantTobaccoPipe",
                        artifactName = "Clay Tobacco Pipe of Giant",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_ClayTobaccoPipeOfGiant.png"),
                        artifactDescription = "This pipe represents a giant in Philippine folklore believed to dwell among large trees and frighten people who wander into secluded places.",
                        questionText = "Which mythical tree-dwelling giant is famously envisioned sitting among broad branches smoking an oversized tobacco cigar?",
                        choices = new string[] {
                            "Kapre",
                            "Bungisngis",
                            "Sarangay",
                            "Batibat"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room10_MorningStarPendant",
                        artifactName = "Morning Star Pendant",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_MorningStarPendant.png"),
                        artifactDescription = "This pendant symbolizes the beauty of the night sky and the celestial figures featured in traditional Filipino stories about the stars.",
                        questionText = "In Tagalog mythological tradition, which celestial maiden and daughter of Bathala ruled as the goddess of the stars?",
                        choices = new string[] {
                            "Tala",
                            "Mayari",
                            "Hanan",
                            "Idiyanale"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room10_SarimanokFigurine",
                        artifactName = "Sarimanok Brass Figurine",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_SarimanokBrassFigurine.png"),
                        artifactDescription = "This figurine represents a legendary bird in Maranao culture, celebrated for its colorful appearance and its important place in traditional art and symbolism.",
                        questionText = "What mythical legendary fowl of the Maranao people is depicted with vibrant multihued plumage clutching a fish in its beak?",
                        choices = new string[] {
                            "Sarimanok",
                            "Minokawa",
                            "Manaul",
                            "Ibong Adarna"
                        },
                        correctChoiceIndex = 0
                    }
                },
                victoryMessage: "You defeated the Nuno and recovered all artifacts in Room 10!\nTake the portal back and bring them inside Room 10 in the Real World to unlock Alterworld Room 11!",
                outputScenePath: "Assets/Scenes/BattleScene_10.unity"
            );

            // Room 11: Mangkukulam
            CreateBattleSceneFromTemplate(
                roomNumber: 11,
                roomTitle: "Room 11 (Alter Realm)",
                monsterName: "Mangkukulam",
                monsterSpritePath: "Assets/Sprites/Enemy_Room11.png",
                monsterAvatarPath: "Assets/UI/Avatar_Enemy_Room11.png",
                slotData: new List<ArtifactBattleSlot>
                {
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room11_ManunggulJar",
                        artifactName = "Manunggul Burial Jar",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_ManunggulBurialJar.png"),
                        artifactDescription = "This jar represents ancient Filipino burial traditions and beliefs about the journey of the soul after death, expressed through carefully crafted imagery on its lid.",
                        questionText = "Excavated in Palawan, which Neolithic burial jar features a boat with two soul figures paddling toward eternity on its lid?",
                        choices = new string[] {
                            "Manunggul Jar",
                            "Maitum Pot",
                            "Calatagan Pot",
                            "Leta-Leta Vessel"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room11_PaoayChurchModel",
                        artifactName = "Baroque Paoay Church Model",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_BaroquePaoayChurchModel.png"),
                        artifactDescription = "This model represents the distinctive architecture of Spanish colonial churches in the Philippines, known for their massive stone walls and strong structural supports designed to withstand earthquakes.",
                        questionText = "Which UNESCO World Heritage Spanish colonial church in Ilocos Norte is celebrated for its gargantuan earthquake-proof buttresses?",
                        choices = new string[] {
                            "Paoay Church",
                            "San Agustin Church",
                            "Miagao Church",
                            "Santa Maria Church"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room11_TerracesSpade",
                        artifactName = "Rice Terraces Hand Spade",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_RiceTerracesHandSpade.png"),
                        artifactDescription = "This spade represents the agricultural knowledge and engineering skills of indigenous Cordilleran communities, who shaped mountain landscapes into productive rice fields.",
                        questionText = "Which breathtaking mountain agricultural wonder carved by indigenous cordilleran tribes is lauded as an ancient engineering masterpiece?",
                        choices = new string[] {
                            "Banaue Rice Terraces",
                            "Sagada Hanging Coffins",
                            "Maligcong Terraces",
                            "Mount Pulag Plateau"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room11_BambooPipe",
                        artifactName = "Las Piñas Bamboo Pipe",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_LasPinasBambooPipe.png"),
                        artifactDescription = "This pipe represents the creative craftsmanship behind a unique musical tradition that combines bamboo with the design of a European-style pipe organ.",
                        questionText = "In St. Joseph Church, what unique 19th-century musical instrument created by Father Diego Cera contains nearly a thousand bamboo pipes?",
                        choices = new string[] {
                            "Bamboo Organ",
                            "Marimba keyboard",
                            "Wooden Clavier",
                            "Brass Carillon"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room11_MaitumPot",
                        artifactName = "Maitum Anthropomorphic Pot",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_MaitumAnthropomorphicPot.png"),
                        artifactDescription = "This pot represents ancient burial practices in Mindanao, where vessels shaped with human facial features reflected cultural beliefs and traditions surrounding death.",
                        questionText = "Discovered in Sarangani, which unique secondary burial jars feature lids sculpted with distinctive human faces and facial expressions?",
                        choices = new string[] {
                            "Maitum Anthropomorphic Pots",
                            "Manunggul Jars",
                            "Calatagan Ritual Pots",
                            "Kalanay Pottery"
                        },
                        correctChoiceIndex = 0
                    }
                },
                victoryMessage: "You defeated the Mangkukulam and recovered all artifacts in Room 11!\nTake the portal back and bring them inside Room 11 in the Real World to unlock Alterworld Room 12!",
                outputScenePath: "Assets/Scenes/BattleScene_11.unity"
            );

            RedirectAlterworldRooms();
            UpdateBuildSettings();
            Debug.Log("[BattleSceneGenerator] Generated Battle Scenes for Rooms 7 to 11 successfully!");
        }

        [MenuItem("Tools/Scenes/Open BattleScene_12")]
        public static void OpenBattleScene12()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/BattleScene_12.unity", OpenSceneMode.Single);
        }

        [MenuItem("Tools/Setup/Generate Battle Scene for Room 12")]
        public static void GenerateBattleSceneForRoom12()
        {
            EnsureSpriteImport("Assets/Sprites/Enemy_Room12.png");
            EnsureSpriteImport("Assets/UI/Avatar_Enemy_Room12.png");

            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_BonifacioRevolver.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_TandangSoraBowl.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_GeneralLunaSabre.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_VisayanJoanMachete.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_MalvarCavalrySaddle.png");

            // Room 12: Tiyanak
            CreateBattleSceneFromTemplate(
                roomNumber: 12,
                roomTitle: "Room 12 (Alter Realm)",
                monsterName: "Tiyanak",
                monsterSpritePath: "Assets/Sprites/Enemy_Room12.png",
                monsterAvatarPath: "Assets/UI/Avatar_Enemy_Room12.png",
                slotData: new List<ArtifactBattleSlot>
                {
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room12_BonifacioRevolver",
                        artifactName = "Bonifacio Revolver",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_BonifacioRevolver.png"),
                        artifactDescription = "This revolver represents the leadership and determination of the revolutionary organizer who helped unite Filipinos in the struggle against Spanish colonial rule.",
                        questionText = "Which revolutionary hero was honored with the supreme title \"Supremo\" as the primary founder and chief organizer of the Katipunan?",
                        choices = new string[] {
                            "Andres Bonifacio",
                            "Emilio Jacinto",
                            "Macario Sakay",
                            "Ladislao Diwa"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room12_TandangSoraBowl",
                        artifactName = "Tandang Sora Medicine Bowl",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_TandangSoraBowl.png"),
                        artifactDescription = "This bowl symbolizes the compassion and courage of Filipino women who provided food, shelter, and medical assistance to revolutionaries during the fight for independence.",
                        questionText = "Which venerable elderly matriarch, revered as \"Tandang Sora\", bravely provided medical care, shelter, and provisions to revolutionary fighters?",
                        choices = new string[] {
                            "Melchora Aquino",
                            "Gregoria de Jesus",
                            "Teresa Magbanua",
                            "Agueda Kahabagan"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room12_GeneralLunaSabre",
                        artifactName = "General Luna Field Sabre",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_GeneralLunaSabre.png"),
                        artifactDescription = "This sabre represents the military strategy and discipline employed by Filipino commanders who organized revolutionary forces to defend the country against American occupation.",
                        questionText = "Which European-educated military strategist and brilliant general served as Commander of the Philippine Revolutionary Army against the Americans?",
                        choices = new string[] {
                            "General Antonio Luna",
                            "General Artemio Ricarte",
                            "General Miguel Malvar",
                            "General Vicente Lukban"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room12_VisayanJoanMachete",
                        artifactName = "Visayan Joan of Arc Machete",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_VisayanJoanMachete.png"),
                        artifactDescription = "This machete symbolizes the bravery of Filipino women who took active roles in combat and challenged traditional expectations during the Philippine Revolution.",
                        questionText = "Which fearless Ilongga teacher was known as the \"Visayan Joan of Arc\" for leading rebel forces in combat against Spanish and American troops?",
                        choices = new string[] {
                            "Teresa Magbanua",
                            "Trinidad Tecson",
                            "Patrocinio Gamboa",
                            "Nieves Fernandez"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room12_GeneralMalvarSaddle",
                        artifactName = "General Malvar Field Saddle",
                        artifactSprite = LoadSprite("Assets/Sprites/Artifacts/Artifact_MalvarCavalrySaddle.png"),
                        artifactDescription = "This saddle represents the perseverance of Filipino military leaders who continued resisting American forces during the later years of the Philippine-American War.",
                        questionText = "Who was celebrated as the last commanding general of the Philippine Republic to formally surrender in Luzon during the Philippine-American War?",
                        choices = new string[] {
                            "Miguel Malvar",
                            "Vicente Lukban",
                            "Mariano Trias",
                            "Pantaleon Garcia"
                        },
                        correctChoiceIndex = 0
                    }
                },
                victoryMessage: "You conquered the Tiyanak and recovered all 5 artifacts in Room 12!\nReturn to Room 12 in the Real World to restore the final artifacts and fulfill your historic quest across all 12 rooms!",
                outputScenePath: "Assets/Scenes/BattleScene_12.unity"
            );

            RedirectAlterworldRooms();
            UpdateBuildSettings();
            Debug.Log("[BattleSceneGenerator] Generated Battle Scene for Room 12 (Tiyanak) successfully!");
        }

        private static void CreateBattleSceneFromTemplate(
            int roomNumber,
            string roomTitle,
            string monsterName,
            string monsterSpritePath,
            string monsterAvatarPath,
            List<ArtifactBattleSlot> slotData,
            string victoryMessage,
            string outputScenePath)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/BattleScene.unity", OpenSceneMode.Single);
            var qbm = Object.FindAnyObjectByType<QuizBattleManager>();
            var mbc = Object.FindAnyObjectByType<MonsterBattleController>();

            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            // 1. Room Identity
            qbm.GetType().GetField("roomNumber", flags).SetValue(qbm, roomNumber);
            qbm.GetType().GetField("roomTitle", flags).SetValue(qbm, roomTitle);
            qbm.GetType().GetField("monsterSpeakerName", flags).SetValue(qbm, monsterName);

            // 2. Sprites
            var enemySpr = LoadSprite(monsterSpritePath);
            var avatarSpr = LoadSprite(monsterAvatarPath);
            if (enemySpr == null)
            {
                Debug.LogError($"[BattleSceneGenerator] CRITICAL: Could not load enemy sprite at {monsterSpritePath}!");
            }

            // 3. Monster Controller & Images
            mbc.GetType().GetField("monsterSprite", flags).SetValue(mbc, enemySpr);
            var mImgObj = GameObject.Find("MonsterImage");
            if (mImgObj != null) mImgObj.GetComponent<Image>().sprite = enemySpr;

            var canvas = Object.FindAnyObjectByType<Canvas>();
            var pImgTr = canvas != null ? (canvas.transform.Find("MonsterPortraitContainer/Frame/Avatar") ?? canvas.transform.Find("MonsterPortraitContainer/Avatar")) : null;
            if (pImgTr != null)
            {
                var pImg = pImgTr.GetComponent<Image>();
                if (pImg != null)
                {
                    pImg.sprite = avatarSpr != null ? avatarSpr : enemySpr;
                    EditorUtility.SetDirty(pImg);
                    mbc.GetType().GetField("monsterPortraitImage", flags)?.SetValue(mbc, pImg);
                }
            }

            // 4. Artifact Slots Data
            qbm.GetType().GetField("artifactSlots", flags).SetValue(qbm, slotData);

            // 5. Artifact Slot Visuals
            for (int s = 0; s < slotData.Count && s < 5; s++)
            {
                var iconObj = GameObject.Find($"ArtifactSlot_{s}/ArtifactIcon");
                if (iconObj != null && slotData[s].artifactSprite != null)
                {
                    iconObj.GetComponent<Image>().sprite = slotData[s].artifactSprite;
                }
            }

            var slotImagesList = qbm.GetType().GetField("artifactSlotImages", flags).GetValue(qbm) as List<Image>;
            if (slotImagesList != null)
            {
                for (int s = 0; s < slotData.Count && s < slotImagesList.Count; s++)
                {
                    if (slotData[s].artifactSprite != null)
                    {
                        slotImagesList[s].sprite = slotData[s].artifactSprite;
                    }
                }
            }

            // 6. Victory Modal Text
            var vicBody = GameObject.Find("VictoryModal/Window/Body")?.GetComponent<TextMeshProUGUI>();
            if (vicBody != null)
            {
                vicBody.text = victoryMessage;
            }

            EditorUtility.SetDirty(qbm);
            EditorUtility.SetDirty(mbc);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, outputScenePath);
            Debug.Log($"[BattleSceneGenerator] Saved {outputScenePath} for Room {roomNumber} ({monsterName})!");
        }

        public static void GenerateBattleScene()
        {
            string scenePath = "Assets/Scenes/BattleScene.unity";

            // If already exists, we keep its high quality setup
            if (File.Exists(scenePath))
            {
                Debug.Log($"[BattleSceneGenerator] BattleScene.unity already exists at {scenePath}");
                return;
            }

            // Fallback generation if missing
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        public static void RedirectAlterworldRooms()
        {
            // 1. Redirect AltLeftHallway doors (Rooms 1..6)
            if (File.Exists("Assets/Scenes/AltLeftHallway.unity"))
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/AltLeftHallway.unity", OpenSceneMode.Single);
                var doors = Object.FindObjectsByType<RoomDoor>(FindObjectsSortMode.None);
                foreach (var d in doors)
                {
                    var offsetField = typeof(RoomDoor).GetField("returnOffset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    Vector2 currentOffset = offsetField != null ? (Vector2)offsetField.GetValue(d) : new Vector2(0f, 1.8f);
                    int rNum = d.GetRoomNumber();
                    string target = rNum == 1 ? "BattleScene" : $"BattleScene_{rNum}";
                    bool startLocked = (rNum != 1);
                    d.SetConfig(target, d.RoomDisplayName, currentOffset, startLocked);
                    EditorUtility.SetDirty(d);
                }
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[BattleSceneGenerator] Redirected {doors.Length} doors in AltLeftHallway to their respective BattleScenes (BattleScene, BattleScene_2 to BattleScene_6)!");
            }

            // 2. Redirect AltRightHallway doors (Rooms 7..11)
            if (File.Exists("Assets/Scenes/AltRightHallway.unity"))
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/AltRightHallway.unity", OpenSceneMode.Single);
                var doors = Object.FindObjectsByType<RoomDoor>(FindObjectsSortMode.None);
                foreach (var d in doors)
                {
                    var offsetField = typeof(RoomDoor).GetField("returnOffset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    Vector2 currentOffset = offsetField != null ? (Vector2)offsetField.GetValue(d) : new Vector2(0f, 1.8f);
                    int rNum = d.GetRoomNumber();
                    if (rNum >= 7 && rNum <= 12)
                    {
                        string target = $"BattleScene_{rNum}";
                        d.SetConfig(target, d.RoomDisplayName, currentOffset, true);
                        EditorUtility.SetDirty(d);
                    }
                }
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[BattleSceneGenerator] Redirected {doors.Length} doors in AltRightHallway to their respective BattleScenes (BattleScene_7 to BattleScene_12)!");
            }
        }

        public static void UpdateBuildSettings()
        {
            string[] requiredScenes = new string[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/CharacterSelect.unity",
                "Assets/Scenes/Lobby.unity",
                "Assets/Scenes/LeftHallway.unity",
                "Assets/Scenes/RightHallway.unity",
                "Assets/Scenes/AltLobby.unity",
                "Assets/Scenes/AltLeftHallway.unity",
                "Assets/Scenes/AltRightHallway.unity",
                "Assets/Scenes/BattleScene.unity",
                "Assets/Scenes/BattleScene_2.unity",
                "Assets/Scenes/BattleScene_3.unity",
                "Assets/Scenes/BattleScene_4.unity",
                "Assets/Scenes/BattleScene_5.unity",
                "Assets/Scenes/BattleScene_6.unity",
                "Assets/Scenes/BattleScene_7.unity",
                "Assets/Scenes/BattleScene_8.unity",
                "Assets/Scenes/BattleScene_9.unity",
                "Assets/Scenes/BattleScene_10.unity",
                "Assets/Scenes/BattleScene_11.unity",
                "Assets/Scenes/BattleScene_12.unity",
                "Assets/Scenes/Room_1.unity",
                "Assets/Scenes/Room_2.unity",
                "Assets/Scenes/Room_3.unity",
                "Assets/Scenes/Room_4.unity",
                "Assets/Scenes/Room_5.unity",
                "Assets/Scenes/Room_6.unity",
                "Assets/Scenes/Room_7.unity",
                "Assets/Scenes/Room_8.unity",
                "Assets/Scenes/Room_9.unity",
                "Assets/Scenes/Room_10.unity",
                "Assets/Scenes/Room_11.unity",
                "Assets/Scenes/Room_12.unity"
            };

            List<EditorBuildSettingsScene> sceneList = new List<EditorBuildSettingsScene>();
            foreach (var path in requiredScenes)
            {
                if (File.Exists(path))
                {
                    sceneList.Add(new EditorBuildSettingsScene(path, true));
                }
            }

            EditorBuildSettings.scenes = sceneList.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log($"[BattleSceneGenerator] Build Settings updated with {sceneList.Count} scenes (including BattleScene_2 to BattleScene_12)!");
        }

        private static void EnsureSpriteImport(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool dirty = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    dirty = true;
                }
                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    dirty = true;
                }
                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    dirty = true;
                }
                if (dirty)
                {
                    importer.SaveAndReimport();
                }
            }
        }

        private static Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var spr = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (spr != null) return spr;

            var all = AssetDatabase.LoadAllAssetsAtPath(path);
            if (all != null)
            {
                foreach (var a in all)
                {
                    if (a is Sprite s) return s;
                }
            }
            return null;
        }
    }
}
