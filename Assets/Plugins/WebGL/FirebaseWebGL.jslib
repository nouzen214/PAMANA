mergeInto(LibraryManager.library, {
    FirebaseWebGL_Init: function (configJsonPtr) {
        var configJson = UTF8ToString(configJsonPtr);
        try {
            var config = JSON.parse(configJson);
            if (typeof firebase !== 'undefined' && !firebase.apps.length) {
                firebase.initializeApp(config);
                console.log("[FirebaseWebGL] Initialized Firebase Web SDK successfully.");
            }
        } catch (e) {
            console.error("[FirebaseWebGL] Init error:", e);
        }
    },

    FirebaseWebGL_GoogleSignIn: function () {
        function sendToUnity(method, param) {
            if (typeof unityInstance !== 'undefined' && unityInstance.SendMessage) {
                unityInstance.SendMessage("FirebaseAuthManager", method, param);
            } else if (typeof window.unityInstance !== 'undefined' && window.unityInstance.SendMessage) {
                window.unityInstance.SendMessage("FirebaseAuthManager", method, param);
            } else if (typeof SendMessage === 'function') {
                SendMessage("FirebaseAuthManager", method, param);
            } else {
                console.error("[FirebaseWebGL] Failed to send message to Unity:", method);
            }
        }

        try {
            if (typeof firebase === 'undefined') {
                console.error("[FirebaseWebGL] Firebase SDK is not loaded.");
                sendToUnity("OnGoogleSignInFailure", "Firebase Web SDK not loaded in browser.");
                return;
            }

            var provider = new firebase.auth.GoogleAuthProvider();
            provider.addScope('email');
            provider.addScope('profile');
            provider.setCustomParameters({ prompt: 'select_account' });

            firebase.auth().signInWithPopup(provider)
                .then(function (result) {
                    var user = result.user;
                    user.getIdToken().then(function (idToken) {
                        var payload = {
                            uid: user.uid,
                            email: user.email || "",
                            displayName: user.displayName || "",
                            idToken: idToken
                        };
                        var json = JSON.stringify(payload);
                        sendToUnity("OnGoogleSignInSuccess", json);
                    }).catch(function (tokenErr) {
                        var payload = {
                            uid: user.uid,
                            email: user.email || "",
                            displayName: user.displayName || "",
                            idToken: ""
                        };
                        sendToUnity("OnGoogleSignInSuccess", JSON.stringify(payload));
                    });
                })
                .catch(function (error) {
                    console.error("[FirebaseWebGL] Google sign-in failed:", error);
                    sendToUnity("OnGoogleSignInFailure", error.message || "Google sign-in was cancelled or failed.");
                });
        } catch (e) {
            console.error("[FirebaseWebGL] Exception during Google sign-in:", e);
            sendToUnity("OnGoogleSignInFailure", e.message || "Exception during Google sign-in.");
        }
    }
});

