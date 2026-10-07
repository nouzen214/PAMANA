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

            var sprStamp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Stamp.png");
            var sprBook  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Book.png");
            var sprPen   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_PenNib.png");

            string[] tables = new string[] { "Tables/Table_TopLeft", "Tables/Table_Middle", "Tables/Table_TopRight" };
            string[] artifactObjectNames = new string[] { "Artifact_Stamp", "Artifact_Book", "Artifact_PenNib" };
            string[] names  = new string[] { "Ancient Seal of Tondo", "Sacred Doctrina Manuscript", "Illustrado Quill Nib" };
            Sprite[] sprites = new Sprite[] { sprStamp, sprBook, sprPen };
            string[] plaqueTexts = new string[]
            {
                "An ancient seal used for marking official royal decrees and trade agreements in pre-colonial Luzon.",
                "An ancient manuscript written in early Baybayin script containing celestial star maps and island chronicles.",
                "A golden calligraphy nib preserved from the 19th-century Philippine reform and literary movement."
            };
            string[] inspectTexts = new string[]
            {
                "A solid bronze seal bearing intricate royal crests. The base is worn smooth from stamping wax agreements.",
                "Bound in aged parchment with delicate gold leaf. The ancient calligraphy is astonishingly intact.",
                "Fine tempered gold alloy with a split nib tip, capable of writing flourished cursive manuscripts."
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

            var sprPot     = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Pot.png");
            var sprJournal = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Journal.png");
            var sprFeather = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Feather.png");

            string[] tables = new string[] { "Tables/Table_TopLeft", "Tables/Table_Middle", "Tables/Table_TopRight" };
            string[] artifactObjectNames = new string[] { "Artifact_Pot", "Artifact_Journal", "Artifact_Feather" };
            string[] names  = new string[] { "Ceremonial Bronze Chafing Pot", "Propagandist's Secret Journal", "Patriot's Feather Quill" };
            Sprite[] sprites = new Sprite[] { sprPot, sprJournal, sprFeather };
            string[] plaqueTexts = new string[]
            {
                "A rare pre-colonial bronze vessel and burner used for royal banquets and ceremonial offerings in ancient Luzon.",
                "A leather-bound journal containing coded entries, clandestine articles, and records from the Philippine Propaganda Movement.",
                "A preserved goose feather quill pen used by 19th-century Filipino reformists to compose influential patriotic writings."
            };
            string[] inspectTexts = new string[]
            {
                "Crafted with sturdy side handles and mounted on a heated tripod brazier. Soot and copper patina line the underside.",
                "Bound in weathered brown leather with tight cross-stitching. The pages are dense with covert revolutionary ciphers.",
                "A long, graceful feather trimmed to a fine writing point. Traces of sepia gall ink linger near the nib."
            };

            SetupTableArtifacts(tables, names, sprites, plaqueTexts, inspectTexts);
            SetupVisualArtifacts(tables, artifactObjectNames, sprites);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[ArtifactUISetup] Successfully built Artifact Inspection UI and Artifacts in Room 2!");
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

            var sprDoc       = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Document.png");
            var sprInkwell   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Inkwell.png");
            var sprParchment = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Parchment.png");

            string[] tables = new string[] { "Tables/Table_TopLeft", "Tables/Table_Middle", "Tables/Table_TopRight" };
            string[] artifactObjectNames = new string[] { "Artifact_Document", "Artifact_Inkwell", "Artifact_Parchment" };
            string[] names  = new string[] { "Revolutionary Draft Decree", "Scholar's Brass Inkwell", "Declaration of Sovereignty" };
            Sprite[] sprites = new Sprite[] { sprDoc, sprInkwell, sprParchment };
            string[] plaqueTexts = new string[]
            {
                "A handwritten draft of a historical decree laying out principles of sovereignty and freedom during the revolutionary era.",
                "A weighted traveling inkpot used by 19th-century Filipino intellectuals to hold indelible iron gall ink during secret correspondence.",
                "A ceremonial parchment proclamation drafted to herald national autonomy and unity among the archipelago's provinces."
            };
            string[] inspectTexts = new string[]
            {
                "Authored in clear, deliberate strokes with an ink-tipped feather resting across the margin. The ink remains remarkably dark and legible.",
                "Crafted with an angled reservoir and pen rest. Hardened black residue from centuries-old ink still coats the bottom.",
                "Heavy vellum inscribed with elaborate flourishes and adorned with official corner ribbons. Wax seals validate its historic authority."
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

            var sprFlask    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Flask.png");
            var sprJar      = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Jar.png");
            var sprScissors = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Scissors.png");

            string[] tables = new string[] { "Tables/Table_TopLeft", "Tables/Table_Middle", "Tables/Table_TopRight" };
            string[] artifactObjectNames = new string[] { "Artifact_Flask", "Artifact_Jar", "Artifact_Scissors" };
            string[] names  = new string[] { "Apothecary's Spherical Flask", "Vintage Apothecary Jar", "Rizal's Surgical Forceps" };
            Sprite[] sprites = new Sprite[] { sprFlask, sprJar, sprScissors };
            string[] plaqueTexts = new string[]
            {
                "A thick-walled glass spherical flask used by 19th-century medical practitioners to distill antiseptics and essential herbal tinctures.",
                "A glazed ceramic apothecary jar designed to preserve healing balms, poultices, and powdered botanical remedies.",
                "Curved stainless surgical forceps preserved from historical Philippine medical clinics and ophthalmic procedures."
            };
            string[] inspectTexts = new string[]
            {
                "Blown from heavy tinted glass with an integrated hanging loop. Dried amber herbal residue remains along the internal curvature.",
                "Finished with a fitted airtight lid and wide base. Faint apothecary labels and measuring gradations are visible along the ceramic rim.",
                "Finely balanced surgical steel with ribbed finger loops and a precision serrated curved jaw, engineered for delicate field operations."
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

            var sprGavel      = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Gavel.png");
            var sprLawTome    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_LawTome.png");
            var sprSpectacles = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Spectacles.png");

            string[] tables = new string[] { "Tables/Table_TopLeft", "Tables/Table_Middle", "Tables/Table_TopRight" };
            string[] artifactObjectNames = new string[] { "Artifact_Gavel", "Artifact_LawTome", "Artifact_Spectacles" };
            string[] names  = new string[] { "Magistrate's Hardwood Gavel", "Codex of Philippine Jurisprudence", "Jurist's Reading Spectacles" };
            Sprite[] sprites = new Sprite[] { sprGavel, sprLawTome, sprSpectacles };
            string[] plaqueTexts = new string[]
            {
                "A ceremonial gavel and striking block carved from native molave hardwood, used in the early judicial courts of the Philippine archipelago.",
                "A monumental bound compendium containing early civil statutes, decrees, and constitutional drafts from the First Philippine Republic.",
                "A pair of antique round reading spectacles once belonging to a prominent 19th-century Filipino jurist and scholar."
            };
            string[] inspectTexts = new string[]
            {
                "Dense dark molave wood with an ergonomic turned handle. Indentations on the sounding block bear witness to historic verdicts rendered.",
                "Bound in embossed dark cowhide with gilt-edged leaves. Heavily annotated margins reflect the constitutional debates of early Filipino jurists.",
                "Tightly curved nose bridge and polished circular lenses. Faint scratches on the dark glass testify to countless hours spent examining historic legal briefs."
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

            var sprStoneTablet  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_StoneTablet.png");
            var sprEscritorio   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Escritorio.png");
            var sprCopperplate  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Copperplate.png");

            string[] tables = new string[] { "Tables/Table_TopLeft", "Tables/Table_Middle", "Tables/Table_TopRight" };
            string[] artifactObjectNames = new string[] { "Artifact_StoneTablet", "Artifact_Escritorio", "Artifact_Copperplate" };
            string[] names  = new string[] { "Monreal Baybayin Stone", "Katipunan Field Escritorio", "Laguna Copperplate Inscription" };
            Sprite[] sprites = new Sprite[] { sprStoneTablet, sprEscritorio, sprCopperplate };
            string[] plaqueTexts = new string[]
            {
                "A preserved limestone tablet carved with authentic pre-colonial Baybayin characters, discovered in the central Philippine islands.",
                "A portable slant-top writing desk and secret dispatch box carried by revolutionary couriers across archipelago hideouts.",
                "A square hammered copperplate bearing the earliest known written legal document found in the Philippine islands, dated 900 CE."
            };
            string[] inspectTexts = new string[]
            {
                "Hardened coralline limestone inscribed with deliberate chiseling. The ancient syllabary records genealogical lineages and village pacts.",
                "Carved from narra timber with brass hinges and a felt writing slant. Secret compartments beneath the base once concealed coded manifestos.",
                "Thin copper hammered flat and etched with intricate Kawi script. The text releases a family and their descendants from historic bonded debts."
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

            var sprSticks  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_ArnisSticks.png");
            var sprDagger  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Dagger.png");
            var sprRifle   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Rifle.png");

            string[] tables = new string[] { "Tables/Table_TopLeft", "Tables/Table_Middle", "Tables/Table_TopRight" };
            string[] artifactObjectNames = new string[] { "Artifact_ArnisSticks", "Artifact_Dagger", "Artifact_Rifle" };
            string[] names  = new string[] { "Crossed Kamagong Baston", "Katipunero Combat Dagger", "Revolutionary Spanish Mauser Rifle" };
            Sprite[] sprites = new Sprite[] { sprSticks, sprDagger, sprRifle };
            string[] plaqueTexts = new string[]
            {
                "A pair of seasoned Kamagong hardwood fighting sticks used in native Filipino martial arts (Arnis / Eskrima / Kali).",
                "A handcrafted double-edged dagger with an ergonomic grip, carried by Katipunan fighters during nighttime skirmishes.",
                "A captured bolt-action service rifle utilized by Filipino revolutionaries during the historic campaigns of 1896 to 1898."
            };
            string[] inspectTexts = new string[]
            {
                "Dense ironwood hardened by fire and oil treatment. Notches along the grips indicate rigorous close-quarters combat training.",
                "Hand-forged carbon steel with a curved spine and brass crossguard. The pommel bears faint initials etched into the darkened wood.",
                "Toughened walnut stock with a long blued steel barrel and ladder sights. Scorch marks near the breech reflect intense battle engagements."
            };

            SetupTableArtifacts(tables, names, sprites, plaqueTexts, inspectTexts);
            SetupVisualArtifacts(tables, artifactObjectNames, sprites);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[ArtifactUISetup] Successfully built Artifact Inspection UI and Artifacts in Room 8!");
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

            var sprSarimanok  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Sarimanok.png");
            var sprAnvil      = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Anvil.png");
            var sprHornAmulet = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_HornAmulet.png");

            string[] tables = new string[] { "Tables/Table_TopLeft", "Tables/Table_Middle", "Tables/Table_TopRight" };
            string[] artifactObjectNames = new string[] { "Artifact_Sarimanok", "Artifact_Anvil", "Artifact_HornAmulet" };
            string[] names  = new string[] { "Bronze Sarimanok Figurine", "Panday's Forging Anvil", "Carabao Horn War Budyong" };
            Sprite[] sprites = new Sprite[] { sprSarimanok, sprAnvil, sprHornAmulet };
            string[] plaqueTexts = new string[]
            {
                "A sacred Maranao brass sculpture representing the mythical Sarimanok, herald of good fortune and celestial messenger.",
                "A heavy iron forging anvil used by colonial and revolutionary blacksmiths (panday) to craft bladed weapons and farm tools.",
                "A traditional acoustic horn fashioned from water buffalo horn, blown by native warriors and sentries to rally village defenders."
            };
            string[] inspectTexts = new string[]
            {
                "Cast in solid brass with intricate okir wing motifs and a circular base. Intricate feather scrollwork adorns the crest.",
                "Forged cast iron with a wide striking face and flared pedestal. Repeated hammer strike marks score the blackened crown.",
                "Polished dark carabao horn fitted with woven cordage and talismanic charms. The mouthpiece exhibits natural patination from ceremonial use."
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

            var sprJarlet  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_ClayJarlet.png");
            var sprTrowel  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_ArchaeologyTrowel.png");
            var sprBurial  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_ManunggulJar.png");

            string[] tables = new string[] { "Tables/Table_TopLeft", "Tables/Table_Middle", "Tables/Table_TopRight" };
            string[] artifactObjectNames = new string[] { "Artifact_ClayJarlet", "Artifact_ArchaeologyTrowel", "Artifact_ManunggulJar" };
            string[] names  = new string[] { "Pre-Colonial Ritual Jarlet", "Excavator's Pointing Trowel", "The Manunggul Burial Jar" };
            Sprite[] sprites = new Sprite[] { sprJarlet, sprTrowel, sprBurial };
            string[] plaqueTexts = new string[]
            {
                "A low-fired earthenware ritual jarlet unearthed in Batangas, featuring an ornamental knobbed lid and suspension lug used for sacred offerings.",
                "A precision steel pointing trowel used by field archaeologists during landmark excavation campaigns across Philippine cave and coastal sites.",
                "A supreme masterpiece of ancient Philippine pottery from Palawan (890–710 BCE), featuring two souls on a spirit boat journeying to the afterlife."
            };
            string[] inspectTexts = new string[]
            {
                "Thick earthenware clay burnished with red slip. The domed lid fits snugly over a stepped neck, and remnants of aromatic resin cling to the interior rim.",
                "Tempered carbon steel blade with a beveled scraping edge and a polished hardwood handle. Clay stains from deep stratigraphic layers coat the heel.",
                "Delicately carved secondary burial jar with incised curvilinear wave scrolls painted in natural red hematite. The boatman steers with an oar while the soul sits forward with arms reverently crossed."
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

            var sprRevolver = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_OfficerRevolver.png");
            var sprBowl     = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_CeremonialBowl.png");
            var sprBolo     = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_KatipunanBolo.png");

            string[] tables = new string[] { "Tables/Table_TopLeft", "Tables/Table_Middle", "Tables/Table_TopRight" };
            string[] artifactObjectNames = new string[] { "Artifact_OfficerRevolver", "Artifact_CeremonialBowl", "Artifact_KatipunanBolo" };
            string[] names  = new string[] { "Katipunan Officer's Revolver", "Katipunan Sandugo Bowl", "Katipunero Combat Bolo" };
            Sprite[] sprites = new Sprite[] { sprRevolver, sprBowl, sprBolo };
            string[] plaqueTexts = new string[]
            {
                "A six-shot service revolver carried by high-ranking revolutionary officers during pivotal battles of the Philippine Revolution.",
                "A ceremonial brass bowl used during clandestine Katipunan initiation ceremonies to perform the solemn blood compact (Sandugo).",
                "A formidable single-edged combat bolo forged by native pandays, wielded by Katipunan freedom fighters in close-quarters liberation battles."
            };
            string[] inspectTexts = new string[]
            {
                "Blued steel frame fitted with polished walnut grips. The cylinder rotates smoothly, and faint patriotic insigne are stamped into the sideplate.",
                "Hammered from heavy solid brass with a flared lip. The polished interior retains subtle patina marks from ceremonial quill dips and wax seals.",
                "Hand-forged tempered leaf spring steel with an aggressive clip-point tip and carved carabao horn handle. Battle notches along the spine testify to historical combat."
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
            SetupRoom4Artifacts();
            SetupRoom5Artifacts();
            SetupRoom6Artifacts();
            SetupRoom7Artifacts();
            SetupRoom8Artifacts();
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
            choiceRt.sizeDelta = new Vector2(360f, 140f);

            var btnArtifactObj = CreateChoiceButton("InspectArtifactButton", "inspect artifact", new Vector2(0f, 38f), choiceBox, plaqueSprite);
            var btnPlaqueObj   = CreateChoiceButton("InspectPlaqueButton",   "inspect plaque",   new Vector2(0f, -38f), choiceBox, plaqueSprite);

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
            for (int i = 0; i < tablePaths.Length; i++)
            {
                var tableObj = GameObject.Find(tablePaths[i]);
                if (tableObj == null) continue;

                var interactable = tableObj.GetComponent<ArtifactInteractable>();
                if (interactable == null) interactable = tableObj.AddComponent<ArtifactInteractable>();

                interactable.Configure(
                    names[i],
                    sprites[i],
                    "...",
                    plaqueTexts[i],
                    inspectTexts[i],
                    "what should i do",
                    "what could this artifact be",
                    "i wonder what this artifact looks like"
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

            Material spriteLitMat = null;
            var firstTable = GameObject.Find(tablePaths[0]);
            if (firstTable != null)
            {
                var sr = firstTable.GetComponent<SpriteRenderer>();
                if (sr != null) spriteLitMat = sr.sharedMaterial;
            }

            for (int i = 0; i < tablePaths.Length; i++)
            {
                var tableObj = GameObject.Find(tablePaths[i]);
                if (tableObj == null) continue;

                var tableSr = tableObj.GetComponent<SpriteRenderer>();

                var artObj = new GameObject(artifactObjectNames[i], typeof(SpriteRenderer), typeof(TableItemSort));
                artObj.transform.SetParent(root.transform, false);
                artObj.transform.position = new Vector3(tableObj.transform.position.x, tableObj.transform.position.y + 0.42f, 0f);
                artObj.transform.localScale = new Vector3(0.13f, 0.13f, 1f);

                var sr = artObj.GetComponent<SpriteRenderer>();
                sr.sprite = sprites[i];
                sr.color = Color.white;
                if (spriteLitMat != null) sr.sharedMaterial = spriteLitMat;
                sr.sortingLayerName = "Default";
                sr.sortingOrder = 2;

                var tis = artObj.GetComponent<TableItemSort>();
                if (tis != null && tableSr != null)
                {
                    tis.SetParentTable(tableSr);
                    EditorUtility.SetDirty(tis);
                }

                // Add 2D Point Light for glow effect
                var glowObj = new GameObject("Glow");
                glowObj.transform.SetParent(artObj.transform, false);
                glowObj.transform.localPosition = Vector3.zero;

                var light = glowObj.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
                light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point;
                light.color = new Color(1.0f, 0.95f, 0.75f, 1.0f);
                light.intensity = 1.2f;
                light.pointLightOuterRadius = 1.8f;
                light.pointLightInnerRadius = 0.2f;
                light.shadowsEnabled = true;
                EditorUtility.SetDirty(light);
            }

            EditorUtility.SetDirty(root);
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
