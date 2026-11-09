using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TopDownGame.Editor
{
    public static class ArtifactUISetup
    {
        [MenuItem("Tools/Setup Artifact Inspection UI")]
        public static void SetupArtifactUI()
        {
            SetupAllRooms();
        }

        [MenuItem("Tools/Artifacts/Setup Room 1 Artifacts")]
        public static void SetupRoom1Artifacts()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Room_1.unity", OpenSceneMode.Single);

            var canvas = GameObject.Find("InGameCanvas");
            if (canvas == null)
            {
                Debug.LogError("InGameCanvas not found in Room_1!");
                return;
            }

            BuildArtifactInspectionCanvasUI(canvas);

            var sprStamp   = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_Stamp.png");
            var sprBook    = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_Book.png");
            var sprPen     = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_PenNib.png");
            var sprGlasses = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_Glasses.png");
            var sprLocket  = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_Locket.png");

            string[] tables = new string[]
            {
                "Tables/Table_TopLeft",
                "Tables/Table_Middle",
                "Tables/Table_TopRight",
                "Tables/Table_BottomLeft",
                "Tables/Table_BottomRight"
            };
            string[] artifactObjectNames = new string[]
            {
                "Artifact_Stamp",
                "Artifact_Book",
                "Artifact_PenNib",
                "Artifact_Glasses",
                "Artifact_Locket"
            };
            string[] names = new string[]
            {
                "University Wax Seal Stamp",
                "Travel Notebook",
                "Dip Pen Nib",
                "Wire-rimmed Spectacles",
                "Gold Locket Pendant"
            };
            Sprite[] sprites = new Sprite[] { sprStamp, sprBook, sprPen, sprGlasses, sprLocket };
            string[] plaqueTexts = new string[]
            {
                "This seal represents Rizal's pursuit of higher education in Spain, where he further developed his knowledge and prepared for his future professional career.",
                "This notebook represents Rizal's experiences abroad, where learning foreign languages allowed him to explore European literature and connect with ideas beyond the Philippines.",
                "This pen nib represents the early schooling that helped shape a young José Rizal's intellectual development during his childhood in Laguna.",
                "These spectacles symbolize Rizal's medical studies and specialized training in Europe, where he pursued knowledge that would help him treat eye diseases.",
                "This pendant symbolizes the important role of family in shaping José Rizal's earliest lessons, values, and introduction to education."
            };
            string[] inspectTexts = new string[]
            {
                "A solid brass university seal stamp bearing official academic insignias. The base is worn smooth from stamping wax certifications.",
                "A leather-bound European travel journal filled with handwritten observations, language translations, and sketches.",
                "Fine tempered gold alloy dip pen nib with a split tip, capable of writing flourished cursive manuscripts.",
                "Thin circular wire frames with curved temple arms. Despite their age, the magnifying lenses remain remarkably clear and intact.",
                "Forged with a delicate top suspension loop and twin oval compartments. Subtle engravings trace the outer rim where a ribbon once held it."
            };

            SetupTableArtifacts(tables, names, sprites, plaqueTexts, inspectTexts);
            SetupVisualArtifacts(tables, artifactObjectNames, sprites);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[ArtifactUISetup] Successfully built Artifact Inspection UI and Artifacts in Room 1!");
        }

        [MenuItem("Tools/Artifacts/Setup Room 2 Artifacts")]
        public static void SetupRoom2Artifacts()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Room_2.unity", OpenSceneMode.Single);

            var canvas = GameObject.Find("InGameCanvas");
            if (canvas == null)
            {
                Debug.LogError("InGameCanvas not found in Room_2!");
                return;
            }

            BuildArtifactInspectionCanvasUI(canvas);

            var sprGalley   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_GalleyProofSheets.png");
            var sprFeather  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_FeatherQuillPen.png");
            var sprPress    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_PrintingPressMatrix.png");
            var sprFili     = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_FirstEditionNovelCopy.png");
            var sprStove    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_AlcoholCookingStove.png");

            string[] tables = new string[]
            {
                "Tables/Table_TopLeft",
                "Tables/Table_Middle",
                "Tables/Table_TopRight",
                "Tables/Table_BottomLeft",
                "Tables/Table_BottomRight"
            };
            string[] artifactObjectNames = new string[]
            {
                "Artifact_GalleyProofSheets",
                "Artifact_FeatherQuillPen",
                "Artifact_PrintingPressMatrix",
                "Artifact_FirstEditionNovelCopy",
                "Artifact_AlcoholCookingStove"
            };
            string[] names = new string[]
            {
                "Galley Proof Sheets",
                "Feather Quill Pen",
                "Printing Press Matrix",
                "First Edition Novel Copy",
                "Alcohol Cooking Stove"
            };
            Sprite[] sprites = new Sprite[] { sprGalley, sprFeather, sprPress, sprFili, sprStove };
            string[] plaqueTexts = new string[]
            {
                "These proof sheets represent the challenges Rizal faced in publishing his first novel and the support he received from fellow Filipinos who believed in his work.",
                "This quill pen symbolizes Rizal's use of literature to expose the suffering of Filipinos under Spanish colonial rule through the characters in his novels.",
                "This printing matrix represents the process of producing Rizal's second novel, which continued his criticism of colonial abuses and social injustice.",
                "This first-edition copy symbolizes the determination and financial support that helped bring Rizal's second novel to publication despite difficulties.",
                "This stove represents the final days of Rizal's life and the farewell poem he left behind as an expression of his love for the Philippines."
            };
            string[] inspectTexts = new string[]
            {
                "Printed on thin European paper with handwritten marginal notations and corrections made during the Berlin printing in 1887.",
                "A trimmed maroon feather quill attached to an ornate golden ferrule. Delicate ink residue remains preserved at the tip.",
                "Sturdy seasoned timber and iron screw mechanism. Worn wooden platen bearing ink stains from early revolutionary publications.",
                "Bound volume opened to the title page of El Filibusterismo, bearing the historic dedication to the martyrdom of GOMBURZA.",
                "Forged brass cylindrical burner with side brackets and a stable base. The hidden fuel chamber housed Rizal's final farewell verses."
            };

            SetupTableArtifacts(tables, names, sprites, plaqueTexts, inspectTexts);
            SetupVisualArtifacts(tables, artifactObjectNames, sprites);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[ArtifactUISetup] Successfully built Artifact Inspection UI and Artifacts in Room 2!");
        }

        [MenuItem("Tools/Artifacts/Setup Room 3 Artifacts")]
        public static void SetupRoom3Artifacts()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Room_3.unity", OpenSceneMode.Single);

            var canvas = GameObject.Find("InGameCanvas");
            if (canvas == null)
            {
                Debug.LogError("InGameCanvas not found in Room_3!");
                return;
            }

            BuildArtifactInspectionCanvasUI(canvas);

            var sprGomburza = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_GomburzaFolio.png");
            var sprCentury  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_PhilippinesCenturyHence.png");
            var sprMalolos  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_LetterToWomenOfMalolos.png");
            var sprSoli     = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_LaSolidaridad.png");
            var sprDiario   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_DiariongTagalog.png");

            string[] tables = new string[]
            {
                "Tables/Table_TopLeft",
                "Tables/Table_Middle",
                "Tables/Table_TopRight",
                "Tables/Table_BottomLeft",
                "Tables/Table_BottomRight"
            };
            string[] artifactObjectNames = new string[]
            {
                "Artifact_GomburzaFolio",
                "Artifact_PhilippinesCenturyHence",
                "Artifact_LetterToWomenOfMalolos",
                "Artifact_LaSolidaridad",
                "Artifact_DiariongTagalog"
            };
            string[] names = new string[]
            {
                "GOMBURZA Memorial Leaflet",
                "Century Hence Serial Article",
                "Malolos Letter Leaflet",
                "La Solidaridad Newspaper Issue",
                "Diariong Tagalog Broadsheet"
            };
            Sprite[] sprites = new Sprite[] { sprGomburza, sprCentury, sprMalolos, sprSoli, sprDiario };
            string[] plaqueTexts = new string[]
            {
                "This leaflet symbolizes the lasting impact of the execution of three Filipino priests on the growth of Filipino nationalism and the fight against colonial injustice.",
                "This article represents Rizal's reflections on the Philippines' future and the possible effects of colonial rule on the nation's destiny.",
                "This leaflet represents Rizal's writings on education, moral values, and the important role of Filipino women in shaping the nation's future.",
                "This newspaper symbolizes the reform movement that sought political and social changes in the Philippines during Spanish colonial rule.",
                "This broadsheet represents the role of early Filipino newspapers in spreading patriotic ideas and giving reformists a platform to express their views."
            };
            string[] inspectTexts = new string[]
            {
                "A folded folio with aged borders, displaying portraits of the three martyred secular priests along with solemn accounts of the Cavite Mutiny.",
                "Broadsheet periodical opened to Rizal's philosophical treatise, exploring Spanish colonial policies and foreseeing future Philippine independence.",
                "Inscribed on weathered parchment with elaborate calligraphic headings in Tagalog, urging Filipino women to reject ignorance and champion enlightenment.",
                "Folded issues of the Propaganda Movement's organ in Barcelona, featuring bold editorials and articles by Del Pilar, Rizal, and López Jaena.",
                "Wide newspaper pages printed with side-by-side columns of Tagalog and Spanish text, featuring nationalistic essays such as Rizal's 'Amor Patrio'."
            };

            SetupTableArtifacts(tables, names, sprites, plaqueTexts, inspectTexts);
            SetupVisualArtifacts(tables, artifactObjectNames, sprites);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[ArtifactUISetup] Successfully built Artifact Inspection UI and Artifacts in Room 3!");
        }

        [MenuItem("Tools/Artifacts/Setup Room 4 Artifacts")]
        public static void SetupRoom4Artifacts()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Room_4.unity", OpenSceneMode.Single);

            var canvas = GameObject.Find("InGameCanvas");
            if (canvas == null)
            {
                Debug.LogError("InGameCanvas not found in Room_4!");
                return;
            }

            BuildArtifactInspectionCanvasUI(canvas);

            var sprOath     = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_MembershipOathCertificate.png");
            var sprWarrant  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_ArrestWarrantDecree.png");
            var sprLedger   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_SecretMemberLedger.png");
            var sprCharter  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_ReorganizedCouncilCharter.png");
            var sprManifest = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_SplitResolutionManifesto.png");

            string[] tables = new string[]
            {
                "Tables/Table_TopLeft",
                "Tables/Table_Middle",
                "Tables/Table_TopRight",
                "Tables/Table_BottomLeft",
                "Tables/Table_BottomRight"
            };
            string[] artifactObjectNames = new string[]
            {
                "Artifact_MembershipOathCertificate",
                "Artifact_ArrestWarrantDecree",
                "Artifact_SecretMemberLedger",
                "Artifact_ReorganizedCouncilCharter",
                "Artifact_SplitResolutionManifesto"
            };
            string[] names = new string[]
            {
                "Membership Oath Certificate",
                "Arrest Warrant Decree",
                "Secret Member Ledger",
                "Reorganized Council Charter",
                "Split Resolution Manifesto"
            };
            Sprite[] sprites = new Sprite[] { sprOath, sprWarrant, sprLedger, sprCharter, sprManifest };
            string[] plaqueTexts = new string[]
            {
                "This certificate represents La Liga Filipina's commitment to unity, mutual support, and the protection of its members against injustice.",
                "This decree symbolizes the colonial authorities' efforts to suppress organizations and individuals who sought change under Spanish rule.",
                "This ledger represents the secret networks formed by Filipino patriots who organized to pursue the country's freedom from Spanish colonial rule.",
                "This charter represents the efforts to revive La Liga Filipina after Rizal's exile and continue its work toward civic reform and national unity.",
                "This manifesto symbolizes the division within the nationalist movement between those who continued supporting peaceful reform and those who pursued independence through revolution."
            };
            string[] inspectTexts = new string[]
            {
                "Illuminated certificate framed with ornate gold filigree borders, sunburst insignia, and signed with a goose quill.",
                "Parchment warrant clamped with a steel brass paperclip and accompanied by a polished molave wood magistrate gavel.",
                "Bound in thick marbled calfskin with numbered registry columns detailing the deliberations and members of the society.",
                "Rolled with delicate turned wooden spindles and adorned with official wax crests validating sovereign decree.",
                "Weathered vellum with hand-stitched ribbon threads and a leaded wax seal bearing the revolutionary emblem."
            };

            SetupTableArtifacts(tables, names, sprites, plaqueTexts, inspectTexts);
            SetupVisualArtifacts(tables, artifactObjectNames, sprites);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[ArtifactUISetup] Successfully built Artifact Inspection UI and Artifacts in Room 4!");
        }

        [MenuItem("Tools/Artifacts/Setup Room 5 Artifacts")]
        public static void SetupRoom5Artifacts()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Room_5.unity", OpenSceneMode.Single);

            var canvas = GameObject.Find("InGameCanvas");
            if (canvas == null)
            {
                Debug.LogError("InGameCanvas not found in Room_5!");
                return;
            }

            BuildArtifactInspectionCanvasUI(canvas);

            var sprForceps = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_SurgicalEyeForceps.png");
            var sprRoll    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_SchoolRollBook.png");
            var sprJar     = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_SpecimenJarWithBeetle.png");
            var sprLamp    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_CoconutOilLamp.png");
            var sprApp     = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_MilitaryDoctorApplication.png");

            string[] tables = new string[]
            {
                "Tables/Table_TopLeft",
                "Tables/Table_Middle",
                "Tables/Table_TopRight",
                "Tables/Table_BottomLeft",
                "Tables/Table_BottomRight"
            };
            string[] artifactObjectNames = new string[]
            {
                "Artifact_SurgicalEyeForceps",
                "Artifact_SchoolRollBook",
                "Artifact_SpecimenJarWithBeetle",
                "Artifact_CoconutOilLamp",
                "Artifact_MilitaryDoctorApplication"
            };
            string[] names = new string[]
            {
                "Surgical Eye Forceps",
                "School Roll Book",
                "Specimen Jar with Beetle",
                "Coconut Oil Lamp",
                "Military Doctor Application"
            };
            Sprite[] sprites = new Sprite[] { sprForceps, sprRoll, sprJar, sprLamp, sprApp };
            string[] plaqueTexts = new string[]
            {
                "These forceps represent Rizal's work as an ophthalmologist during his exile in Dapitan, where he continued providing medical care to patients.",
                "This roll book represents the small community school Rizal established in Talisay, where he educated boys and taught practical skills alongside academic subjects.",
                "This specimen jar represents Rizal's interest in natural history and his collection and study of insects during his stay in Dapitan.",
                "This lamp represents Rizal's community improvement projects in Dapitan, where he applied practical knowledge to help improve local living conditions.",
                "This application represents Rizal's willingness to offer his medical expertise to help people suffering during a public health crisis overseas."
            };
            string[] inspectTexts = new string[]
            {
                "Finely balanced surgical steel with ribbed finger loops and precision curved jaws, engineered for delicate ophthalmic procedures.",
                "Bound in durable dark oilcloth with hand-ruled student columns detailing the curriculum taught to sixteen young scholars.",
                "A heavy glass jar sealed with crimson beeswax and thread, housing a preserved horned beetle specimen of Apogonia rizali.",
                "Blown from heavy tinted glass with an integrated hanging loop and copper oil burner engineered for municipal street illumination.",
                "Aged Spanish clinical admission parchment bearing patient biodata, volunteer requests, and official vice-royal stamps."
            };

            SetupTableArtifacts(tables, names, sprites, plaqueTexts, inspectTexts);
            SetupVisualArtifacts(tables, artifactObjectNames, sprites);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[ArtifactUISetup] Successfully built Artifact Inspection UI and Artifacts in Room 5!");
        }

        [MenuItem("Tools/Artifacts/Setup Room 6 Artifacts")]
        public static void SetupRoom6Artifacts()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Room_6.unity", OpenSceneMode.Single);

            var canvas = GameObject.Find("InGameCanvas");
            if (canvas == null)
            {
                Debug.LogError("InGameCanvas not found in Room_6!");
                return;
            }

            BuildArtifactInspectionCanvasUI(canvas);

            var sprBaton = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_RoyalGovernorBaton.png");
            var sprGavel = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_TribunalCourtGavel.png");
            var sprScale = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_BandalaQuotaScale.png");
            var sprBook  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_ParishRegistryBook.png");
            var sprCoin  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_SilverColumnarCoin.png");

            string[] tables = new string[]
            {
                "Tables/Table_TopLeft",
                "Tables/Table_Middle",
                "Tables/Table_TopRight",
                "Tables/Table_BottomLeft",
                "Tables/Table_BottomRight"
            };
            string[] artifactObjectNames = new string[]
            {
                "Artifact_RoyalGovernorBaton",
                "Artifact_TribunalCourtGavel",
                "Artifact_BandalaQuotaScale",
                "Artifact_ParishRegistryBook",
                "Artifact_SilverColumnarCoin"
            };
            string[] names = new string[]
            {
                "Royal Governor Baton",
                "Tribunal Court Gavel",
                "Bandala Quota Scale",
                "Parish Registry Book",
                "Silver Columnar Coin"
            };
            Sprite[] sprites = new Sprite[] { sprBaton, sprGavel, sprScale, sprBook, sprCoin };
            string[] plaqueTexts = new string[]
            {
                "This baton represents the authority held by the highest colonial official, who governed the Philippines in the name of the Spanish Crown.",
                "This gavel symbolizes the colonial judicial institutions responsible for administering justice and advising the highest government authorities.",
                "This scale represents the compulsory agricultural purchasing system that burdened Filipino farmers by requiring them to sell their produce to colonial authorities under government-set terms.",
                "This registry book represents the extensive influence of Spanish religious orders over local communities and colonial affairs in the Philippines.",
                "This coin represents the financial support provided by the Spanish Crown's colonial treasury in Mexico to help fund the administration of the Philippines."
            };
            string[] inspectTexts = new string[]
            {
                "Turned from indigenous kamagong ironwood with gold-banded ends. Carried during formal court sessions and vice-royal ceremonies.",
                "Dense dark molave wood with an ergonomic turned handle. Indentations on the sounding block bear witness to historic verdicts rendered.",
                "Solid cast brass with counterbalanced pans suspended from a central pivot pillar. The base is weighted with lead for precision balance.",
                "Bound in embossed dark cowhide with brass locking clasp and gilt-edged leaves. Heavily annotated margins reflect church governance.",
                "Minted silver columnar coins bearing Spanish colonial crests, representing the annual Real Situado financial subsidy from Mexico."
            };

            SetupTableArtifacts(tables, names, sprites, plaqueTexts, inspectTexts);
            SetupVisualArtifacts(tables, artifactObjectNames, sprites);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[ArtifactUISetup] Successfully built Artifact Inspection UI and Artifacts in Room 6!");
        }

        [MenuItem("Tools/Artifacts/Setup Room 7 Artifacts")]
        public static void SetupRoom7Artifacts()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Room_7.unity", OpenSceneMode.Single);

            var canvas = GameObject.Find("InGameCanvas");
            if (canvas == null)
            {
                Debug.LogError("InGameCanvas not found in Room_7!");
                return;
            }

            BuildArtifactInspectionCanvasUI(canvas);

            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_LaSolidaridadInaugural.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_FrayBotodBooklet.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_EditorialOfficeBadge.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_SpoliariumGoldMedal.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_PropagandistFundReceipt.png");

            var sprSolidaridad = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_LaSolidaridadInaugural.png");
            var sprFrayBotod   = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_FrayBotodBooklet.png");
            var sprBadge       = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_EditorialOfficeBadge.png");
            var sprSpoliarium  = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_SpoliariumGoldMedal.png");
            var sprReceipt     = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_PropagandistFundReceipt.png");

            string[] tables = new string[]
            {
                "Tables/Table_TopLeft",
                "Tables/Table_Middle",
                "Tables/Table_TopRight",
                "Tables/Table_BottomLeft",
                "Tables/Table_BottomRight"
            };
            string[] artifactObjectNames = new string[]
            {
                "Artifact_LaSolidaridadInaugural",
                "Artifact_FrayBotodBooklet",
                "Artifact_EditorialOfficeBadge",
                "Artifact_SpoliariumGoldMedal",
                "Artifact_PropagandistFundReceipt"
            };
            string[] names = new string[]
            {
                "La Solidaridad Inaugural Issue",
                "Fray Botod Satirical Booklet",
                "Editorial Office Press Badge",
                "Spoliarium Gold Medal",
                "Propagandist Fund Receipt"
            };
            Sprite[] sprites = new Sprite[] { sprSolidaridad, sprFrayBotod, sprBadge, sprSpoliarium, sprReceipt };
            string[] plaqueTexts = new string[]
            {
                "This newspaper represents the efforts of Filipino reformists in Spain to spread their ideas and advocate for political and social changes in the Philippines.",
                "This booklet represents the use of satire and literature to criticize the abuses and misconduct of some Spanish friars during the colonial period.",
                "This badge represents the work of Filipino reformists who organized their publication activities abroad to bring attention to the concerns of their homeland.",
                "This medal symbolizes the achievements of Filipino artists abroad and the recognition of Filipino talent during the Spanish colonial period.",
                "This receipt represents the financial contributions of Filipino supporters who helped sustain reformist activities and publications in Spain."
            };
            string[] inspectTexts = new string[]
            {
                "The historic yellowed newsprint of the premier February 15, 1889 issue published in Barcelona. Clean typeset Spanish columns advocate for freedom of speech, assembly, and Philippine parliamentary representation.",
                "A worn palm-sized pamphlet with handwritten Tagalog and Spanish annotations. The satirical sketch satirizes the portly Augustinian friar Botod, personifying clerical greed and abuses.",
                "A polished press credential badge with metal clip bearing the masthead seal of La Solidaridad. It granted reformist contributors and correspondents safe passage across European publishing circles.",
                "A gleaming gilded medal resting upon rich crimson velvet within an ornate polished mahogany presentation box. Inscribed to Juan Luna at the 1884 Madrid National Exposition of Fine Arts.",
                "A crisp parchment treasury receipt stamped with the official wax seal of the Manila patriotic committee. It acknowledges clandestine monetary contributions smuggled to Madrid to sustain the reform movement."
            };

            SetupTableArtifacts(tables, names, sprites, plaqueTexts, inspectTexts);
            SetupVisualArtifacts(tables, artifactObjectNames, sprites);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[ArtifactUISetup] Successfully built Artifact Inspection UI and Artifacts in Room 7!");
        }

        [MenuItem("Tools/Artifacts/Setup Room 8 Artifacts")]
        public static void SetupRoom8Artifacts()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Room_8.unity", OpenSceneMode.Single);

            var canvas = GameObject.Find("InGameCanvas");
            if (canvas == null)
            {
                Debug.LogError("InGameCanvas not found in Room_8!");
                return;
            }

            BuildArtifactInspectionCanvasUI(canvas);

            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_TornCedulaCertificate.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_RevolutionaryKatipunanBolo.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_BloodOathSigningKnife.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_ActaDeLaProclamacion.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_RemingtonRollingBlockRifle.png");

            var sprCedula = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_TornCedulaCertificate.png");
            var sprBolo   = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_RevolutionaryKatipunanBolo.png");
            var sprKnife  = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_BloodOathSigningKnife.png");
            var sprActa   = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_ActaDeLaProclamacion.png");
            var sprRifle  = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_RemingtonRollingBlockRifle.png");

            string[] tables = new string[]
            {
                "Tables/Table_TopLeft",
                "Tables/Table_Middle",
                "Tables/Table_TopRight",
                "Tables/Table_BottomLeft",
                "Tables/Table_BottomRight"
            };
            string[] artifactObjectNames = new string[]
            {
                "Artifact_TornCedulaCertificate",
                "Artifact_RevolutionaryKatipunanBolo",
                "Artifact_BloodOathSigningKnife",
                "Artifact_ActaDeLaProclamacion",
                "Artifact_RemingtonRollingBlockRifle"
            };
            string[] names = new string[]
            {
                "Torn Cédula Certificate",
                "Revolutionary Katipunan Bolo",
                "Blood Oath Signing Knife",
                "Acta de la Proclamación",
                "Remington Rolling-Block Rifle"
            };
            Sprite[] sprites = new Sprite[] { sprCedula, sprBolo, sprKnife, sprActa, sprRifle };
            string[] plaqueTexts = new string[]
            {
                "This certificate represents the act of defiance by Filipino revolutionaries who rejected a symbol of Spanish colonial authority during the uprising of 1896.",
                "This bolo symbolizes the courage of Katipunan members who took up arms against Spanish colonial rule during the Philippine Revolution.",
                "This knife represents the initiation practices of the Katipunan, through which members demonstrated their loyalty and commitment to the revolutionary cause.",
                "This document represents the historic declaration of Philippine independence in 1898 after years of resistance against Spanish colonial rule.",
                "This rifle symbolizes the sacrifices of Filipino soldiers who defended their homeland during the struggle against American forces following the Spanish colonial period."
            };
            string[] inspectTexts = new string[]
            {
                "Two ripped fragments of Spanish colonial cédulas personales. The deliberate tears across official stamps and tax amounts marked the irrevocable cry for national liberation.",
                "A formidable curved steel talibong blade with a rich wooden hilt and carved scabbard. Standard weapon carried by Katipuneros during the 1896 uprising.",
                "A ceremonial dagger fitted with an ornate brass pommel, crossguard, and an inset blood-red gemstone. Used during Katipunan sandugo initiation rites.",
                "An open printed folio of the historic 1898 declaration, detailing the grievances against Spanish colonial rule and proclaiming Philippine independence.",
                "A rugged single-shot rolling-block military rifle with a polished walnut stock and long steel barrel, favored by Filipino soldiers during the revolution."
            };

            SetupTableArtifacts(tables, names, sprites, plaqueTexts, inspectTexts);
            SetupVisualArtifacts(tables, artifactObjectNames, sprites);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[ArtifactUISetup] Successfully built Artifact Inspection UI and Artifacts in Room 8!");
        }

        [MenuItem("Tools/Artifacts/Setup Room 9 Artifacts")]
        public static void SetupRoom9Artifacts()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Room_9.unity", OpenSceneMode.Single);

            var canvas = GameObject.Find("InGameCanvas");
            if (canvas == null)
            {
                Debug.LogError("InGameCanvas not found in Room_9!");
                return;
            }

            BuildArtifactInspectionCanvasUI(canvas);

            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_WovenTnalakCloth.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_BoneTattooChisel.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_BaybayinBambooStrip.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_AncestralBululCarving.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_AgungBrassGong.png");

            var sprCloth  = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_WovenTnalakCloth.png");
            var sprChisel = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_BoneTattooChisel.png");
            var sprBamboo = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_BaybayinBambooStrip.png");
            var sprBulul  = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_AncestralBululCarving.png");
            var sprGong   = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_AgungBrassGong.png");

            string[] tables = new string[]
            {
                "Tables/Table_TopLeft",
                "Tables/Table_Middle",
                "Tables/Table_TopRight",
                "Tables/Table_BottomLeft",
                "Tables/Table_BottomRight"
            };
            string[] artifactObjectNames = new string[]
            {
                "Artifact_WovenTnalakCloth",
                "Artifact_BoneTattooChisel",
                "Artifact_BaybayinBambooStrip",
                "Artifact_AncestralBululCarving",
                "Artifact_AgungBrassGong"
            };
            string[] names = new string[]
            {
                "Woven T'nalak Cloth",
                "Bone Tattoo Chisel",
                "Baybayin Bamboo Strip",
                "Ancestral Bulul Carving",
                "Agung Brass Gong"
            };
            Sprite[] sprites = new Sprite[] { sprCloth, sprChisel, sprBamboo, sprBulul, sprGong };
            string[] plaqueTexts = new string[]
            {
                "This cloth represents the T'boli people's traditional weaving practices, in which abaca fibers are transformed into intricate patterns connected to cultural beliefs and dream-inspired designs.",
                "This chisel represents the traditional tattooing practices of Visayan communities, whose decorated bodies led Spanish chroniclers to refer to them as the \"Pintados,\" or painted ones.",
                "This strip represents the indigenous writing traditions of the Philippines, including a syllabic script used by Tagalog communities before and during the early Spanish colonial period.",
                "This carving represents the Ifugao people's spiritual traditions and their use of wooden figures associated with protecting rice and ensuring a plentiful harvest.",
                "This gong represents the musical traditions of communities in the southern Philippines, where gongs and other instruments are played together in ensembles during cultural and social occasions."
            };
            string[] inspectTexts = new string[]
            {
                "Finely woven from natural abaca fibers and dyed with organic tree bark and roots. The intricate geometric patterns are dream-woven by T'boli master weavers.",
                "Carved bamboo tapping stick fitted with a sharp bone needle and bound with natural cord. Used in traditional batok tattooing rituals across the archipelago.",
                "A seasoned bamboo strip inscribed with ancient pre-colonial syllabic characters etched smoothly into the outer rind using a small iron point.",
                "A seated ancestral Bulul figure carved from dense indigenous hardwood with arms crossed over folded knees, seasoned by traditional sacrificial oils.",
                "A heavy bossed bronze gong with a deep, curved rim and central knob. Hand-beaten to produce deep, resonant tones during ceremonial gatherings."
            };

            SetupTableArtifacts(tables, names, sprites, plaqueTexts, inspectTexts);
            SetupVisualArtifacts(tables, artifactObjectNames, sprites);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[ArtifactUISetup] Successfully built Artifact Inspection UI and Artifacts in Room 9!");
        }

        [MenuItem("Tools/Artifacts/Setup Room 10 Artifacts")]
        public static void SetupRoom10Artifacts()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Room_10.unity", OpenSceneMode.Single);

            var canvas = GameObject.Find("InGameCanvas");
            if (canvas == null)
            {
                Debug.LogError("InGameCanvas not found in Room_10!");
                return;
            }

            BuildArtifactInspectionCanvasUI(canvas);

            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_BakunawaTerracottaFigurine.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_TikbalangHorseHoofTalisman.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_ClayTobaccoPipeOfGiant.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_MorningStarPendant.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_SarimanokBrassFigurine.png");

            var sprDragon     = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_BakunawaTerracottaFigurine.png");
            var sprHornAmulet = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_TikbalangHorseHoofTalisman.png");
            var sprPipe       = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_ClayTobaccoPipeOfGiant.png");
            var sprAnvil      = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_MorningStarPendant.png");
            var sprSarimanok  = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_SarimanokBrassFigurine.png");

            string[] tables = new string[]
            {
                "Tables/Table_TopLeft",
                "Tables/Table_Middle",
                "Tables/Table_TopRight",
                "Tables/Table_BottomLeft",
                "Tables/Table_BottomRight"
            };
            string[] artifactObjectNames = new string[]
            {
                "Artifact_BakunawaTerracottaFigurine",
                "Artifact_TikbalangHorseHoofTalisman",
                "Artifact_ClayTobaccoPipeOfGiant",
                "Artifact_MorningStarPendant",
                "Artifact_SarimanokBrassFigurine"
            };
            string[] names  = new string[]
            {
                "Bakunawa Terracotta Figurine",
                "Tikbalang Horse-Hoof Talisman",
                "Clay Tobacco Pipe of Giant",
                "Morning Star Pendant",
                "Sarimanok Brass Figurine"
            };
            Sprite[] sprites = new Sprite[] { sprDragon, sprHornAmulet, sprPipe, sprAnvil, sprSarimanok };
            string[] plaqueTexts = new string[]
            {
                "This figurine represents the powerful serpent-like creature in Philippine mythology associated with eclipses and ancient explanations for changes in the appearance of the moon.",
                "This talisman represents a creature in Philippine folklore known for its unusual combination of human and horse-like features and its association with forests and mountain paths.",
                "This pipe represents a giant in Philippine folklore believed to dwell among large trees and frighten people who wander into secluded places.",
                "This pendant symbolizes the beauty of the night sky and the celestial figures featured in traditional Filipino stories about the stars.",
                "This figurine represents a legendary bird in Maranao culture, celebrated for its colorful appearance and its important place in traditional art and symbolism."
            };
            string[] inspectTexts = new string[]
            {
                "Molded from porous terracotta clay with sinuous serpentine coils, a spiked dorsal crest, and gaping maw ready to swallow the moon.",
                "A horse-hoof talisman bound tightly with braided leather cords, carved charms, and iron amulets used by wanderers to pierce forest illusions.",
                "A curved wooden smoking pipe with a speckled bowl and blackened chamber, seasoned by aromatic tobacco smoked high in the boughs of ancient balete trees.",
                "An eight-pointed morning star medallion set within a plush velvet-lined wooden presentation case, honoring Tala the celestial goddess of stars.",
                "Cast in brass with vibrant polychrome inlay and feathered okir scrollwork, depicting the mythical bird of good fortune and royal prestige."
            };

            SetupTableArtifacts(tables, names, sprites, plaqueTexts, inspectTexts);
            SetupVisualArtifacts(tables, artifactObjectNames, sprites);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[ArtifactUISetup] Successfully built Artifact Inspection UI and Artifacts in Room 10!");
        }

        [MenuItem("Tools/Artifacts/Setup Room 11 Artifacts")]
        public static void SetupRoom11Artifacts()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Room_11.unity", OpenSceneMode.Single);

            var canvas = GameObject.Find("InGameCanvas");
            if (canvas == null)
            {
                Debug.LogError("InGameCanvas not found in Room_11!");
                return;
            }

            BuildArtifactInspectionCanvasUI(canvas);

            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_ManunggulBurialJar.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_BaroquePaoayChurchModel.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_RiceTerracesHandSpade.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_LasPinasBambooPipe.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_MaitumAnthropomorphicPot.png");

            var sprBurial = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_ManunggulBurialJar.png");
            var sprChurch = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_BaroquePaoayChurchModel.png");
            var sprSpade  = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_RiceTerracesHandSpade.png");
            var sprPipe   = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_LasPinasBambooPipe.png");
            var sprMaitum = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_MaitumAnthropomorphicPot.png");

            string[] tables = new string[]
            {
                "Tables/Table_TopLeft",
                "Tables/Table_Middle",
                "Tables/Table_TopRight",
                "Tables/Table_BottomLeft",
                "Tables/Table_BottomRight"
            };
            string[] artifactObjectNames = new string[]
            {
                "Artifact_ManunggulBurialJar",
                "Artifact_BaroquePaoayChurchModel",
                "Artifact_RiceTerracesHandSpade",
                "Artifact_LasPinasBambooPipe",
                "Artifact_MaitumAnthropomorphicPot"
            };
            string[] names  = new string[]
            {
                "Manunggul Burial Jar",
                "Baroque Paoay Church Model",
                "Rice Terraces Hand Spade",
                "Las Piñas Bamboo Pipe",
                "Maitum Anthropomorphic Pot"
            };
            Sprite[] sprites = new Sprite[] { sprBurial, sprChurch, sprSpade, sprPipe, sprMaitum };
            string[] plaqueTexts = new string[]
            {
                "This jar represents ancient Filipino burial traditions and beliefs about the journey of the soul after death, expressed through carefully crafted imagery on its lid.",
                "This model represents the distinctive architecture of Spanish colonial churches in the Philippines, known for their massive stone walls and strong structural supports designed to withstand earthquakes.",
                "This spade represents the agricultural knowledge and engineering skills of indigenous Cordilleran communities, who shaped mountain landscapes into productive rice fields.",
                "This pipe represents the creative craftsmanship behind a unique musical tradition that combines bamboo with the design of a European-style pipe organ.",
                "This pot represents ancient burial practices in Mindanao, where vessels shaped with human facial features reflected cultural beliefs and traditions surrounding death."
            };
            string[] inspectTexts = new string[]
            {
                "A masterwork Neolithic secondary burial jar discovered in Palawan. Incised curvilinear wave scrolls adorn the shoulders, and the lid features two souls sailing a bangka to the spirit realm.",
                "An intricate architectural model of Paoay Church in Ilocos Norte, showcasing 24 massive contoured stone buttresses and the iconic detached coral-stone bell tower in Earthquake Baroque style.",
                "A hand-forged steel agricultural trowel with a beveled spade tip and weathered wooden handle, used in the cultivation and stone wall maintenance of ancient high-altitude rice terraces.",
                "A seasoned bamboo organ pipe crafted from indigenous mountain bamboo, tuned to acoustic perfection for the historic 1824 Bamboo Organ in Las Piñas.",
                "Prehistoric secondary burial pottery discovered at Ayub Cave in Sarangani. The terracotta lid is sculptured in the likeness of a human head with detailed expressive facial features."
            };

            SetupTableArtifacts(tables, names, sprites, plaqueTexts, inspectTexts);
            SetupVisualArtifacts(tables, artifactObjectNames, sprites);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[ArtifactUISetup] Successfully built Artifact Inspection UI and Artifacts in Room 11!");
        }

        [MenuItem("Tools/Artifacts/Setup Room 12 Artifacts")]
        public static void SetupRoom12Artifacts()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Room_12.unity", OpenSceneMode.Single);

            var canvas = GameObject.Find("InGameCanvas");
            if (canvas == null)
            {
                Debug.LogError("InGameCanvas not found in Room_12!");
                return;
            }

            BuildArtifactInspectionCanvasUI(canvas);

            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_BonifacioRevolver.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_TandangSoraBowl.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_VisayanJoanMachete.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_GeneralLunaSabre.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_MalvarCavalrySaddle.png");

            var sprRevolver = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_BonifacioRevolver.png");
            var sprBowl     = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_TandangSoraBowl.png");
            var sprBolo     = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_VisayanJoanMachete.png");
            var sprSaber    = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_GeneralLunaSabre.png");
            var sprSaddle   = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_MalvarCavalrySaddle.png");

            string[] tables = new string[]
            {
                "Tables/Table_TopLeft",
                "Tables/Table_Middle",
                "Tables/Table_TopRight",
                "Tables/Table_BottomLeft",
                "Tables/Table_BottomRight"
            };
            string[] artifactObjectNames = new string[]
            {
                "Artifact_BonifacioRevolver",
                "Artifact_TandangSoraBowl",
                "Artifact_GeneralLunaSabre",
                "Artifact_VisayanJoanMachete",
                "Artifact_MalvarCavalrySaddle"
            };
            string[] names  = new string[]
            {
                "Bonifacio Revolver",
                "Tandang Sora Medicine Bowl",
                "General Luna Field Sabre",
                "Visayan Joan of Arc Machete",
                "General Malvar Field Saddle"
            };
            Sprite[] sprites = new Sprite[] { sprRevolver, sprBowl, sprSaber, sprBolo, sprSaddle };
            string[] plaqueTexts = new string[]
            {
                "This revolver represents the leadership and determination of the revolutionary organizer who helped unite Filipinos in the struggle against Spanish colonial rule.",
                "This bowl symbolizes the compassion and courage of Filipino women who provided food, shelter, and medical assistance to revolutionaries during the fight for independence.",
                "This sabre represents the military strategy and discipline employed by Filipino commanders who organized revolutionary forces to defend the country against American occupation.",
                "This machete symbolizes the bravery of Filipino women who took active roles in combat and challenged traditional expectations during the Philippine Revolution.",
                "This saddle represents the perseverance of Filipino military leaders who continued resisting American forces during the later years of the Philippine-American War."
            };
            string[] inspectTexts = new string[]
            {
                "Blued steel frame fitted with polished walnut grips. The cylinder rotates smoothly, and faint patriotic insigne are stamped into the sideplate.",
                "Hammered from heavy solid brass with a flared lip. The polished interior retains subtle patina marks from ceremonial quill dips and wax seals.",
                "Tempered carbon steel blade etched with fine decorative scrollwork near the ricasso. The brass three-bar guard protects a ribbed leather grip bound with twisted brass wire.",
                "Hand-forged tempered leaf spring steel with an aggressive clip-point tip and carved carabao horn handle. Battle notches along the spine testify to historical combat.",
                "Crafted from thick, conditioned cowhide over a wooden saddle tree. Heavy brass rivets and cinch buckles secure the stirrup leathers, showing genuine battle-worn patina from campaigns across Luzon."
            };

            SetupTableArtifacts(tables, names, sprites, plaqueTexts, inspectTexts);
            SetupVisualArtifacts(tables, artifactObjectNames, sprites);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[ArtifactUISetup] Successfully built Artifact Inspection UI and Artifacts in Room 12!");
        }

        [MenuItem("Tools/Artifacts/Setup All Rooms Artifacts")]
        public static void SetupAllRooms()
        {
            SetupRoom1Artifacts();
            SetupRoom2Artifacts();
            SetupRoom3Artifacts();
            SetupRoom4Artifacts();
            SetupRoom5Artifacts();
            SetupRoom6Artifacts();
            SetupRoom7Artifacts();
            SetupRoom8Artifacts();
            SetupRoom9Artifacts();
            SetupRoom10Artifacts();
            SetupRoom11Artifacts();
            SetupRoom12Artifacts();
            Debug.Log("[ArtifactUISetup] Successfully configured artifacts in all rooms!");
        }

        public static void BuildArtifactInspectionCanvasUI(GameObject canvas)
        {
            var canvasComp = canvas.GetComponent<Canvas>();
            if (canvasComp != null)
            {
                canvasComp.renderMode = RenderMode.ScreenSpaceCamera;
                var cam = Camera.main;
                if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
                canvasComp.worldCamera = cam;
                canvasComp.planeDistance = 10f;
                canvasComp.sortingOrder = 30000;
                EditorUtility.SetDirty(canvasComp);
            }

            // Remove existing panel if present
            var existing = canvas.transform.Find("ArtifactInspectionPanel");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            // Load UI Sprites
            var dboxSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/DialogueBox_Clean.png");
            if (dboxSprite == null) dboxSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/DialogueBox.png");

            var frameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/AvatarFrame_Clean.png");
            if (frameSprite == null) frameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/AvatarFrame.png");

            var plaqueSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/PlaqueFrame_Clean.png");

            var girlAvatar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Avatar_Girl.png");
            if (girlAvatar == null) girlAvatar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/ThoughtBubbleAvatar_Darla.png");

            var boyAvatar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Avatar_Boy.png");
            if (boyAvatar == null) boyAvatar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/ThoughtBubbleAvatar_Darrel.png");

            // 1. Root Panel
            var rootPanel = new GameObject("ArtifactInspectionPanel", typeof(RectTransform));
            rootPanel.transform.SetParent(canvas.transform, false);
            var rootRt = rootPanel.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            // 1b. Dimmer Background (Visual backdrop only)
            var bgDismissObj = new GameObject("BackgroundDismiss", typeof(RectTransform), typeof(Image));
            bgDismissObj.transform.SetParent(rootPanel.transform, false);
            var bgRt = bgDismissObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImg = bgDismissObj.GetComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.65f);
            bgImg.raycastTarget = false;

            // 2. Dialogue Box (Bottom)
            var dboxObj = new GameObject("DialogueBox", typeof(RectTransform), typeof(Image));
            dboxObj.transform.SetParent(rootPanel.transform, false);
            var dboxRt = dboxObj.GetComponent<RectTransform>();
            dboxRt.anchorMin = new Vector2(0.5f, 0f);
            dboxRt.anchorMax = new Vector2(0.5f, 0f);
            dboxRt.pivot = new Vector2(0.5f, 0f);
            dboxRt.anchoredPosition = new Vector2(110f, 35f);
            dboxRt.sizeDelta = new Vector2(1050f, 287f);
            var dboxImg = dboxObj.GetComponent<Image>();
            dboxImg.sprite = dboxSprite;
            dboxImg.preserveAspect = true;

            // 2b. Avatar Frame (Character Background)
            var avFrameObj = new GameObject("AvatarFrame", typeof(RectTransform), typeof(Image));
            avFrameObj.transform.SetParent(dboxObj.transform, false);
            var avFrameRt = avFrameObj.GetComponent<RectTransform>();
            avFrameRt.anchorMin = new Vector2(0f, 0f);
            avFrameRt.anchorMax = new Vector2(0f, 0f);
            avFrameRt.pivot = new Vector2(1f, 0f);
            avFrameRt.anchoredPosition = new Vector2(-15f, 0f);
            avFrameRt.sizeDelta = new Vector2(280f, 290f);
            var avFrameImg = avFrameObj.GetComponent<Image>();
            avFrameImg.sprite = frameSprite;
            avFrameImg.color = Color.white;
            avFrameImg.preserveAspect = true;

            // 2c. Avatar Portrait inside Frame
            var avPortObj = new GameObject("AvatarPortrait", typeof(RectTransform), typeof(Image));
            avPortObj.transform.SetParent(avFrameObj.transform, false);
            var avPortRt = avPortObj.GetComponent<RectTransform>();
            avPortRt.anchorMin = new Vector2(0.5f, 0.5f);
            avPortRt.anchorMax = new Vector2(0.5f, 0.5f);
            avPortRt.pivot = new Vector2(0.5f, 0.5f);
            avPortRt.anchoredPosition = new Vector2(5f, 8f);
            avPortRt.sizeDelta = new Vector2(190f, 190f);
            var avPortImg = avPortObj.GetComponent<Image>();
            avPortImg.preserveAspect = true;
            avPortImg.sprite = boyAvatar;

            // 2d. Character Name Text (in dark ribbon)
            var nameObj = new GameObject("CharacterName", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameObj.transform.SetParent(dboxObj.transform, false);
            var nameRt = nameObj.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0f, 1f);
            nameRt.anchorMax = new Vector2(0f, 1f);
            nameRt.pivot = new Vector2(0f, 1f);
            nameRt.anchoredPosition = new Vector2(85f, -28f);
            nameRt.sizeDelta = new Vector2(260f, 45f);
            var nameTmp = nameObj.GetComponent<TextMeshProUGUI>();
            nameTmp.text = "Darrel";
            nameTmp.fontSize = 28f;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.color = new Color(1f, 0.95f, 0.85f, 1f);

            // 2e. Dialogue Text (in parchment area)
            var diagObj = new GameObject("DialogueText", typeof(RectTransform), typeof(TextMeshProUGUI));
            diagObj.transform.SetParent(dboxObj.transform, false);
            var diagRt = diagObj.GetComponent<RectTransform>();
            diagRt.anchorMin = new Vector2(0f, 0f);
            diagRt.anchorMax = new Vector2(1f, 1f);
            diagRt.offsetMin = new Vector2(55f, 30f);
            diagRt.offsetMax = new Vector2(-110f, -100f);
            var diagTmp = diagObj.GetComponent<TextMeshProUGUI>();
            diagTmp.text = "what should i do";
            diagTmp.fontSize = 25f;
            diagTmp.color = new Color(0.18f, 0.12f, 0.08f, 1f);

            // 2f. Dialogue Arrow Button (Acts as close / back button on bottom-right arrow)
            var arrowBtnObj = new GameObject("DialogueArrowButton", typeof(RectTransform), typeof(Image), typeof(Button));
            arrowBtnObj.transform.SetParent(dboxObj.transform, false);
            var arrowRt = arrowBtnObj.GetComponent<RectTransform>();
            arrowRt.anchorMin = new Vector2(1f, 0f);
            arrowRt.anchorMax = new Vector2(1f, 0f);
            arrowRt.pivot = new Vector2(0.5f, 0.5f);
            arrowRt.anchoredPosition = new Vector2(-67f, 41.5f);
            arrowRt.sizeDelta = new Vector2(85f, 75f);
            var arrowImg = arrowBtnObj.GetComponent<Image>();
            arrowImg.color = new Color(1f, 1f, 1f, 0.001f);
            arrowImg.raycastTarget = true;
            var arrowBtn = arrowBtnObj.GetComponent<Button>();
            arrowBtn.targetGraphic = arrowImg;
            var aColors = arrowBtn.colors;
            aColors.normalColor = new Color(1f, 1f, 1f, 0.001f);
            aColors.highlightedColor = new Color(1f, 0.85f, 0.3f, 0.35f);
            aColors.pressedColor = new Color(0.9f, 0.6f, 0.1f, 0.55f);
            aColors.selectedColor = new Color(1f, 0.85f, 0.3f, 0.35f);
            arrowBtn.colors = aColors;

            // 3. Choice Buttons Container
            var choiceBox = new GameObject("ChoiceButtons", typeof(RectTransform));
            choiceBox.transform.SetParent(rootPanel.transform, false);
            var choiceRt = choiceBox.GetComponent<RectTransform>();
            choiceRt.anchorMin = new Vector2(1f, 0f);
            choiceRt.anchorMax = new Vector2(1f, 0f);
            choiceRt.pivot = new Vector2(1f, 0f);
            choiceRt.anchoredPosition = new Vector2(-65f, 345f);
            choiceRt.sizeDelta = new Vector2(360f, 220f);

            var vlg = choiceBox.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.spacing = 10f;
            vlg.childControlWidth = false;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            var btnRestoreObj  = CreateChoiceButton("RestoreArtifactButton", "Place from bag", Vector2.zero, choiceBox, plaqueSprite);
            var btnArtifactObj = CreateChoiceButton("InspectArtifactButton", "inspect artifact", Vector2.zero, choiceBox, plaqueSprite);
            var btnPlaqueObj   = CreateChoiceButton("InspectPlaqueButton",   "inspect plaque",   Vector2.zero, choiceBox, plaqueSprite);

            // 4. Plaque Popup
            var plaquePopupObj = new GameObject("PlaquePopup", typeof(RectTransform), typeof(Image));
            plaquePopupObj.transform.SetParent(rootPanel.transform, false);
            var pPopupRt = plaquePopupObj.GetComponent<RectTransform>();
            pPopupRt.anchorMin = new Vector2(0.5f, 0.5f);
            pPopupRt.anchorMax = new Vector2(0.5f, 0.5f);
            pPopupRt.pivot = new Vector2(0.5f, 0.5f);
            pPopupRt.anchoredPosition = new Vector2(0f, 110f);
            pPopupRt.sizeDelta = new Vector2(920f, 280f);
            var pPopupImg = plaquePopupObj.GetComponent<Image>();
            pPopupImg.sprite = plaqueSprite;

            // Plaque Title
            var pTitleObj = new GameObject("PlaqueTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            pTitleObj.transform.SetParent(plaquePopupObj.transform, false);
            var pTitleRt = pTitleObj.GetComponent<RectTransform>();
            pTitleRt.anchorMin = new Vector2(0.5f, 1f);
            pTitleRt.anchorMax = new Vector2(0.5f, 1f);
            pTitleRt.pivot = new Vector2(0.5f, 1f);
            pTitleRt.anchoredPosition = new Vector2(0f, -40f);
            pTitleRt.sizeDelta = new Vector2(500f, 45f);
            var pTitleTmp = pTitleObj.GetComponent<TextMeshProUGUI>();
            pTitleTmp.text = "...";
            pTitleTmp.alignment = TextAlignmentOptions.Center;
            pTitleTmp.fontSize = 32f;
            pTitleTmp.fontStyle = FontStyles.Bold;
            pTitleTmp.color = new Color(0.28f, 0.18f, 0.10f, 1f);

            // Plaque Body
            var pBodyObj = new GameObject("PlaqueBody", typeof(RectTransform), typeof(TextMeshProUGUI));
            pBodyObj.transform.SetParent(plaquePopupObj.transform, false);
            var pBodyRt = pBodyObj.GetComponent<RectTransform>();
            pBodyRt.anchorMin = new Vector2(0f, 0f);
            pBodyRt.anchorMax = new Vector2(1f, 1f);
            pBodyRt.offsetMin = new Vector2(90f, 40f);
            pBodyRt.offsetMax = new Vector2(-90f, -95f);
            var pBodyTmp = pBodyObj.GetComponent<TextMeshProUGUI>();
            pBodyTmp.text = "...";
            pBodyTmp.alignment = TextAlignmentOptions.Center;
            pBodyTmp.fontSize = 24f;
            pBodyTmp.color = new Color(0.24f, 0.16f, 0.10f, 1f);

            var closePlaqueBtnObj = CreatePlaqueCloseButton("ClosePlaqueButton", plaquePopupObj, plaqueSprite);
            var closePlaqueBtn = closePlaqueBtnObj.GetComponent<Button>();

            // 5. Artifact Silhouette Popup (Inspect View matching reference image)
            var glowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/SilhouetteGlow.png");

            var artPopupObj = new GameObject("ArtifactDetailPopup", typeof(RectTransform), typeof(Image), typeof(Button));
            artPopupObj.transform.SetParent(rootPanel.transform, false);
            var aPopupRt = artPopupObj.GetComponent<RectTransform>();
            aPopupRt.anchorMin = Vector2.zero;
            aPopupRt.anchorMax = Vector2.one;
            aPopupRt.offsetMin = Vector2.zero;
            aPopupRt.offsetMax = Vector2.zero;
            var aPopupImg = artPopupObj.GetComponent<Image>();
            aPopupImg.color = Color.clear;
            aPopupImg.raycastTarget = true;
            var closeArtBtn = artPopupObj.GetComponent<Button>();

            // 5b. Soft White Glow Halo
            var glowObj = new GameObject("GlowHalo", typeof(RectTransform), typeof(Image));
            glowObj.transform.SetParent(artPopupObj.transform, false);
            var glowRt = glowObj.GetComponent<RectTransform>();
            glowRt.anchorMin = new Vector2(0.5f, 0.5f);
            glowRt.anchorMax = new Vector2(0.5f, 0.5f);
            glowRt.pivot = new Vector2(0.5f, 0.5f);
            glowRt.anchoredPosition = new Vector2(0f, 110f);
            glowRt.sizeDelta = new Vector2(720f, 720f);
            var glowImg = glowObj.GetComponent<Image>();
            glowImg.sprite = glowSprite;
            glowImg.color = Color.white;
            glowImg.raycastTarget = false;

            // 5c. Artifact Silhouette Display Image
            var aDisplayObj = new GameObject("ArtifactDisplayImage", typeof(RectTransform), typeof(Image));
            aDisplayObj.transform.SetParent(artPopupObj.transform, false);
            var aDisplayRt = aDisplayObj.GetComponent<RectTransform>();
            aDisplayRt.anchorMin = new Vector2(0.5f, 0.5f);
            aDisplayRt.anchorMax = new Vector2(0.5f, 0.5f);
            aDisplayRt.pivot = new Vector2(0.5f, 0.5f);
            aDisplayRt.anchoredPosition = new Vector2(0f, 110f);
            aDisplayRt.sizeDelta = new Vector2(460f, 500f);
            var aDisplayImg = aDisplayObj.GetComponent<Image>();
            aDisplayImg.preserveAspect = true;
            aDisplayImg.color = Color.black;
            aDisplayImg.raycastTarget = false;

            // Sibling hierarchy order:
            // 0: Dimmer, 1: Silhouette Popup, 2: Plaque Popup, 3: Dialogue Box, 4: Choice Buttons
            bgDismissObj.transform.SetSiblingIndex(0);
            artPopupObj.transform.SetSiblingIndex(1);
            plaquePopupObj.transform.SetSiblingIndex(2);
            dboxObj.transform.SetSiblingIndex(3);
            choiceBox.transform.SetSiblingIndex(4);

            // 6. Attach ArtifactInspectionUI component to InGameCanvas
            var oldUi = rootPanel.GetComponent<ArtifactInspectionUI>();
            if (oldUi != null) Object.DestroyImmediate(oldUi);

            var uiCtrl = canvas.GetComponent<ArtifactInspectionUI>();
            if (uiCtrl == null) uiCtrl = canvas.AddComponent<ArtifactInspectionUI>();

            SetField(uiCtrl, "rootPanel", rootPanel);
            SetField(uiCtrl, "avatarImage", avPortImg);
            SetField(uiCtrl, "boyAvatarSprite", boyAvatar);
            SetField(uiCtrl, "girlAvatarSprite", girlAvatar);
            SetField(uiCtrl, "characterNameText", nameTmp);
            SetField(uiCtrl, "dialogueText", diagTmp);
            SetField(uiCtrl, "choiceContainer", choiceBox);
            SetField(uiCtrl, "restoreArtifactButton", btnRestoreObj.GetComponent<Button>());
            SetField(uiCtrl, "inspectArtifactButton", btnArtifactObj.GetComponent<Button>());
            SetField(uiCtrl, "inspectPlaqueButton", btnPlaqueObj.GetComponent<Button>());
            SetField(uiCtrl, "plaquePopup", plaquePopupObj);
            SetField(uiCtrl, "plaqueTitleText", pTitleTmp);
            SetField(uiCtrl, "plaqueBodyText", pBodyTmp);
            SetField(uiCtrl, "closePlaqueButton", closePlaqueBtn);
            SetField(uiCtrl, "artifactPopup", artPopupObj);
            SetField(uiCtrl, "artifactGlowImage", glowImg);
            SetField(uiCtrl, "artifactDetailImage", aDisplayImg);
            SetField(uiCtrl, "artifactDetailTitleText", null);
            SetField(uiCtrl, "artifactDetailBodyText", null);
            SetField(uiCtrl, "closeArtifactDetailButton", closeArtBtn);
            SetField(uiCtrl, "backgroundTapButton", null);
            SetField(uiCtrl, "dialogueArrowButton", arrowBtn);

            EditorUtility.SetDirty(uiCtrl);

            // Initially hide root panel
            rootPanel.SetActive(false);
        }

        private static GameObject CreateChoiceButton(string name, string text, Vector2 pos, GameObject parent, Sprite plaqueSprite)
        {
            var bObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            bObj.transform.SetParent(parent.transform, false);
            var rt = bObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(1f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(330f, 56f);

            var img = bObj.GetComponent<Image>();
            img.sprite = plaqueSprite;

            var btn = bObj.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.95f, 0.85f, 1f);
            colors.pressedColor = new Color(0.85f, 0.80f, 0.70f, 1f);
            colors.selectedColor = Color.white;
            btn.colors = colors;

            // Label (clean, centered on plaque banner)
            var lObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lObj.transform.SetParent(bObj.transform, false);
            var lRt = lObj.GetComponent<RectTransform>();
            lRt.anchorMin = Vector2.zero;
            lRt.anchorMax = Vector2.one;
            lRt.offsetMin = new Vector2(20f, 0f);
            lRt.offsetMax = new Vector2(-20f, 0f);
            var lTmp = lObj.GetComponent<TextMeshProUGUI>();
            lTmp.text = text;
            lTmp.alignment = TextAlignmentOptions.Center;
            lTmp.fontSize = 21f;
            lTmp.fontStyle = FontStyles.Bold;
            lTmp.color = new Color(0.18f, 0.12f, 0.08f, 1f);

            return bObj;
        }

        private static GameObject CreatePlaqueCloseButton(string name, GameObject parent, Sprite plaqueSprite)
        {
            var bObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            bObj.transform.SetParent(parent.transform, false);
            var rt = bObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-28f, -22f);
            rt.sizeDelta = new Vector2(80f, 36f);

            var img = bObj.GetComponent<Image>();
            img.sprite = plaqueSprite;

            var btn = bObj.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.94f, 0.85f, 1f);
            colors.pressedColor = new Color(0.85f, 0.78f, 0.65f, 1f);
            colors.selectedColor = Color.white;
            btn.colors = colors;

            var txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(bObj.transform, false);
            var txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;
            var tmp = txtObj.GetComponent<TextMeshProUGUI>();
            tmp.text = "Back";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 18f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(0.24f, 0.16f, 0.10f, 1f);

            return bObj;
        }

        private static void SetupTableArtifacts(string[] tablePaths, string[] names, Sprite[] sprites, string[] plaqueTexts, string[] inspectTexts)
        {
            string scName = EditorSceneManager.GetActiveScene().name;
            int rNum = 1;
            if (scName.StartsWith("Room_") && int.TryParse(scName.Substring(5), out int parsed))
            {
                rNum = parsed;
            }

            for (int i = 0; i < tablePaths.Length; i++)
            {
                var tableObj = GameObject.Find(tablePaths[i]);
                if (tableObj == null) continue;

                var interactable = tableObj.GetComponent<ArtifactInteractable>();
                if (interactable == null) interactable = tableObj.AddComponent<ArtifactInteractable>();

                interactable.Configure(
                    names[i],
                    sprites[i],
                    names[i],
                    plaqueTexts[i],
                    inspectTexts[i],
                    "what should i do",
                    "what could this artifact be",
                    "i wonder what this artifact looks like",
                    rNum
                );
                EditorUtility.SetDirty(interactable);

                // Add or configure trigger collider on the table for player interaction
                var colliders = tableObj.GetComponents<BoxCollider2D>();
                BoxCollider2D triggerCol = null;
                foreach (var c in colliders)
                {
                    if (c.isTrigger) { triggerCol = c; break; }
                }
                if (triggerCol == null)
                {
                    triggerCol = tableObj.AddComponent<BoxCollider2D>();
                }
                triggerCol.isTrigger = true;
                triggerCol.size = new Vector2(10.0f, 10.0f);
                triggerCol.offset = new Vector2(0f, -4.0f);
                EditorUtility.SetDirty(triggerCol);
            }
        }

        private static void SetupVisualArtifacts(string[] tablePaths, string[] artifactObjectNames, Sprite[] sprites)
        {
            var existingRoot = GameObject.Find("RoomArtifacts");
            if (existingRoot != null) Object.DestroyImmediate(existingRoot);

            var root = new GameObject("RoomArtifacts");
            root.transform.position = Vector3.zero;

            var unlitMat = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");

            for (int i = 0; i < tablePaths.Length; i++)
            {
                var tableObj = GameObject.Find(tablePaths[i]);
                if (tableObj == null) continue;

                var tableSr = tableObj.GetComponent<SpriteRenderer>();

                var artObj = new GameObject(artifactObjectNames[i], typeof(SpriteRenderer), typeof(TableItemSort));
                artObj.transform.SetParent(root.transform, false);
                artObj.transform.position = new Vector3(tableObj.transform.position.x, tableObj.transform.position.y + 0.51f, 0f);
                artObj.transform.localScale = new Vector3(0.13f, 0.13f, 1f);

                var sr = artObj.GetComponent<SpriteRenderer>();
                sr.sprite = sprites[i];
                sr.color = Color.black; // Solid black silhouette indicating the artifact is missing
                if (unlitMat != null) sr.sharedMaterial = unlitMat;
                sr.sortingLayerName = "Default";
                sr.sortingOrder = 3;
                EditorUtility.SetDirty(sr);

                var tis = artObj.GetComponent<TableItemSort>();
                if (tis != null && tableSr != null)
                {
                    tis.SetParentTable(tableSr);
                    EditorUtility.SetDirty(tis);
                }
                EditorUtility.SetDirty(artObj);
            }

            EditorUtility.SetDirty(root);
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

        private static Sprite LoadArtifactSprite(string path)
        {
            var spr = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (spr != null) return spr;

            var all = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in all)
            {
                if (a is Sprite s) return s;
            }
            return null;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}
