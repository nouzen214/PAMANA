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

            var sprStamp   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Stamp.png");
            var sprBook    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Book.png");
            var sprPen     = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_PenNib.png");
            var sprGlasses = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Glasses.png");
            var sprLocket  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Locket.png");

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
                "Ancient Seal of Tondo",
                "Sacred Doctrina Manuscript",
                "Illustrado Quill Nib",
                "Wire-rimmed Spectacles",
                "Gold Locket Pendant"
            };
            Sprite[] sprites = new Sprite[] { sprStamp, sprBook, sprPen, sprGlasses, sprLocket };
            string[] plaqueTexts = new string[]
            {
                "An ancient seal used for marking official royal decrees and trade agreements in pre-colonial Luzon.",
                "An ancient manuscript written in early Baybayin script containing celestial star maps and island chronicles.",
                "A golden calligraphy nib preserved from the 19th-century Philippine reform and literary movement.",
                "Delicate round wire-rimmed spectacles used by Filipino scholars and reformists while studying classical literature and writing essays.",
                "A twin-oval keepsake pendant carried by 19th-century travelers to preserve miniature portraits and keepsakes of loved ones."
            };
            string[] inspectTexts = new string[]
            {
                "A solid bronze seal bearing intricate royal crests. The base is worn smooth from stamping wax agreements.",
                "Bound in aged parchment with delicate gold leaf. The ancient calligraphy is astonishingly intact.",
                "Fine tempered gold alloy with a split nib tip, capable of writing flourished cursive manuscripts.",
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
                "Annotated proof sheets from the original print run of Noli Me Tángere, printed in Berlin through the generosity of Dr. Maximo Viola.",
                "A preserved writing quill pen used to author the emotional and patriotic chapters of José Rizal's novels.",
                "A historical wooden printing press bed used in Ghent to set and print the initial edition of El Filibusterismo in 1891.",
                "An original preserved first edition volume of El Filibusterismo, rescued and printed through Valentin Ventura's urgent financial support.",
                "The historic brass alcohol cooking stove from Fort Santiago inside which José Rizal concealed the manuscript of Mi Último Adiós."
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
                "A historic tribute folio commemorating Fathers Gomez, Burgos, and Zamora, whose 1872 execution ignited the Philippine revolution.",
                "An original serialized essay by José Rizal analyzing the historical decline and predicting the eventual liberation of the Philippines.",
                "José Rizal's celebrated 1889 letter written to praise and empower the young women of Malolos for courageously pursuing education.",
                "The premier reformist newspaper published in Spain advocating civil rights, freedom of speech, and Filipino parliamentary representation.",
                "The historic first bilingual newspaper published in Tagalog and Spanish, co-founded by Marcelo H. del Pilar in 1882."
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
                "An official induction certificate defining the civic duties, mutual defense, and moral code of La Liga Filipina members.",
                "A Spanish vice-royal decree ordering the immediate arrest and four-year banishment of José Rizal to Dapitan.",
                "A clandestine registry containing the pseudonyms and records of early reformists, including Katipunan founder Andres Bonifacio.",
                "A scroll charter signed during the 1893 restructuring of the Supreme Council under secretary Apolinario Mabini.",
                "A historical manifesto penned when La Liga fractured into the moderate Cuerpo de Compromisarios and the revolutionary Katipunan."
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
                "Curved ophthalmic surgical forceps used by Dr. José Rizal to successfully remove cataracts for patients in Dapitan.",
                "A student attendance and grade ledger from Rizal's tuition-free community school established in Talisay, Dapitan.",
                "A sealed glass specimen jar preserving biological specimens discovered by Rizal, including the Apogonia rizali beetle.",
                "A spherical municipal streetlamp introduced by Rizal using native coconut oil to illuminate the streets of Dapitan at night.",
                "An official military doctor application submitted by Rizal to volunteer as an army surgeon in Cuba."
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
                "A gold-tipped baton carried by colonial governors as the emblem of supreme vice-royal executive authority.",
                "A ceremonial hardwood gavel and striking block used in the highest appellate court in colonial Manila.",
                "A dual-pan balance scale used by colonial administrators to weigh and enforce compulsory agricultural quotas on native farmers.",
                "A thick, leather-bound parish codex containing civil records and symbolizing the friars' profound political dominance.",
                "Spanish colonial silver columnar coins dispatched annually from the Viceroyalty of New Spain to subsidize the archipelago."
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

            var sprStoneTablet       = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_StoneTablet.png");
            var sprEscritorio        = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Escritorio.png");
            var sprCopperplate       = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Copperplate.png");
            var sprDispatchPortfolio = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_DispatchPortfolio.png");
            var sprChronicleFolio    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_ChronicleFolio.png");

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
                "Artifact_StoneTablet",
                "Artifact_Escritorio",
                "Artifact_Copperplate",
                "Artifact_DispatchPortfolio",
                "Artifact_ChronicleFolio"
            };
            string[] names = new string[]
            {
                "Monreal Baybayin Stone",
                "Katipunan Field Escritorio",
                "Laguna Copperplate Inscription",
                "Courier's Dispatch Portfolio",
                "Genealogical Chronicle Folio"
            };
            Sprite[] sprites = new Sprite[] { sprStoneTablet, sprEscritorio, sprCopperplate, sprDispatchPortfolio, sprChronicleFolio };
            string[] plaqueTexts = new string[]
            {
                "A preserved limestone tablet carved with authentic pre-colonial Baybayin characters, discovered in the central Philippine islands.",
                "A portable slant-top writing desk and secret dispatch box carried by revolutionary couriers across archipelago hideouts.",
                "A square hammered copperplate bearing the earliest known written legal document found in the Philippine islands, dated 900 CE.",
                "A reinforced leather courier portfolio fitted with an iron clasp, used to secretly transport uncensored revolutionary correspondence between provinces.",
                "An opened manuscript chronicle detailing pre-colonial baranganic lineages, royal trade manifests, and customary laws across ancient Luzon."
            };
            string[] inspectTexts = new string[]
            {
                "Hardened coralline limestone inscribed with deliberate chiseling. The ancient syllabary records genealogical lineages and village pacts.",
                "Carved from narra timber with brass hinges and a felt writing slant. Secret compartments beneath the base once concealed coded manifestos.",
                "Thin copper hammered flat and etched with intricate Kawi script. The text releases a family and their descendants from historic bonded debts.",
                "Stiffened saddle leather treated against seawater and rain. The interior reveals hidden sleeves sized for cipher keys and dispatch letters.",
                "Hand-laid mulberry paper displayed across parallel columns. Baybayin annotations sit alongside Spanish translations by early friar chroniclers."
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

            var sprSticks       = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_ArnisSticks.png");
            var sprDagger       = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Dagger.png");
            var sprRifle        = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Rifle.png");
            var sprBrokenTablet = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_BrokenTablet.png");
            var sprWhetstone    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Whetstone.png");

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
                "Artifact_ArnisSticks",
                "Artifact_Dagger",
                "Artifact_Rifle",
                "Artifact_BrokenTablet",
                "Artifact_Whetstone"
            };
            string[] names = new string[]
            {
                "Crossed Kamagong Baston",
                "Katipunero Combat Dagger",
                "Revolutionary Spanish Mauser Rifle",
                "Fractured Katipunan Oath Tablet",
                "Armorer's Honing Whetstone"
            };
            Sprite[] sprites = new Sprite[] { sprSticks, sprDagger, sprRifle, sprBrokenTablet, sprWhetstone };
            string[] plaqueTexts = new string[]
            {
                "A pair of seasoned Kamagong hardwood fighting sticks used in native Filipino martial arts (Arnis / Eskrima / Kali).",
                "A handcrafted double-edged dagger with an ergonomic grip, carried by Katipunan fighters during nighttime skirmishes.",
                "A captured bolt-action service rifle utilized by Filipino revolutionaries during the historic campaigns of 1896 to 1898.",
                "A twin-piece inscribed stone tablet once split between revolutionary lodge masters, brought together to verify authentic Katipunan assemblies.",
                "A fine-grit river whetstone used by revolutionary blacksmiths (pandays) and freedom fighters to sharpen combat bolos, daggers, and bayonets."
            };
            string[] inspectTexts = new string[]
            {
                "Dense ironwood hardened by fire and oil treatment. Notches along the grips indicate rigorous close-quarters combat training.",
                "Hand-forged carbon steel with a curved spine and brass crossguard. The pommel bears faint initials etched into the darkened wood.",
                "Toughened walnut stock with a long blued steel barrel and ladder sights. Scorch marks near the breech reflect intense battle engagements.",
                "Dense dark slate bearing matching jagged break lines. When aligned, the incised Baybayin code reveals the secret password of the Katipunan council.",
                "Heavy sedimentary stone with concave wear groves along the top surface, polished smooth by countless blades prepared on the eve of battle."
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

            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_InabelCloth.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_Bulul.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_Kolitong.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_DuyoBowl.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_Tongali.png");

            var sprCloth = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_InabelCloth.png");
            var sprBulul = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_Bulul.png");
            var sprKolit = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_Kolitong.png");
            var sprBowl  = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_DuyoBowl.png");
            var sprFlute = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_Tongali.png");

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
                "Artifact_InabelCloth",
                "Artifact_Bulul",
                "Artifact_Kolitong",
                "Artifact_DuyoBowl",
                "Artifact_Tongali"
            };
            string[] names = new string[]
            {
                "Traditional Inabel Woven Textile",
                "Sacred Ifugao Bulul (Rice Guardian)",
                "Kalinga Bamboo Tube Zither (Kolitong)",
                "Ifugao Carved Wooden Duyo (Feast Bowl)",
                "Kalinga Bamboo Nose Flute (Tongali)"
            };
            Sprite[] sprites = new Sprite[] { sprCloth, sprBulul, sprKolit, sprBowl, sprFlute };
            string[] plaqueTexts = new string[]
            {
                "A finely loomed traditional woven textile crafted using heritage indigenous backstrap looms, featuring sacred geometric patterns from northern Luzon.",
                "A revered carved wooden anthropomorphic deity sculpture representing ancestral spirits, traditionally placed inside Ifugao granaries to protect the rice harvest and ensure communal abundance.",
                "A traditional polychordal bamboo tube zither featuring strings etched directly from the bamboo's outer skin, supported by wooden bridges to accompany sacred epic chants and celebrations.",
                "A traditional ceremonial feast bowl carved from a single piece of native Philippine hardwood, used during ancient Cañao gatherings and sacred harvest ceremonies in the Cordilleras.",
                "A slender indigenous nose flute fashioned from mature mountain bamboo, played by blowing through a single nostril to produce gentle, haunting melodies during courtship and peaceful gatherings."
            };
            string[] inspectTexts = new string[]
            {
                "Tightly spun natural cotton threads with fringed tassels along the hem. The interlocking geometric motifs symbolize rice paddies, ancestral waves, and protective clan seals.",
                "Carved in a seated posture with arms reverently crossed over folded knees. A sacred dark patina from ritual sacrificial oils coats the kamagong hardwood figure.",
                "Hollow whole bamboo tube fitted with parallel strings raised on carved wooden fret bridges. When plucked, the resonant bamboo chamber produces rich, melodic percussive harmonies.",
                "Hand-hewn from solid narra heartwood with a flared circular rim. The dark polished patina reflects centuries of ceremonial offerings and rice wine libations.",
                "Carefully calibrated node holes drilled into seasoned mountain bamboo. The mouthpiece is angled smoothly, retaining natural lacquer from decades of ritual playing."
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

            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_Sarimanok.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_Anvil.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_HornAmulet.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_TobaccoPipe.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_BakunawaDragon.png");

            var sprSarimanok  = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_Sarimanok.png");
            var sprAnvil      = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_Anvil.png");
            var sprHornAmulet = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_HornAmulet.png");
            var sprPipe       = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_TobaccoPipe.png");
            var sprDragon     = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_BakunawaDragon.png");

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
                "Artifact_Sarimanok",
                "Artifact_Anvil",
                "Artifact_HornAmulet",
                "Artifact_TobaccoPipe",
                "Artifact_BakunawaDragon"
            };
            string[] names  = new string[]
            {
                "Bronze Sarimanok Figurine",
                "Panday's Forging Anvil",
                "Carabao Horn War Budyong",
                "Traditional Ancestral Kuwako (Smoking Pipe)",
                "Mythical Bakunawa Dragon Figurine"
            };
            Sprite[] sprites = new Sprite[] { sprSarimanok, sprAnvil, sprHornAmulet, sprPipe, sprDragon };
            string[] plaqueTexts = new string[]
            {
                "A sacred Maranao brass sculpture representing the mythical Sarimanok, herald of good fortune and celestial messenger.",
                "A heavy iron forging anvil used by colonial and revolutionary blacksmiths (panday) to craft bladed weapons and farm tools.",
                "A traditional acoustic horn fashioned from water buffalo horn, blown by native warriors and sentries to rally village defenders.",
                "A handcrafted smoking pipe fashioned from dense indigenous hardwood and brass, historically smoked by tribal elders and community leaders during council meetings and peaceful negotiations.",
                "A magnificent serpentine dragon sculpture depicting the Bakunawa, the legendary Philippine dragon believed to swallow the moon during lunar eclipses."
            };
            string[] inspectTexts = new string[]
            {
                "Cast in solid brass with intricate okir wing motifs and a circular base. Intricate feather scrollwork adorns the crest.",
                "Forged cast iron with a wide striking face and flared pedestal. Repeated hammer strike marks score the blackened crown.",
                "Polished dark carabao horn fitted with woven cordage and talismanic charms. The mouthpiece exhibits natural patination from ceremonial use.",
                "Smooth polished hardwood bowl fitted with a curved stem and reinforced brass ferrule. Faint aromatic resin and charred tobacco residue line the interior bowl.",
                "Intricately cast with undulating serpentine coils, prominent dorsal fins, and a crested draconic head. Subtle okir relief patterns adorn the circular mounting plinth."
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

            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_ClayJarlet.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_ArchaeologyTrowel.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_ManunggulJar.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_BoneTool.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_PaoayChurch.png");

            var sprJarlet  = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_ClayJarlet.png");
            var sprTrowel  = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_ArchaeologyTrowel.png");
            var sprBurial  = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_ManunggulJar.png");
            var sprBone    = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_BoneTool.png");
            var sprChurch  = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_PaoayChurch.png");

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
                "Artifact_ClayJarlet",
                "Artifact_ArchaeologyTrowel",
                "Artifact_ManunggulJar",
                "Artifact_BoneTool",
                "Artifact_PaoayChurch"
            };
            string[] names  = new string[]
            {
                "Pre-Colonial Ritual Jarlet",
                "Excavator's Pointing Trowel",
                "The Manunggul Burial Jar",
                "Prehistoric Polished Bone Tool",
                "Architectural Model of Paoay Church"
            };
            Sprite[] sprites = new Sprite[] { sprJarlet, sprTrowel, sprBurial, sprBone, sprChurch };
            string[] plaqueTexts = new string[]
            {
                "A low-fired earthenware ritual jarlet unearthed in Batangas, featuring an ornamental knobbed lid and suspension lug used for sacred offerings.",
                "A precision steel pointing trowel used by field archaeologists during landmark excavation campaigns across Philippine cave and coastal sites.",
                "A supreme masterpiece of ancient Philippine pottery from Palawan (890–710 BCE), featuring two souls on a spirit boat journeying to the afterlife.",
                "A smoothed and polished ancient bone implement recovered from prehistoric cave dig sites, used by ancient islanders for crafting, leatherworking, and fine carving.",
                "A detailed miniature replica of the historic Paoay Church (Saint Augustine Church) in Ilocos Norte, a UNESCO World Heritage Site renowned for its colossal earthquake baroque buttresses and detached coral bell tower."
            };
            string[] inspectTexts = new string[]
            {
                "Thick earthenware clay burnished with red slip. The domed lid fits snugly over a stepped neck, and remnants of aromatic resin cling to the interior rim.",
                "Tempered carbon steel blade with a beveled scraping edge and a polished hardwood handle. Clay stains from deep stratigraphic layers coat the heel.",
                "Delicately carved secondary burial jar with incised curvilinear wave scrolls painted in natural red hematite. The boatman steers with an oar while the soul sits forward with arms reverently crossed.",
                "A straight, dense fossilized bone shaft polished to a resilient luster. Microscopic wear striations along the rounded tips indicate intensive prehistoric crafting use.",
                "Hand-carved replica reproducing the 24 massive contoured buttresses and the iconic multi-tiered coral stone bell tower. The architectural masterpiece stands as an enduring monument to Filipino-Spanish colonial engineering."
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

            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_OfficerRevolver.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_CeremonialBowl.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_KatipunanBolo.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_OfficerSaber.png");
            EnsureSpriteImport("Assets/Sprites/Artifacts/Artifact_CavalrySaddle.png");

            var sprRevolver = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_OfficerRevolver.png");
            var sprBowl     = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_CeremonialBowl.png");
            var sprBolo     = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_KatipunanBolo.png");
            var sprSaber    = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_OfficerSaber.png");
            var sprSaddle   = LoadArtifactSprite("Assets/Sprites/Artifacts/Artifact_CavalrySaddle.png");

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
                "Artifact_OfficerRevolver",
                "Artifact_CeremonialBowl",
                "Artifact_KatipunanBolo",
                "Artifact_OfficerSaber",
                "Artifact_CavalrySaddle"
            };
            string[] names  = new string[]
            {
                "Katipunan Officer's Revolver",
                "Katipunan Sandugo Bowl",
                "Katipunero Combat Bolo",
                "Revolutionary Officer's Cavalry Saber",
                "Revolutionary Cavalry Saddle"
            };
            Sprite[] sprites = new Sprite[] { sprRevolver, sprBowl, sprBolo, sprSaber, sprSaddle };
            string[] plaqueTexts = new string[]
            {
                "A six-shot service revolver carried by high-ranking revolutionary officers during pivotal battles of the Philippine Revolution.",
                "A ceremonial brass bowl used during clandestine Katipunan initiation ceremonies to perform the solemn blood compact (Sandugo).",
                "A formidable single-edged combat bolo forged by native pandays, wielded by Katipunan freedom fighters in close-quarters liberation battles.",
                "A curved steel cavalry saber with an ornate brass knuckle-guard hilt, carried by high-ranking revolutionary officers and cavalry generals during the revolutionary campaigns.",
                "A handcrafted leather cavalry saddle equipped with iron stirrups, utilized by Filipino revolutionary cavalry units and officers leading mobile reconnaissance and battlefield charges."
            };
            string[] inspectTexts = new string[]
            {
                "Blued steel frame fitted with polished walnut grips. The cylinder rotates smoothly, and faint patriotic insigne are stamped into the sideplate.",
                "Hammered from heavy solid brass with a flared lip. The polished interior retains subtle patina marks from ceremonial quill dips and wax seals.",
                "Hand-forged tempered leaf spring steel with an aggressive clip-point tip and carved carabao horn handle. Battle notches along the spine testify to historical combat.",
                "Tempered carbon steel blade etched with fine decorative scrollwork near the ricasso. The brass three-bar guard protects a ribbed leather grip bound with twisted brass wire.",
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
            avPortRt.anchoredPosition = new Vector2(5f, 5f);
            avPortRt.sizeDelta = new Vector2(210f, 210f);
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

            var btnRestoreObj  = CreateChoiceButton("RestoreArtifactButton", "place from bag", Vector2.zero, choiceBox, plaqueSprite);
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

            // Badge circle
            var badgeObj = new GameObject("Badge", typeof(RectTransform), typeof(Image));
            badgeObj.transform.SetParent(bObj.transform, false);
            var badgeRt = badgeObj.GetComponent<RectTransform>();
            badgeRt.anchorMin = new Vector2(0f, 0.5f);
            badgeRt.anchorMax = new Vector2(0f, 0.5f);
            badgeRt.pivot = new Vector2(0.5f, 0.5f);
            badgeRt.anchoredPosition = new Vector2(36f, 0f);
            badgeRt.sizeDelta = new Vector2(38f, 38f);
            var badgeImg = badgeObj.GetComponent<Image>();
            var circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/CircleBadge.png");
            if (circleSprite != null)
            {
                badgeImg.sprite = circleSprite;
                badgeImg.color = Color.white;
            }
            else
            {
                badgeImg.color = new Color(0.68f, 0.15f, 0.95f, 1f);
            }

            var btn = bObj.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.95f, 0.85f, 1f);
            colors.pressedColor = new Color(0.85f, 0.80f, 0.70f, 1f);
            colors.selectedColor = Color.white;
            btn.colors = colors;

            var dLetterObj = new GameObject("Letter", typeof(RectTransform), typeof(TextMeshProUGUI));
            dLetterObj.transform.SetParent(badgeObj.transform, false);
            var dRt = dLetterObj.GetComponent<RectTransform>();
            dRt.anchorMin = Vector2.zero;
            dRt.anchorMax = Vector2.one;
            dRt.offsetMin = Vector2.zero;
            dRt.offsetMax = Vector2.zero;
            var dTmp = dLetterObj.GetComponent<TextMeshProUGUI>();
            dTmp.text = "D";
            dTmp.alignment = TextAlignmentOptions.Center;
            dTmp.fontSize = 20f;
            dTmp.fontStyle = FontStyles.Bold;
            dTmp.color = Color.white;

            // Label
            var lObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lObj.transform.SetParent(bObj.transform, false);
            var lRt = lObj.GetComponent<RectTransform>();
            lRt.anchorMin = new Vector2(0f, 0f);
            lRt.anchorMax = new Vector2(1f, 1f);
            lRt.offsetMin = new Vector2(62f, 0f);
            lRt.offsetMax = new Vector2(-15f, 0f);
            var lTmp = lObj.GetComponent<TextMeshProUGUI>();
            lTmp.text = text;
            lTmp.alignment = TextAlignmentOptions.Left;
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
