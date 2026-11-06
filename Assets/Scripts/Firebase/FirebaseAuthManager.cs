using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TopDownGame
{
    public class FirebaseAuthManager : MonoBehaviour
    {
        private static FirebaseAuthManager _instance;
        public static FirebaseAuthManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("FirebaseAuthManager");
                    _instance = go.AddComponent<FirebaseAuthManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public string UserId { get; private set; } = "";
        public string IdToken { get; private set; } = "";
        public string Email { get; private set; } = "";
        public string Username { get; private set; } = "";
        public bool IsLoggedIn => !string.IsNullOrEmpty(UserId);
        public bool IsGuest => !string.IsNullOrEmpty(UserId) && UserId.StartsWith("guest_");
        public bool IsGoogleUser => !string.IsNullOrEmpty(UserId) && !UserId.StartsWith("guest_");

        public void UnbindGoogle()
        {
            string guestId = "guest_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            UserId = guestId;
            Email = $"{guestId}@pamana.game";
            Username = "Guest";
            IdToken = "";
            PlayerPrefs.SetString("GuestUserId", guestId);
            PlayerPrefs.SetString("PlayerUsername", Username);
            PlayerPrefs.Save();
            Debug.Log($"[FirebaseAuthManager] Google account unbound. Session reverted to guest ({guestId}).");
        }

        private Action<bool, string> _pendingGoogleCallback;
        private readonly ConcurrentQueue<Action> _mainThreadActions = new ConcurrentQueue<Action>();

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void FirebaseWebGL_Init(string configJson);

        [DllImport("__Internal")]
        private static extern void FirebaseWebGL_GoogleSignIn();
#endif

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitFirebaseWebGL();
        }

        private void Update()
        {
            while (_mainThreadActions.TryDequeue(out var action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[FirebaseAuthManager] MainThreadAction error: {ex}");
                }
            }
        }

        private void OnDestroy()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            StopBrowserAuthListener();
#endif
        }

        private void InitFirebaseWebGL()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                var config = new FirebaseWebConfigJson
                {
                    apiKey = FirebaseConfig.ApiKey,
                    authDomain = FirebaseConfig.AuthDomain,
                    databaseURL = FirebaseConfig.DatabaseUrl,
                    projectId = FirebaseConfig.ProjectId,
                    appId = FirebaseConfig.AppId
                };
                string json = JsonUtility.ToJson(config);
                FirebaseWebGL_Init(json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[FirebaseAuthManager] WebGL Init error: {ex.Message}");
            }
#endif
        }

        public void Register(string emailOrUsername, string password, Action<bool, string> onComplete)
        {
            if (string.IsNullOrWhiteSpace(emailOrUsername))
            {
                onComplete?.Invoke(false, "Please enter a username or email.");
                return;
            }

            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                onComplete?.Invoke(false, "Password must be at least 6 characters.");
                return;
            }

            string formattedEmail = FirebaseConfig.FormatEmail(emailOrUsername);
            string chosenUsername = FirebaseConfig.ExtractUsername(emailOrUsername);

            StartCoroutine(PostAuthCoroutine(FirebaseConfig.SignUpUrl, formattedEmail, password, (success, errorMsg) =>
            {
                if (success)
                {
                    Username = chosenUsername;
                    PlayerPrefs.SetString("PlayerUsername", Username);
                    PlayerPrefs.Save();
                }
                onComplete?.Invoke(success, errorMsg);
            }));
        }

        public void Login(string emailOrUsername, string password, Action<bool, string> onComplete)
        {
            if (string.IsNullOrWhiteSpace(emailOrUsername))
            {
                onComplete?.Invoke(false, "Please enter a username or email.");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                onComplete?.Invoke(false, "Please enter your password.");
                return;
            }

            string formattedEmail = FirebaseConfig.FormatEmail(emailOrUsername);
            string chosenUsername = FirebaseConfig.ExtractUsername(emailOrUsername);

            StartCoroutine(PostAuthCoroutine(FirebaseConfig.SignInUrl, formattedEmail, password, (success, errorMsg) =>
            {
                if (success)
                {
                    Username = chosenUsername;
                    PlayerPrefs.SetString("PlayerUsername", Username);
                    PlayerPrefs.Save();
                }
                onComplete?.Invoke(success, errorMsg);
            }));
        }

        public void ResetPassword(string emailOrUsername, Action<bool, string> onComplete)
        {
            if (string.IsNullOrWhiteSpace(emailOrUsername))
            {
                onComplete?.Invoke(false, "Please enter your email or username.");
                return;
            }

            string email = FirebaseConfig.FormatEmail(emailOrUsername);
            var req = new ResetPasswordRequest { requestType = "PASSWORD_RESET", email = email };
            string bodyJson = JsonUtility.ToJson(req);

            StartCoroutine(SendPostRequest(FirebaseConfig.ResetPasswordUrl, bodyJson, (success, resp) =>
            {
                if (success)
                {
                    onComplete?.Invoke(true, "Password reset email sent! Check your inbox.");
                }
                else
                {
                    string friendly = ParseFirebaseError(resp);
                    onComplete?.Invoke(false, friendly);
                }
            }));
        }

        public void GoogleSignIn(Action<bool, string> onComplete)
        {
            _pendingGoogleCallback = onComplete;

#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                FirebaseWebGL_GoogleSignIn();
            }
            catch (Exception ex)
            {
                onComplete?.Invoke(false, $"Google sign-in error: {ex.Message}");
            }
#else
            StartGoogleBrowserAuth();
#endif
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        private HttpListener _browserAuthListener;
        private CancellationTokenSource _browserAuthCts;

        private void StopBrowserAuthListener()
        {
            try
            {
                if (_browserAuthCts != null)
                {
                    _browserAuthCts.Cancel();
                    _browserAuthCts.Dispose();
                    _browserAuthCts = null;
                }
                if (_browserAuthListener != null)
                {
                    if (_browserAuthListener.IsListening)
                    {
                        _browserAuthListener.Stop();
                    }
                    _browserAuthListener.Close();
                    _browserAuthListener = null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[FirebaseAuthManager] StopListener warning: {ex.Message}");
            }
        }

        private void StartGoogleBrowserAuth()
        {
            StopBrowserAuthListener();

            int port = 52424;
            try
            {
                var socket = new TcpListener(IPAddress.Loopback, 0);
                socket.Start();
                port = ((IPEndPoint)socket.LocalEndpoint).Port;
                socket.Stop();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[FirebaseAuthManager] Port lookup fallback: {ex.Message}");
                port = 52424;
            }

            try
            {
                _browserAuthListener = new HttpListener();
                _browserAuthListener.Prefixes.Add($"http://localhost:{port}/");
                _browserAuthListener.Start();
                _browserAuthCts = new CancellationTokenSource();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAuthManager] Failed to start local auth listener: {ex.Message}");
                _pendingGoogleCallback?.Invoke(false, $"Could not open local auth server: {ex.Message}");
                _pendingGoogleCallback = null;
                return;
            }

            string authUrl = $"http://localhost:{port}/";
            Debug.Log($"[FirebaseAuthManager] Launching browser for Google Sign-In: {authUrl}");
            Application.OpenURL(authUrl);

            var token = _browserAuthCts.Token;
            Task.Run(async () =>
            {
                try
                {
                    while (!token.IsCancellationRequested && _browserAuthListener != null && _browserAuthListener.IsListening)
                    {
                        var contextTask = _browserAuthListener.GetContextAsync();
                        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(180), token);

                        var completed = await Task.WhenAny(contextTask, timeoutTask);
                        if (completed == timeoutTask)
                        {
                            if (!token.IsCancellationRequested)
                            {
                                _mainThreadActions.Enqueue(() =>
                                {
                                    OnGoogleSignInFailure("Google sign-in timed out. Please try again.");
                                });
                            }
                            break;
                        }

                        var context = await contextTask;
                        var request = context.Request;
                        var response = context.Response;

                        if (request.HttpMethod == "GET" && (request.Url.AbsolutePath == "/" || request.Url.AbsolutePath == "/index.html"))
                        {
                            string html = GenerateAuthHtml(port);
                            byte[] buffer = Encoding.UTF8.GetBytes(html);
                            response.ContentType = "text/html; charset=utf-8";
                            response.ContentLength64 = buffer.Length;
                            response.StatusCode = 200;
                            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                            response.OutputStream.Close();
                        }
                        else if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/auth-callback")
                        {
                            string jsonPayload = "";
                            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8))
                            {
                                jsonPayload = await reader.ReadToEndAsync();
                            }

                            byte[] buffer = Encoding.UTF8.GetBytes("{\"status\":\"ok\"}");
                            response.ContentType = "application/json";
                            response.ContentLength64 = buffer.Length;
                            response.StatusCode = 200;
                            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                            response.OutputStream.Close();

                            _mainThreadActions.Enqueue(() =>
                            {
                                OnGoogleSignInSuccess(jsonPayload);
                            });
                            break;
                        }
                        else if (request.Url.AbsolutePath == "/auth-cancel")
                        {
                            byte[] buffer = Encoding.UTF8.GetBytes("{\"status\":\"cancelled\"}");
                            response.ContentType = "application/json";
                            response.ContentLength64 = buffer.Length;
                            response.StatusCode = 200;
                            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                            response.OutputStream.Close();

                            _mainThreadActions.Enqueue(() =>
                            {
                                OnGoogleSignInFailure("Google sign-in was cancelled by user.");
                            });
                            break;
                        }
                        else
                        {
                            response.StatusCode = 404;
                            response.OutputStream.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!token.IsCancellationRequested)
                    {
                        Debug.LogWarning($"[FirebaseAuthManager] Auth server error: {ex.Message}");
                    }
                }
                finally
                {
                    StopBrowserAuthListener();
                }
            }, token);
        }

        private static string GenerateAuthHtml(int port)
        {
            const string htmlTemplate = @"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>PAMANA - Sign In with Google</title>
  <script src=""https://www.gstatic.com/firebasejs/10.12.0/firebase-app-compat.js""></script>
  <script src=""https://www.gstatic.com/firebasejs/10.12.0/firebase-auth-compat.js""></script>
  <style>
    body {
      margin: 0;
      padding: 0;
      background: radial-gradient(circle at center, #2e1d12 0%, #120b07 100%);
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      color: #f7eed8;
    }
    .card {
      background: #25180f;
      border: 3px solid #b8864a;
      border-radius: 16px;
      box-shadow: 0 10px 35px rgba(0,0,0,0.8), inset 0 0 20px rgba(184, 134, 74, 0.15);
      padding: 36px 40px;
      width: 440px;
      max-width: 90%;
      text-align: center;
      box-sizing: border-box;
    }
    h1 {
      margin: 0 0 6px 0;
      font-size: 26px;
      color: #ffde8a;
      letter-spacing: 2px;
      text-transform: uppercase;
      text-shadow: 0 2px 4px rgba(0,0,0,0.6);
    }
    p.sub {
      margin: 0 0 24px 0;
      font-size: 14px;
      color: #d1b894;
    }
    .btn-google {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 12px;
      background: #ffffff;
      color: #3c4043;
      border: 2px solid #b8864a;
      border-radius: 8px;
      padding: 12px 20px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      width: 100%;
      transition: all 0.2s ease;
      box-shadow: 0 4px 12px rgba(0,0,0,0.3);
      box-sizing: border-box;
    }
    .btn-google:hover {
      background: #f8f9fa;
      transform: translateY(-2px);
      box-shadow: 0 6px 16px rgba(255, 222, 138, 0.35);
      border-color: #ffd700;
    }
    .google-icon {
      width: 24px;
      height: 24px;
    }
    div#status-box {
      margin-top: 22px;
      padding: 12px 14px;
      background: rgba(0,0,0,0.35);
      border-radius: 8px;
      font-size: 14px;
      line-height: 1.5;
      min-height: 48px;
      display: flex;
      align-items: center;
      justify-content: center;
      color: #ffde8a;
      border: 1px dashed #8c683c;
    }
    .cancel-link {
      display: inline-block;
      margin-top: 18px;
      font-size: 13px;
      color: #a88b68;
      text-decoration: underline;
      cursor: pointer;
    }
    .cancel-link:hover {
      color: #ffde8a;
    }
    .spinner {
      display: inline-block;
      width: 16px;
      height: 16px;
      border: 2px solid #b8864a;
      border-top-color: #ffd700;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
      margin-right: 8px;
    }
    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  </style>
</head>
<body>
  <div class=""card"">
    <h1>PAMANA</h1>
    <p class=""sub"">Philippine History & Heritage</p>
    <button id=""googleBtn"" class=""btn-google"" onclick=""triggerGoogleSignIn()"">
      <svg class=""google-icon"" viewBox=""0 0 48 48"">
        <path fill=""#EA4335"" d=""M24 9.5c3.54 0 6.71 1.22 9.21 3.6l6.85-6.85C35.9 2.38 30.47 0 24 0 14.62 0 6.51 5.38 2.56 13.22l7.98 6.19C12.43 13.72 17.74 9.5 24 9.5z""/>
        <path fill=""#4285F4"" d=""M46.98 24.55c0-1.57-.15-3.09-.38-4.55H24v9.02h12.94c-.58 2.96-2.26 5.48-4.78 7.18l7.73 6c4.51-4.18 7.09-10.36 7.09-17.65z""/>
        <path fill=""#FBBC05"" d=""M10.53 28.59c-.48-1.45-.76-2.99-.76-4.59s.27-3.14.76-4.59l-7.98-6.19C.92 16.46 0 20.12 0 24c0 3.88.92 7.54 2.56 10.78l7.97-6.19z""/>
        <path fill=""#34A853"" d=""M24 48c6.48 0 11.93-2.13 15.89-5.81l-7.73-6c-2.15 1.45-4.92 2.3-8.16 2.3-6.26 0-11.57-4.22-13.47-9.91l-7.98 6.19C6.51 42.62 14.62 48 24 48z""/>
      </svg>
      <span>Sign In with Google</span>
    </button>
    <div id=""status-box""><span class=""spinner""></span> Opening Google Sign-In...</div>
    <div><a class=""cancel-link"" onclick=""cancelAuth()"">Cancel and return to game</a></div>
  </div>

  <script>
    const firebaseConfig = {
      apiKey: '{{API_KEY}}',
      authDomain: '{{AUTH_DOMAIN}}',
      databaseURL: '{{DATABASE_URL}}',
      projectId: '{{PROJECT_ID}}',
      appId: '{{APP_ID}}'
    };
    firebase.initializeApp(firebaseConfig);
    const auth = firebase.auth();

    async function triggerGoogleSignIn() {
      const statusBox = document.getElementById('status-box');
      const googleBtn = document.getElementById('googleBtn');
      statusBox.innerHTML = '<span class=""spinner""></span> Waiting for Google sign-in...';
      try {
        const provider = new firebase.auth.GoogleAuthProvider();
        provider.addScope('email');
        provider.addScope('profile');
        provider.setCustomParameters({ prompt: 'select_account' });
        const result = await auth.signInWithPopup(provider);
        const user = result.user;
        statusBox.innerHTML = '<span class=""spinner""></span> Connecting to PAMANA game...';
        let idToken = '';
        try {
          idToken = await user.getIdToken();
        } catch(e) {}
        const payload = {
          uid: user.uid,
          email: user.email || '',
          displayName: user.displayName || '',
          idToken: idToken
        };
        await fetch('/auth-callback', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(payload)
        });
        statusBox.innerHTML = '<span style=""color:#77e077;font-weight:bold;"">✓ Successfully Connected!</span><br>Welcome, <b>' + (user.displayName || user.email) + '</b>!<br><br>You can now return to PAMANA.';
        googleBtn.style.display = 'none';
        setTimeout(() => { window.close(); }, 2500);
      } catch (err) {
        console.error(err);
        if (err.code === 'auth/popup-blocked') {
          statusBox.innerHTML = 'Popup was blocked by your browser.<br><b>Please click the ""Sign In with Google"" button above.</b>';
        } else if (err.code === 'auth/popup-closed-by-user') {
          statusBox.innerText = 'Sign-in window closed. Click the button above to try again.';
        } else {
          statusBox.innerText = 'Error: ' + (err.message || err);
        }
      }
    }

    function cancelAuth() {
      fetch('/auth-cancel').finally(() => {
        document.getElementById('status-box').innerText = 'Cancelled. You can close this window.';
        setTimeout(() => { window.close(); }, 800);
      });
    }

    window.addEventListener('load', () => {
      setTimeout(triggerGoogleSignIn, 300);
    });
  </script>
</body>
</html>";

            return htmlTemplate
                .Replace("{{API_KEY}}", FirebaseConfig.ApiKey)
                .Replace("{{AUTH_DOMAIN}}", FirebaseConfig.AuthDomain)
                .Replace("{{DATABASE_URL}}", FirebaseConfig.DatabaseUrl)
                .Replace("{{PROJECT_ID}}", FirebaseConfig.ProjectId)
                .Replace("{{APP_ID}}", FirebaseConfig.AppId)
                .Replace("{{PORT}}", port.ToString());
        }
#endif

        // Called via Unity SendMessage from FirebaseWebGL.jslib
        public void OnGoogleSignInSuccess(string json)
        {
            Debug.Log($"[FirebaseAuthManager] Google Sign-In Success: {json}");
            try
            {
                var data = JsonUtility.FromJson<GoogleSignInResultJson>(json);
                UserId = data.uid;
                Email = data.email;
                Username = !string.IsNullOrEmpty(data.displayName) ? data.displayName : FirebaseConfig.ExtractUsername(data.email);
                IdToken = data.idToken;

                PlayerPrefs.SetString("PlayerUsername", Username);
                PlayerPrefs.Save();

                _pendingGoogleCallback?.Invoke(true, "Google sign-in successful!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAuthManager] Parse error on Google sign-in: {ex.Message}");
                _pendingGoogleCallback?.Invoke(false, "Failed to parse Google sign-in response.");
            }
            finally
            {
                _pendingGoogleCallback = null;
            }
        }

        // Called via Unity SendMessage from FirebaseWebGL.jslib
        public void OnGoogleSignInFailure(string error)
        {
            Debug.LogWarning($"[FirebaseAuthManager] Google Sign-In Failure: {error}");
            _pendingGoogleCallback?.Invoke(false, error ?? "Google sign-in was cancelled or failed.");
            _pendingGoogleCallback = null;
        }

        public void SignOut()
        {
            UserId = "";
            IdToken = "";
            Email = "";
            Username = "";
            if (FirebaseDatabaseManager.Instance != null)
            {
                FirebaseDatabaseManager.Instance.ClearAllLocalGameData();
            }
        }

        public void GuestSignIn(Action<bool, string> onComplete)
        {
            string guestId = PlayerPrefs.GetString("GuestUserId", "");
            if (string.IsNullOrEmpty(guestId))
            {
                guestId = "guest_" + System.Guid.NewGuid().ToString("N").Substring(0, 8);
                PlayerPrefs.SetString("GuestUserId", guestId);
                PlayerPrefs.Save();
            }

            UserId = guestId;
            Email = $"{guestId}@pamana.game";
            Username = "Guest";
            IdToken = "";

            PlayerPrefs.SetString("PlayerUsername", Username);
            PlayerPrefs.Save();

            Debug.Log($"[FirebaseAuthManager] Signed in as Guest: {UserId}");
            onComplete?.Invoke(true, "Playing as Guest!");
        }

        private IEnumerator PostAuthCoroutine(string url, string email, string password, Action<bool, string> callback)
        {
            var req = new AuthRequestJson
            {
                email = email,
                password = password,
                returnSecureToken = true
            };
            string json = JsonUtility.ToJson(req);

            yield return SendPostRequest(url, json, (success, response) =>
            {
                if (success)
                {
                    try
                    {
                        var res = JsonUtility.FromJson<AuthResponseJson>(response);
                        UserId = res.localId;
                        IdToken = res.idToken;
                        Email = res.email;
                        callback?.Invoke(true, "Success!");
                    }
                    catch (Exception ex)
                    {
                        callback?.Invoke(false, $"Response parse error: {ex.Message}");
                    }
                }
                else
                {
                    string friendly = ParseFirebaseError(response);
                    callback?.Invoke(false, friendly);
                }
            });
        }

        private IEnumerator SendPostRequest(string url, string jsonBody, Action<bool, string> callback)
        {
            using (var uwr = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
                uwr.downloadHandler = new DownloadHandlerBuffer();
                uwr.SetRequestHeader("Content-Type", "application/json");

                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(true, uwr.downloadHandler.text);
                }
                else
                {
                    string errText = uwr.downloadHandler != null ? uwr.downloadHandler.text : uwr.error;
                    callback?.Invoke(false, errText);
                }
            }
        }

        private string ParseFirebaseError(string rawJson)
        {
            if (string.IsNullOrEmpty(rawJson)) return "Network error. Please check your internet connection.";

            if (rawJson.Contains("EMAIL_EXISTS")) return "That username or email is already registered.";
            if (rawJson.Contains("EMAIL_NOT_FOUND") || rawJson.Contains("USER_NOT_FOUND")) return "User not found. Please register first.";
            if (rawJson.Contains("INVALID_LOGIN_CREDENTIALS") || rawJson.Contains("INVALID_PASSWORD")) return "Incorrect password. Please try again.";
            if (rawJson.Contains("WEAK_PASSWORD")) return "Password must be at least 6 characters.";
            if (rawJson.Contains("USER_DISABLED")) return "This account has been disabled.";
            if (rawJson.Contains("TOO_MANY_ATTEMPTS_TRY_LATER")) return "Too many failed attempts. Please try again later.";

            return "Authentication failed. Please verify your details.";
        }

        [Serializable]
        private class AuthRequestJson
        {
            public string email;
            public string password;
            public bool returnSecureToken;
        }

        [Serializable]
        private class AuthResponseJson
        {
            public string localId;
            public string idToken;
            public string email;
            public string refreshToken;
            public string expiresIn;
        }

        [Serializable]
        private class ResetPasswordRequest
        {
            public string requestType;
            public string email;
        }

        [Serializable]
        private class FirebaseWebConfigJson
        {
            public string apiKey;
            public string authDomain;
            public string databaseURL;
            public string projectId;
            public string appId;
        }

        [Serializable]
        private class GoogleSignInResultJson
        {
            public string uid;
            public string email;
            public string displayName;
            public string idToken;
        }
    }
}

