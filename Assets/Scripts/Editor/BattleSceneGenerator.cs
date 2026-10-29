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
        [MenuItem("Tools/Setup/Generate All Battle Scenes (Rooms 1 to 6) and Redirect Alterworld")]
        public static void GenerateAndRedirectAll()
        {
            GenerateBattleScene(); // Room 1
            GenerateBattleScenesForRooms2To6();
            RedirectAlterworldRooms();
            UpdateBuildSettings();
            Debug.Log("[BattleSceneGenerator] Successfully generated all 6 Alter World Battle Scenes and updated hallway doors!");
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
                        artifactDescription = "Annotated proof sheets from the original print run of Noli Me Tángere, funded by Dr. Maximo Viola in Berlin.",
                        questionText = "Who was the generous friend who provided Rizal the financial funds needed to print the first edition of Noli Me Tangere in Berlin?",
                        choices = new string[] {
                            "A. Maximo Viola",
                            "B. Valentin Ventura",
                            "C. Ferdinand Blumentritt",
                            "D. Juan Luna"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room2_FeatherQuillPen",
                        artifactName = "Feather Quill Pen",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_FeatherQuillPen.png"),
                        artifactDescription = "A fine writing feather quill used to author the characters and poignant tragedies of Noli Me Tángere.",
                        questionText = "Which character in Noli Me Tangere symbolized the tragic plight of an oppressed Filipino mother driven to insanity?",
                        choices = new string[] {
                            "A. Sisa",
                            "B. Maria Clara",
                            "C. Doña Victorina",
                            "D. Tiya Isabel"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room2_PrintingPressMatrix",
                        artifactName = "Printing Press Matrix",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_PrintingPressMatrix.png"),
                        artifactDescription = "A movable type wooden press bed used to publish the first run of El Filibusterismo in Ghent.",
                        questionText = "In which European city was Jose Rizal's second novel, El Filibusterismo, published in September 1891?",
                        choices = new string[] {
                            "A. Ghent, Belgium",
                            "B. Berlin, Germany",
                            "C. Madrid, Spain",
                            "D. Paris, France"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room2_FirstEditionNovelCopy",
                        artifactName = "First Edition Novel Copy",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_FirstEditionNovelCopy.png"),
                        artifactDescription = "An original 1891 first edition volume of El Filibusterismo, saved by Valentin Ventura's urgent financial loan.",
                        questionText = "Who provided the crucial financial loan that rescued the printing of El Filibusterismo from being suspended?",
                        choices = new string[] {
                            "A. Valentin Ventura",
                            "B. Maximo Viola",
                            "C. Graciano Lopez Jaena",
                            "D. Marcelo del Pilar"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room2_AlcoholCookingStove",
                        artifactName = "Alcohol Cooking Stove",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_AlcoholCookingStove.png"),
                        artifactDescription = "A small brass alcohol burner used by Rizal in Fort Santiago, inside which he hid the original manuscript of Mi Último Adiós.",
                        questionText = "Inside which everyday household object did Rizal hide the folded manuscript of his farewell poem, Mi Último Adiós?",
                        choices = new string[] {
                            "A. Alcohol cooking stove",
                            "B. Leather boot heel",
                            "C. Hollow walking cane",
                            "D. Bible book spine"
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
                        artifactId = "Room3_NoliManuscript",
                        artifactName = "Manuscript of Noli Me Tángere",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_OpenBook.png"),
                        artifactDescription = "José Rizal's original handwritten manuscript of Noli Me Tángere, completed in Berlin in 1887 to expose colonial oppression.",
                        questionText = "In which European capital was José Rizal's first novel, \"Noli Me Tángere\", printed in 1887 with the aid of Maximo Viola?",
                        choices = new string[] {
                            "A. Berlin, Germany",
                            "B. Ghent, Belgium",
                            "C. Madrid, Spain",
                            "D. Paris, France"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room3_FiliFirstEdition",
                        artifactName = "First Edition of El Filibusterismo",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_BoundNovel.png"),
                        artifactDescription = "A rare first edition copy of El Filibusterismo, published in Ghent in 1891 and dedicated to the martyrdom of Gomburza.",
                        questionText = "To whose sacred memory did José Rizal solemnly dedicate his second novel, \"El Filibusterismo\"?",
                        choices = new string[] {
                            "A. The martyred priests GOMBURZA",
                            "B. His mother, Teodora Alonso",
                            "C. The Katipunan secret society",
                            "D. Dr. Ferdinand Blumentritt"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room3_UltimoAdios",
                        artifactName = "Original Draft of Mi Último Adiós",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_ManuscriptSheet.png"),
                        artifactDescription = "The historic farewell poem composed by José Rizal on the eve of his execution at Bagumbayan, December 1896.",
                        questionText = "Where did Rizal conceal his final untitled farewell poem before handing it to his family prior to his execution?",
                        choices = new string[] {
                            "A. Inside an alcohol cooking lamp",
                            "B. Sewn into his overcoat lining",
                            "C. Hidden in his leather boots",
                            "D. Concealed within his pocket watch"
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
                        artifactId = "Room4_RevolutionaryDecree",
                        artifactName = "Revolutionary Draft Decree",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Document.png"),
                        artifactDescription = "A handwritten draft of a historical decree laying out principles of sovereignty and governance during the revolution.",
                        questionText = "Who was revered as the \"Brains of the Revolution\" and served as the chief legal adviser of the revolutionary government?",
                        choices = new string[] {
                            "A. Apolinario Mabini",
                            "B. Emilio Jacinto",
                            "C. Felipe Calderon",
                            "D. Pedro Paterno"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room4_BrassInkwell",
                        artifactName = "Scholar's Brass Inkwell",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Inkwell.png"),
                        artifactDescription = "A weighted traveling inkpot used by 19th-century Filipino intellectuals to hold indelible iron gall ink during secret correspondence.",
                        questionText = "What historic decree did General Emilio Aguinaldo issue on June 23, 1898, replacing the Dictatorial Government?",
                        choices = new string[] {
                            "A. The Revolutionary Government Decree",
                            "B. The Malolos Accord",
                            "C. The Biak-na-Bato Constitution",
                            "D. The Treaty of Paris"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room4_DeclarationParchment",
                        artifactName = "Declaration of Sovereignty",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Parchment.png"),
                        artifactDescription = "A ceremonial parchment proclamation drafted to herald national autonomy and unity among the archipelago's provinces.",
                        questionText = "In which historic town was the Philippine Declaration of Independence solemnly proclaimed on June 12, 1898?",
                        choices = new string[] {
                            "A. Kawit, Cavite",
                            "B. Malolos, Bulacan",
                            "C. Barasoain, Bulacan",
                            "D. Caloocan, Manila"
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
                        artifactId = "Room5_ApothecaryFlask",
                        artifactName = "Apothecary's Spherical Flask",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Flask.png"),
                        artifactDescription = "A thick-walled glass spherical flask used by 19th-century medical practitioners to distill antiseptics and essential herbal tinctures.",
                        questionText = "Under which eminent eye surgeons in Paris and Heidelberg did José Rizal master ophthalmic surgery?",
                        choices = new string[] {
                            "A. Dr. Louis de Wecker and Dr. Otto Becker",
                            "B. Dr. Rudolf Virchow and Dr. Meyer",
                            "C. Dr. Ferdinand Blumentritt",
                            "D. Dr. Miguel Morayta"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room5_ApothecaryJar",
                        artifactName = "Vintage Apothecary Jar",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Jar.png"),
                        artifactDescription = "A glazed ceramic apothecary jar designed to preserve healing balms, poultices, and powdered botanical remedies.",
                        questionText = "During his four-year exile in Dapitan (1892–1896), what vital public engineering feat did Rizal construct for the community?",
                        choices = new string[] {
                            "A. A community gravity waterworks system",
                            "B. A steamboat shipyard",
                            "C. A copper mining foundry",
                            "D. A telegraph transmitter station"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room5_SurgicalForceps",
                        artifactName = "Rizal's Surgical Forceps",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Scissors.png"),
                        artifactDescription = "Curved stainless surgical forceps preserved from historical Philippine medical clinics and ophthalmic procedures.",
                        questionText = "Upon which immediate family member did Dr. José Rizal successfully perform cataract surgery in Hong Kong and Calamba?",
                        choices = new string[] {
                            "A. His mother, Teodora Alonso",
                            "B. His brother, Paciano Rizal",
                            "C. His sister, Maria",
                            "D. His father, Francisco Mercado"
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
                        artifactId = "Room6_HardwoodGavel",
                        artifactName = "Magistrate's Hardwood Gavel",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Gavel.png"),
                        artifactDescription = "A ceremonial gavel and striking block carved from native molave hardwood, used in the early judicial courts of the Philippine archipelago.",
                        questionText = "Who was appointed as the very first Chief Justice of the Supreme Court of the Philippines in 1901?",
                        choices = new string[] {
                            "A. Cayetano Arellano",
                            "B. Florentino Torres",
                            "C. Victorino Mapa",
                            "D. Manuel Araullo"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room6_LawTome",
                        artifactName = "Codex of Philippine Jurisprudence",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_LawTome.png"),
                        artifactDescription = "A monumental bound compendium containing early civil statutes, decrees, and constitutional drafts from the First Philippine Republic.",
                        questionText = "Which historical basilica church hosted the revolutionary congress that drafted the Malolos Constitution in 1898-1899?",
                        choices = new string[] {
                            "A. Barasoain Church in Malolos, Bulacan",
                            "B. Manila Cathedral in Intramuros",
                            "C. San Agustin Church",
                            "D. Quiapo Church"
                        },
                        correctChoiceIndex = 0
                    },
                    new ArtifactBattleSlot
                    {
                        artifactId = "Room6_ReadingSpectacles",
                        artifactName = "Jurist's Reading Spectacles",
                        artifactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Spectacles.png"),
                        artifactDescription = "A pair of antique round reading spectacles once belonging to a prominent 19th-century Filipino jurist and scholar.",
                        questionText = "Who was the Filipino lawyer and statesman who authored the primary draft of the Malolos Constitution?",
                        choices = new string[] {
                            "A. Felipe Calderón y Roca",
                            "B. Apolinario Mabini",
                            "C. Pedro Paterno",
                            "D. Gregorio Araneta"
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
            var enemySpr = AssetDatabase.LoadAssetAtPath<Sprite>(monsterSpritePath);
            var avatarSpr = AssetDatabase.LoadAssetAtPath<Sprite>(monsterAvatarPath);

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
            Debug.Log($"[BattleSceneGenerator] Build Settings updated with {sceneList.Count} scenes (including BattleScene_2 to BattleScene_6)!");
        }
    }
}
